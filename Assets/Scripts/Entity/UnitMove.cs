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
using UnityEngine.Tilemaps;

public class UnitMove : MonoBehaviour
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
    PathDrawer Path;
    PathDrawer LongPath;
    Coroutine MovingCoroutine;

    private bool active = false;
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
            PathUpdate();
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
        // manual movement (get click position from mouse position now)
        Destroy(LongPath);
        longPathPoints = null;
        Vector3 clickPos = MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane());
        var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, float.MaxValue);
        if(path.Count == 0)
        {
            //clicked on impassable tile
            isAutoMove = false;
            return;
        }
        TileEntity tile = path[Math.Min((int)RangeLeft, path.Count - 1)];
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

        DetermineMoveType(tile, clickPos);
    }

    private void DetermineMoveType(TileEntity tile, Vector3 clickPos)
    {
        if (RangeLeft == 0)
        {
            // automatic movement is finished, could trigger some event here
            return;
        }
        else
        {
            // this is automatic movement at the end of turn
            var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, float.MaxValue);
            tile = path[Math.Min((int)RangeLeft, path.Count - 1)];
            PathHide();
        }
        if (tile == null) return;


        CityTile movedFromCityTile = mapManager.MapEntity.Tile(transform.position).CityTilePresent;
        bool attackedCity = false;

        // attack enemy city
        if (tile.CityTilePresent is not null && tile.CityTilePresent.city.Owner != unitController.owner)
        {
            attackedCity = true;
            SubClass(tile, clickPos, true);
            this.unitController.Attack(tile.CityTilePresent.city);
        }
        // attack
        else if (tile.UnitPresent is not null && tile.UnitPresent.owner != this.unitController.owner && !this.unitController.attacked)
        {
            SubClass(tile, clickPos, true);
            this.unitController.Attack(tile.UnitPresent);
        }
        // move to empty tile
        else if (tile.UnitPresent is null)
        {
            SubClass(tile, clickPos, false);
        }
        // move to tile with own fort or city 
        else if (unitController.CanStackUnits(tile))
        {
            SubClass(tile, clickPos, false);
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

    private void SubClass(TileEntity tile, Vector3 clickPos, bool attackMove)
    {
        this.mapManager.gameManager.soundManager.GetComponent<SoundManager>().PlayMoveSound(this.unitController);
        TileEntity oldTile = mapManager.MapEntity.Tile(hexPosition);
        oldTile.UnitPresent = null;
        unitController.owner.ResetUnitPresentOnTile(oldTile, this.unitController);

        List<TileEntity> path;
        if (!attackMove)
        {
            path = mapManager.MapEntity.PathTiles(transform.position, clickPos, RangeLeft);
        }
        else
        {
            path = mapManager.MapEntity.PathTilesNextTo(transform.position, clickPos, RangeLeft);
        }

        path.Last().UnitPresent = this.unitController;
        hexPosition = path.Last().Position;
        AreaHide();
        Path.IsEnabled = false;
        PathHide();


        Move(path, () =>
        {
            Path.IsEnabled = true;
            AreaShow();
        });
    }

    public void Move(List<TileEntity> path, Action onCompleted)
    {
        if (path != null)
        {
            if (MovingCoroutine != null)
            {
                StopCoroutine(MovingCoroutine);
            }
            MovingCoroutine = StartCoroutine(MovingAnimation(path, onCompleted));
            var amountOfSteps = (int)Math.Ceiling((double)path.Count / 2);
            RangeLeft -= amountOfSteps;
            if(longPathPoints != null && longPathPoints.Count > amountOfSteps)
            {
                longPathPoints.RemoveRange(0, amountOfSteps);
            }
            if (longPathPoints != null && isAutoMove)
            {
                LongPath.SetNumberOfTurns(CalculateLongPathNumberOfTurns(longPathPoints.Count - amountOfSteps));
            }
            if (RangeLeft == 0)
            {
                unitController.Deactivate();
            }
        }
        else
        {
            onCompleted.SafeInvoke();
        }
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

        DetermineMoveType(longPathTile, longPathClickPosition);
    }

    void AreaShow()
    {
        AreaHide();
        Area.Show(mapManager.MapEntity.WalkableBorder(transform.position, RangeLeft), mapManager.MapEntity);
    }

    public void AreaHide()
    {
        Area.Hide();
    }

    void PathCreate()
    {
        if (Path)
        {
            Destroy(Path);
        }
        Path = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
        Path.Show(new List<Vector3>() { }, mapManager.MapEntity);
        Path.InactiveState();
        Path.IsEnabled = true;
    }

    public void PathHide()
    {
        if (Path)
        {
            Path.Hide();
        }
    }

    public void PathUpdate()
    {
        if (Path && Path.IsEnabled)
        {
            var tile = mapManager.MapEntity.Tile(MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane()));
            if (tile != null && tile.Vacant)
            {
                var path = mapManager.MapEntity.PathPoints(transform.position, mapManager.MapEntity.WorldPosition(tile.Position), float.MaxValue);
                int pathLength = path.Count - 1; // path length is the number of steps minus one (because the first step is the current position)
                int longPathNumberOfTurns = CalculateLongPathNumberOfTurns(pathLength); 

                Path.Show(path, mapManager.MapEntity);
                Path.Init(unitController.owner.color, unitController.owner.color, longPathNumberOfTurns);
                Path.ActiveState();
                Area.ActiveState();
            }
            else
            {
                Path.InactiveState();
                Area.InactiveState();
            }
        }
    }

    private int CalculateLongPathNumberOfTurns(int pathLength)
    {
        return (int)Math.Ceiling(pathLength / (float)Range);
    }

    public void Activate()
    {
        active = true;
        justActivated = true;
        AreaShow();
        PathCreate();
    }

    public void Deactivate()
    {
        active = false;
        justActivated = false;
        AreaHide();
        PathHide();
    }

    public void ResetRange()
    {
        RangeLeft = Range;
    }
    private void CreateLongPath()
    {
        // could be changed to a different prefab
        LongPath = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
        LongPath.Show(new List<Vector3>() { }, mapManager.MapEntity);
        LongPath.Init(unitController.owner.color, unitController.owner.color, CalculateLongPathNumberOfTurns(longPathPoints != null ? longPathPoints.Count - 1 : 0));
        LongPath.InactiveState();
        LongPath.IsEnabled = false;
    }

    public void ShowLongPath()
    {
        if(LongPath == null)
        {
            CreateLongPath();
        }
        LongPath.Show(longPathPoints, mapManager.MapEntity);
    }

    public void HideLongPath()
    {
        LongPath.Hide();
    }
}