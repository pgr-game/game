using RedBjorn.ProtoTiles.Example;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupplyManager
{
    private PlayerManager playerManager;
    private List<SupplyLineController> supplyLines;

    // Prefabs
    public PathDrawer PathPrefab;

    public void Init(PlayerManager playerManager, SupplyLoadData loadData)
    {
        this.playerManager = playerManager;
        supplyLines = new List<SupplyLineController>();

        if (loadData != null)
        {
            supplyLines = loadData.supplyLines;
        }
    }

    public void Create()
    {

    }
}
