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
        TileEntity previousTile = null;
        foreach (TileEntity tile in this.path)
        {
            tile.SupplyLineProvider = playerSupplyManager.playerManager;
            if (previousTile != null) {
                float distance  = playerSupplyManager.gameManager.mapManager.MapEntity.Distance(previousTile.Position , tile.Position);
                var surrourndingTiles = playerSupplyManager.gameManager.mapManager.MapEntity.WalkableTiles(tile.Position, distance); //could work later for the surrouding tiles to get buff as well
                foreach (TileEntity tileSurr in surrourndingTiles)
                {
                    tileSurr.SupplyLineProvider = playerSupplyManager.playerManager;
                }
            }
            previousTile = tile;
        }

        Vector3 pathStartWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.First().Position);
        Vector3 pathEndWorldPosition = playerSupplyManager.gameManager.mapManager.MapEntity.WorldPosition(path.Last().Position);
        var drawerPath = playerSupplyManager.gameManager.mapManager.MapEntity.PathPoints(pathStartWorldPosition, pathEndWorldPosition, float.MaxValue);
        supplyLineDrawer.Show(drawerPath, playerSupplyManager.gameManager.mapManager.MapEntity);
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
