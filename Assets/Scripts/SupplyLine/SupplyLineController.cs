using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyLineController
{
    private PlayerSupplyManager playerSupplyManager;
    private City originCity;
    private List<TileEntity> path;
    private PathDrawer supplyLineDrawer;


    public void Init(PlayerSupplyManager playerSupplyManager, City originCity, List<TileEntity> path, PathDrawer supplyLineDrawer)
    {
        this.playerSupplyManager = playerSupplyManager;
        this.originCity = originCity;
        this.path = path;
        this.supplyLineDrawer = supplyLineDrawer;
    }
}
