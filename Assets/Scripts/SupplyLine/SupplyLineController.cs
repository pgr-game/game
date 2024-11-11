using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SupplyLineController 
{
    private PlayerSupplyManager playerSupplyManager;
    public GameObject hexHighlitPrefab;
    private MapManager mapManager;
    public City originCity { get; private set; }
    private List<TileEntity> path;
    private PathDrawer supplyLineDrawer;
    private List<TileEntity> areaOfEffectTiles;
    private List<GameObject> tilesHighlited=new List<GameObject>();


    public int Init(PlayerSupplyManager playerSupplyManager, City originCity, List<TileEntity> path, PathDrawer supplyLineDrawer, GameObject hexHighlitPrefab)
    {

        mapManager = playerSupplyManager.gameManager.mapManager;
        if (path.Count == 0)
        {
            return -1;
        }
        this.playerSupplyManager = playerSupplyManager;
        this.originCity = originCity;
        this.path = path;
        this.supplyLineDrawer = supplyLineDrawer;

        Color playerColor = this.playerSupplyManager.playerManager.color;
        Color color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.5f);
        int distance = 2; // tile size is 1
        areaOfEffectTiles = mapManager.GetTilesSurroundingArea(path, distance, true);
        bool first = false;
        foreach (TileEntity tile in areaOfEffectTiles)
        {
            tile.SupplyLineProvider = playerSupplyManager.playerManager;
            if (!first)
            {
                var v = mapManager.MapEntity.WorldPosition(tile);
                GameObject newHighlit = GameObject.Instantiate(hexHighlitPrefab, v,new Quaternion(0f,0f,45f,0));
                newHighlit.transform.Find("Model").GetComponent<Renderer>().material.color = color;
                tilesHighlited.Add(newHighlit);
                //first = true;
            }
        
            if (tile.CityTilePresent)
            {
                tile.CityTilePresent.city.UpdateSuppliedStatus();
            }
        }
        //UnitController unit = prefab.GetComponent<UnitController>();


        Vector3 pathStartWorldPosition = mapManager.MapEntity.WorldPosition(path.First().Position);
        Vector3 pathEndWorldPosition = mapManager.MapEntity.WorldPosition(path.Last().Position);
        var drawerPath = mapManager.MapEntity.PathPoints(pathStartWorldPosition, pathEndWorldPosition, float.MaxValue);
        supplyLineDrawer.Show(drawerPath, mapManager.MapEntity);
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
        foreach(var t in tilesHighlited)
        {
            GameObject.Destroy(t);
        }
        foreach (TileEntity tile in areaOfEffectTiles)
        {
            tile.SupplyLineProvider = null;
            if (tile.CityTilePresent)
            {
                tile.CityTilePresent.city.UpdateSuppliedStatus();
            }
        }
        UnityEngine.Object.Destroy(supplyLineDrawer.gameObject);
    }
}
