using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SupplyLineController
{
    private PlayerSupplyManager playerSupplyManager;
    public City originCity { get; private set; }
    private List<TileEntity> path;
    private PathDrawer supplyLineDrawer;


    public void Init(PlayerSupplyManager playerSupplyManager, City originCity, List<TileEntity> path, PathDrawer supplyLineDrawer)
    {
        this.playerSupplyManager = playerSupplyManager;
        this.originCity = originCity;
        this.path = path;
        this.supplyLineDrawer = supplyLineDrawer;

        Vector3 pathStartWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.First().Position);
        Vector3 pathEndWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.Last().Position);
        var drawerPath = playerSupplyManager.gameManager.mapManager.MapEntity.PathPoints(pathStartWorldPosition, pathEndWorldPosition, float.MaxValue);
        supplyLineDrawer.Show(drawerPath, playerSupplyManager.gameManager.mapManager.MapEntity);
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy(supplyLineDrawer.gameObject);
    }
}
