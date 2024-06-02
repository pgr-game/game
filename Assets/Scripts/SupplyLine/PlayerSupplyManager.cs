using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupplyManager
{
    private PlayerManager playerManager;
    private List<SupplyLineController> supplyLines;
    public AreaOutline passableArea;

    // Prefabs
    public PathDrawer pathPrefab;
    public AreaOutline passableAreaPrefab;

    public void Init(PlayerManager playerManager, SupplyLoadData loadData)
    {
        this.playerManager = playerManager;
        supplyLines = new List<SupplyLineController>();
        //passableArea = Spawner.Spawn(passableAreaPrefab, Vector3.zero, Quaternion.identity);
        //passableArea.Hide();

        if (loadData != null)
        {
            supplyLines = loadData.supplyLines;
        }
    }

    public void Create(City originCity)
    {
        List<TileEntity> supplyLineTiles = new List<TileEntity>();

        //passableArea.Show(playerManager.mapManager.MapEntity.WalkableBorder(originCity.uiAnchor, float.MaxValue), playerManager.mapManager.MapEntity);
    }
}
