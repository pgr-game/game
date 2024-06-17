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


    public int Init(PlayerSupplyManager playerSupplyManager, City originCity, List<TileEntity> path, PathDrawer supplyLineDrawer)
    {
        if (path.Count == 0)
        {
            return -1;
        }
        this.playerSupplyManager = playerSupplyManager;
        this.originCity = originCity;
        this.path = path;
        this.supplyLineDrawer = supplyLineDrawer;

        float distance = 2f; // tile size is 1
        foreach (TileEntity tile in this.path)
        {
            tile.SupplyLineProvider = playerSupplyManager.playerManager;
            var surrourndingTiles = playerSupplyManager.gameManager.mapManager.MapEntity.WalkableTiles(tile.Position, distance); //could work later for the surrouding tiles to get buff as well
            foreach (TileEntity tileSurr in surrourndingTiles)
            {
                tileSurr.SupplyLineProvider = playerSupplyManager.playerManager;
            }
        }

        Vector3 pathStartWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.First().Position);
        Vector3 pathEndWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.Last().Position);
        var drawerPath = playerSupplyManager.gameManager.mapManager.MapEntity.PathPoints(pathStartWorldPosition, pathEndWorldPosition, float.MaxValue);
        supplyLineDrawer.Show(drawerPath, playerSupplyManager.gameManager.mapManager.MapEntity);
        return 1;
    }


    public bool EnemyOnSupplyLine()
    {
        foreach (TileEntity tile in path)
        {
            bool enemyUnitPresent = tile.UnitPresent != null && tile.UnitPresent.owner != this.playerSupplyManager.playerManager;
            if (enemyUnitPresent)
            {
                return true;
            }
        }
        return false;
    }


    public void Destroy()
    {
        TileEntity previousTile = null;
        foreach (TileEntity tile in this.path)
        {
            tile.SupplyLineProvider = null;
            if (previousTile != null)
            {
                float distance = playerSupplyManager.gameManager.mapManager.MapEntity.Distance(previousTile.Position, tile.Position);
                var surrourndingTiles = playerSupplyManager.gameManager.mapManager.MapEntity.WalkableTiles(tile.Position, distance); //could work later for the surrouding tiles to get buff as well
                foreach (TileEntity tileSurr in surrourndingTiles)
                {
                    tileSurr.SupplyLineProvider = null;
                }
            }
            previousTile = tile;
        }
        UnityEngine.Object.Destroy(supplyLineDrawer.gameObject);
    }
}
