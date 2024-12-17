using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class UnitMove : NetworkBehaviour
{
    public float Speed = 5;
    public float Range = 10f;
    public float RangeLeft;
    public Transform RotationNode;
    public AreaOutline AreaPrefab;
    public PathDrawer PathPrefab;   

    public MapManager mapManager;
    public UnitController unitController;
    public AreaOutline Area;
    PathDrawer LongPath;
    Coroutine MovingCoroutine;

    public bool active { get; private set; } = false;
    private bool justActivated = false;
    public bool isAutoMove = false;

    private TileEntity longPathTile;
    public Vector3 longPathClickPosition;
    List<Vector3> longPathPoints;

    public Vector3Int hexPosition;

    void Update()
    {
        if (active && !justActivated)
        {
            if (MyInput.GetOnWorldUp(mapManager.MapEntity.Settings.Plane()))
            {
                HandleWorldClick();
            }
            UpdateLongPath();
        }
        if (active && justActivated)
        {
            if (Input.GetMouseButtonUp(0))
            {
                justActivated = false;
            }
        } 
    }

    public void Init(MapManager mapManager, UnitController unitController, float? rangeLeft, Vector3? longPathClickPosition)
    {
        this.unitController = unitController;
        if (rangeLeft != null)
        {
            RangeLeft = (float)rangeLeft;
        }
        else
        {
            RangeLeft = Range;
        }
        if(longPathClickPosition != null && longPathClickPosition != Vector3.zero)
        {
            //load long path and auto move
            var path = mapManager.MapEntity.PathTiles(transform.position, (Vector3)longPathClickPosition, float.MaxValue);
            longPathTile = path[Math.Min((int)RangeLeft, path.Count - 1)];
            longPathPoints = path.Select(x => mapManager.MapEntity.WorldPosition(x)).ToList();
            longPathPoints.Add(mapManager.MapEntity.WorldPosition(longPathTile));
            isAutoMove = true;
        }
        var position = transform.position;
        var tile = mapManager.MapEntity.Tile(position);
        tile.UnitPresent = this.unitController;
        hexPosition = tile.Position;
        this.mapManager = mapManager;
        Area = Spawner.Spawn(AreaPrefab, Vector3.zero, Quaternion.identity);
        AreaHide();
    }

	// TODO: probably need changing when we implement multiple units on a tile
	void HandleWorldClick()
    {
        if (LongPath)
        {
            Destroy(LongPath.gameObject);
        }
        // manual movement (get click position from mouse position now)
        Vector3 clickPos = MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane());

        if (unitController.owner.gameManager.isMultiplayer)
        {
	        ClickedToMoveToPositionRpc(clickPos);
		}
        else
        {
	        ClickedToMoveToPosition(clickPos);
		}
	}

    [Rpc(SendTo.Everyone)]
    public void ClickedToMoveToPositionRpc(Vector3 clickPos)
    {
	    ClickedToMoveToPosition(clickPos);
    }

    public void ClickedToMoveToPosition(Vector3 clickPos)
    {
        TileEntity tile;
        bool isAttackMove = false;

        var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, float.MaxValue);
	    if (path.Count == 0)
	    {
            //clicked on impassable tile or enemy
            isAutoMove = false;
            tile = mapManager.MapEntity.Tile(clickPos);
            var currentlyOccupiedTile = mapManager.MapEntity.Tile(transform.position);
            if ((tile?.UnitPresent != this 
                 || (tile?.CityTilePresent != null && !unitController.owner.playerCitiesManager.Contains(tile?.CityTilePresent?.city))
                ) && mapManager.GetTilesSurroundingArea(new List<TileEntity>() { tile }, 1, false)
                .Contains(currentlyOccupiedTile))
            {
                //attack
                isAttackMove = true;
            }
            else
            {
                //impassable tile
                isAutoMove = false;
                return;
            }
	    }
        else
        {
            tile = path[Math.Min((int)RangeLeft, path.Count - 1)];
        }
        if (path.Count - 1 > RangeLeft)
	    {
		    // clicked out of range, set long path
		    longPathTile = path.Last();
		    longPathPoints = path.Select(x => mapManager.MapEntity.WorldPosition(x)).ToList();
		    longPathPoints.Add(mapManager.MapEntity.WorldPosition(tile));
		    longPathClickPosition = clickPos;
		    isAutoMove = true;
	    }
	    else
	    {
		    // no need for long path, tile is within reach
		    isAutoMove = false;
	    }

	    DetermineMoveType(tile, clickPos, isAttackMove);
	}

    private void DetermineMoveType(TileEntity tile, Vector3 clickPos, bool isAttackMove)
    {
        if (RangeLeft == 0)
        {
            // automatic movement is finished, could trigger some event here
            unitController.Deactivate();
            return;
        }
        else
        {
            // this is automatic movement at the end of turn
            var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, float.MaxValue);
            if (path.Count > 0)
            {
                tile = path[Math.Min((int)RangeLeft, path.Count - 1)];
            }
            else if(!isAttackMove)
            {
                tile = null;
            }
            HideLongPath();
        }
        if (tile == null) return;


        CityTile movedFromCityTile = mapManager.MapEntity.Tile(transform.position).CityTilePresent;
        bool attackedCity = false;

        // attack enemy city
        if (tile.CityTilePresent is not null && tile.CityTilePresent.city.Owner != unitController.owner)
        {
            attackedCity = true;
            Move(clickPos, true);
            this.unitController.Attack(tile.CityTilePresent.city);
        }
        // attack
        else if (tile.UnitPresent is not null && tile.UnitPresent.owner != this.unitController.owner && !this.unitController.attacked)
        {
            Move(clickPos, true);
            this.unitController.Attack(tile.UnitPresent);
        }
        // move to empty tile
        else if (tile.UnitPresent is null)
        {
            Move(clickPos, false);
        }
        // move to tile with own fort or city 
        else if (unitController.CanStackUnits(tile))
        {
            Move(clickPos, false);
        }

        if (movedFromCityTile is not null && (!tile.CityTilePresent || tile.CityTilePresent.city != movedFromCityTile.city))
        {
            //moved out of city
            movedFromCityTile.city.RemoveFromGarrison(unitController);
        }
        if (movedFromCityTile is null && tile.CityTilePresent && !attackedCity)
        {
            //moved into city
            tile.CityTilePresent.city.AddToGarrison(unitController);
        }
    }

    private void Move(Vector3 clickPos, bool attackMove)
    {
        List<TileEntity> path;
        if (!attackMove)
        {
            path = mapManager.MapEntity.PathTiles(transform.position, clickPos, RangeLeft);
        }
        else
        {
            path = mapManager.MapEntity.PathTilesNextTo(transform.position, clickPos, RangeLeft);
        }

        if (path.Count == 0 && attackMove)
        {
            path = new List<TileEntity>() { mapManager.MapEntity.Tile(transform.position) };
        }

        var numberOfSteps = (int)Math.Ceiling((double)path.Count / 2);
        RangeLeft -= numberOfSteps;

        TileEntity oldTile = mapManager.MapEntity.Tile(hexPosition);
        oldTile.UnitPresent = null;
        unitController.playerUnitsManager.ResetUnitPresentOnTile(oldTile, this.unitController);
        path.Last().UnitPresent = this.unitController;
        hexPosition = path.Last().Position;
        DestroyFortsOnTheWay(path);

        if (longPathPoints != null && longPathPoints.Count > numberOfSteps)
        {
            longPathPoints.RemoveRange(0, numberOfSteps);
        }
        if (longPathPoints != null && isAutoMove && LongPath != null)
        {
            LongPath.SetNumberOfTurns(CalculateLongPathNumberOfTurns(longPathPoints.Count - numberOfSteps));
        }
        if (RangeLeft == 0)
        {
            unitController.Deactivate();
        }

        Action onCompleted = () =>
        {
            CreateLongPath();
            ActivateLongPath();
            ShowLongPath();
            AreaShow();
        };

        DisplayMovement(path, onCompleted);
    }

    private void DisplayMovement(List<TileEntity> path, Action onCompleted) {
        this.mapManager.gameManager.soundManager.GetComponent<SoundManager>().PlayMoveSound(this.unitController);

        if (MovingCoroutine != null)
        {
            StopCoroutine(MovingCoroutine);
        }
        MovingCoroutine = StartCoroutine(MovingAnimation(path, onCompleted));
    }

    IEnumerator MovingAnimation(List<TileEntity> path, Action onCompleted)
    {
        var nextIndex = 0;
        transform.position = mapManager.MapEntity.Settings.Projection(transform.position);

        while (nextIndex < path.Count)
        {
            var targetPoint = mapManager.MapEntity.WorldPosition(path[nextIndex]);
            var stepDir = (targetPoint - transform.position) * Speed;
            if (mapManager.MapEntity.RotationType == RotationType.LookAt)
            {
                RotationNode.rotation = Quaternion.LookRotation(stepDir, Vector3.up);
            }
            else if (mapManager.MapEntity.RotationType == RotationType.Flip)
            {
                RotationNode.rotation = mapManager.MapEntity.Settings.Flip(stepDir);
            }
            var reached = stepDir.sqrMagnitude < 0.01f;
            while (!reached)
            {

                transform.position += stepDir * Time.deltaTime;
                reached = Vector3.Dot(stepDir, (targetPoint - transform.position)) < 0f;
                yield return null;
            }
            transform.position = targetPoint;
            nextIndex++;
        }
        if (active == true)
        {
            onCompleted.SafeInvoke();
        }
    }

    public void TryAutoMove()
    {
        if (!isAutoMove || longPathTile == null || longPathClickPosition == null) return;

        DetermineMoveType(longPathTile, longPathClickPosition, false);
    }

    private void DestroyFortsOnTheWay(List<TileEntity> tiles)
    {
        foreach (var tile in tiles)
        {
            if(tile.FortPresent && tile.FortPresent.owner != unitController.owner)
            {
                tile.FortPresent.Destroy(tile);
            }
        }
    }

    void AreaShow()
    {
        AreaHide();
        Area.Show(mapManager.MapEntity.WalkableBorder(transform.position, RangeLeft), mapManager.MapEntity);
    }

    public void UnitShow()
    {
        AreaHide();
        var tile = mapManager.MapEntity.WalkableBorder(transform.position,0);
        Area.Show(tile, mapManager.MapEntity);
    }

    public void AreaHide()
    {
        Area.Hide();
    }

    public void Activate()
    {
        isAutoMove = false;
        longPathClickPosition = Vector3.zero;
        longPathTile = null;
        longPathPoints = null;

        active = true;
        justActivated = true;
        AreaShow();
        CreateLongPath();
        ShowLongPath();
        LongPath.IsEnabled = true;
    }

    public void Deactivate()
    {
        active = false;
        justActivated = false;
        AreaHide();
        HideLongPath();
        if(LongPath)
        {
            LongPath.IsEnabled = false;
        }
    }

    public void ResetRange()
    {
        RangeLeft = Range;
    }
    private void CreateLongPath()
    {
        if(LongPath)
        {
            Destroy(LongPath.gameObject);
        }
        // could be changed to a different prefab
        LongPath = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
        LongPath.Show(new List<Vector3>() { }, mapManager.MapEntity);
        LongPath.Init(unitController.owner.color, unitController.owner.color, CalculateLongPathNumberOfTurns(longPathPoints != null ? longPathPoints.Count - 1 : 0));
        DeactivateLongPath();
    }

    public void ActivateLongPath()
    {
        LongPath.ActiveState();
        LongPath.IsEnabled = true;
    }

    public void DeactivateLongPath()
    {
        LongPath.InactiveState();
        LongPath.IsEnabled = false;
    }

    public void ShowLongPath()
    {
        if (LongPath == null)
        {
            CreateLongPath();
        }
        LongPath.Show(longPathPoints, mapManager.MapEntity);
    }

    public void HideLongPath()
    {
        if (LongPath)
        {
            LongPath.Hide();
        }
    }

    public void UpdateLongPath()
    {
        if (LongPath && LongPath.IsEnabled)
        {
            var tile = mapManager.MapEntity.Tile(MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane()));
            if (tile != null && tile.Vacant)
            {
                var path = mapManager.MapEntity.PathPoints(transform.position, mapManager.MapEntity.WorldPosition(tile.Position), float.MaxValue);
                int pathLength = path.Count - 1; // path length is the number of steps minus one (because the first step is the current position)
                int longPathNumberOfTurns = CalculateLongPathNumberOfTurns(pathLength);

                LongPath.Show(path, mapManager.MapEntity);
                LongPath.Init(unitController.owner.color, unitController.owner.color, longPathNumberOfTurns);
                LongPath.ActiveState();
                Area.ActiveState();
            }
            else
            {
                LongPath.InactiveState();
                Area.InactiveState();
            }
        }
    }

    private int CalculateLongPathNumberOfTurns(int pathLength)
    {
        return (int)Math.Ceiling(pathLength / (float)Range);
    }
}