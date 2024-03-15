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

//namespace RedBjorn.ProtoTiles.Example
//{
public class UnitMove : MonoBehaviour
    {
        public float Speed = 5;
        public float Range = 10f;
        public float RangeLeft;
        public Transform RotationNode;
        public AreaOutline AreaPrefab;
        public PathDrawer PathPrefab;

        //MapEntity mapManager.MapEntity;

        public MapManager mapManager;
        public UnitController unitController;
        public AreaOutline Area;
        PathDrawer Path;
        Coroutine MovingCoroutine;

        private bool active = false;
        private bool justActivated = false;
        
        public Vector3Int hexPosition;

        void Update()
        { 
            if(active && !justActivated) {
                if (MyInput.GetOnWorldUp(mapManager.MapEntity.Settings.Plane()))
                {
                    HandleWorldClick();
                }
                PathUpdate();
            }   
            if(active && justActivated) {
                if (Input.GetMouseButtonUp(0))
                {
                    justActivated = false;
                }
            }

        }

        public void Init(MapManager mapManager, UnitController unitController)
        {
            this.unitController = unitController;
            RangeLeft = Range;
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
            var clickPos = MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane());
            var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, RangeLeft);
            var tile = path.Last();
            if(tile == null) return;
            // attack
            if(tile.UnitPresent is not null && tile.UnitPresent.owner != this.unitController.owner && !this.unitController.attacked)
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
            else if(unitController.CanStackUnits(tile))
            {
                SubClass(tile, clickPos, false);
            }
        }

        private void SubClass(TileEntity tile,Vector3 clickPos,bool attackMove)
        {
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
                MovingCoroutine = StartCoroutine(Moving(path, onCompleted));
                var amountOfSteps = (int)Math.Ceiling((double)path.Count / 2);
                RangeLeft -= amountOfSteps;
            }
            else
            {

                    onCompleted.SafeInvoke();

                
            }
        }

        IEnumerator Moving(List<TileEntity> path, Action onCompleted)
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
                else if(mapManager.MapEntity.RotationType == RotationType.Flip)
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
            if (active==true)
            {
                onCompleted.SafeInvoke();
            }
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
            if (!Path)
            {
                Path = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
                Path.Show(new List<Vector3>() { }, mapManager.MapEntity);
                Path.InactiveState();
                Path.IsEnabled = true;
            }
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
                    var path = mapManager.MapEntity.PathPoints(transform.position, mapManager.MapEntity.WorldPosition(tile.Position), RangeLeft);
                    Path.Show(path, mapManager.MapEntity);
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
    }
//}
