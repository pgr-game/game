using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

//namespace RedBjorn.ProtoTiles.Example
//{
    public class UnitMove : MonoBehaviour
    {
        public float Speed = 5;
        public float Range = 10f;
        public Transform RotationNode;
        public AreaOutline AreaPrefab;
        public PathDrawer PathPrefab;

        //MapEntity mapManager.MapEntity;

        public MapManager mapManager;
        private AreaOutline Area;
        PathDrawer Path;
        Coroutine MovingCoroutine;

        private bool active = false;
        private bool justActivated = false;

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

        public void Init(MapManager mapManager)
        {
            this.mapManager = mapManager;
            Area = Spawner.Spawn(AreaPrefab, Vector3.zero, Quaternion.identity);
        }

        void HandleWorldClick()
        {
            var clickPos = MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane());
            var tile = mapManager.MapEntity.Tile(clickPos);
            if (tile != null && tile.Vacant)
            {
                AreaHide();
                Path.IsEnabled = false;
                PathHide();
                var path = mapManager.MapEntity.PathTiles(transform.position, clickPos, Range);
                Move(path, () =>
                {
                    Path.IsEnabled = true;
                    AreaShow();
                });
            }
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
            onCompleted.SafeInvoke();
        }

        void AreaShow()
        {
            AreaHide();
            Area.Show(mapManager.MapEntity.WalkableBorder(transform.position, Range), mapManager.MapEntity);
        }

        void AreaHide()
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

        void PathHide()
        {
            if (Path)
            {
                Path.Hide();
            }
        }

        void PathUpdate()
        {
            if (Path && Path.IsEnabled)
            {
                var tile = mapManager.MapEntity.Tile(MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane()));
                if (tile != null && tile.Vacant)
                {
                    var path = mapManager.MapEntity.PathPoints(transform.position, mapManager.MapEntity.WorldPosition(tile.Position), Range);
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
    }
//}
