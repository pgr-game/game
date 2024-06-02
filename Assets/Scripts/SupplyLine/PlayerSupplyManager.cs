using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSupplyManager
{
    private PlayerManager playerManager;
    private List<SupplyLineController> supplyLines;

    // Creating supply lines
    public bool drawingSupplyLine = false;
    public bool justActivated = true;
    private City originCity;
    private Vector3? drawingStartPosition;
    private Vector3? drawingEndPosition;
    private AreaOutline passableArea;
    private PathDrawer supplyLineDrawer;

    public void Init(PlayerManager playerManager, SupplyLoadData loadData)
    {
        this.playerManager = playerManager;
        supplyLines = new List<SupplyLineController>();
        passableArea = Spawner.Spawn(playerManager.passableAreaPrefab, Vector3.zero, Quaternion.identity);
        passableArea.Hide();
        supplyLineDrawer = Spawner.Spawn(playerManager.pathPrefab, Vector3.zero, Quaternion.identity);
        supplyLineDrawer.Hide();

        if (loadData != null)
        {
            supplyLines = loadData.supplyLines;
        }
    }

    public void OpenSupplyLineDrawer(City originCity)
    {
        this.originCity = originCity;
        drawingStartPosition = originCity.cityTiles.First().transform.position;

        passableArea.Show(playerManager.mapManager.MapEntity.WalkableBorder((Vector3)drawingStartPosition, float.MaxValue), playerManager.mapManager.MapEntity);

        Color supplyActiveColor = playerManager.color;
        supplyActiveColor.a = supplyLineDrawer.ActiveColor.a;
        Color supplyInactiveColor = playerManager.color;
        supplyInactiveColor.a = supplyLineDrawer.InactiveColor.a;

        supplyLineDrawer.Show(new List<Vector3>() { }, playerManager.mapManager.MapEntity);
        supplyLineDrawer.Init(supplyActiveColor, supplyInactiveColor, 0);
        supplyLineDrawer.ActiveState();
        supplyLineDrawer.IsEnabled = true;
        drawingSupplyLine = true;
    }

    public void UpdateSupplyLineDrawer()
    {
        var tile = playerManager.mapManager.MapEntity.Tile(MyInput.GroundPosition(playerManager.mapManager.MapEntity.Settings.Plane()));
        if (tile != null && tile.Vacant)
        {
            var path = playerManager.mapManager.MapEntity.PathPoints((Vector3)drawingStartPosition, playerManager.mapManager.MapEntity.WorldPosition(tile.Position), float.MaxValue);

            supplyLineDrawer.Show(path, playerManager.mapManager.MapEntity);
            supplyLineDrawer.ActiveState();
        }
        else
        {
            supplyLineDrawer.InactiveState();
        }
    }

    public void CreateSupplyLine(Vector3? startPosition, Vector3 endPosition)
    {
        if(startPosition == null)
        {
            startPosition = drawingStartPosition;
        }
        List<TileEntity> path = playerManager.mapManager.MapEntity.PathTiles((Vector3)startPosition, endPosition, float.MaxValue);
        SupplyLineController newSupplyLine = new SupplyLineController();
        newSupplyLine.Init(this, originCity, path, supplyLineDrawer);
        supplyLines.Add(newSupplyLine);
        ClearSupplyLineCreator();
    }

    public void HideSupplyLineCreator()
    {
        supplyLineDrawer.Hide();
    }

    public void ClearSupplyLineCreator()
    {
        drawingSupplyLine = false;
        justActivated = true;
        originCity = null;
        drawingStartPosition = null;
        drawingEndPosition = null;
        passableArea.Hide();
    }
}
