using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RedBjorn.ProtoTiles;
    
public class MapManager : MonoBehaviour
{
    public GameManager gameManager;
    public MapSettings Map;
    public KeyCode GridToggle = KeyCode.G;
    public MapView MapView;

    public MapEntity MapEntity { get; private set; }
    RaycastHit hit;  

    public GameObject CityUIPrefab; 

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        if (!MapView)
        {
            MapView = GameObject.FindObjectOfType<MapView>();
        }
        MapEntity = new MapEntity(Map, MapView);

        GameObject[] cityTiles = GameObject.FindGameObjectsWithTag("CityTile");
        foreach (GameObject cityTileObject in cityTiles)
        {
            CityTile cityTileComponent = cityTileObject.GetComponent<CityTile>();
            if (cityTileComponent != null)
            {
                cityTileComponent.Init(gameManager);
            }
        }

        if (MapView)
        {
            MapView.Init(MapEntity);
            InitTileRenderOrder();
            InitCities();
        }
        else
        {
            Debug.Log("Can't find MapView. Random errors can occur");
        }
    }


    void Update()
    {
        if (Input.GetKeyUp(GridToggle))
        {
            MapEntity.GridToggle();
        }
    }

    void InitTileRenderOrder() {
        foreach(Transform child in MapView.transform.Find("Tiles")) {
            SpriteRenderer sprite = child.Find("Model").GetComponent<SpriteRenderer>();
            sprite.sortingOrder = -(int)child.transform.position.y+100000;
        }
    }

    void InitCities() {
        //create City for every cluster of CityTile tiles, except for players' starting cities
        foreach(Transform child in MapView.transform.Find("Tiles")) {
            CityTile cityTile = child.GetComponent<CityTile>();
            if(cityTile) {
                if(cityTile.city == null) {
                    List<CityTile> cityTiles = GetCityTilesInPosition(cityTile.transform.position);
                    List<Vector3> positions = cityTiles.Select(cityTile => cityTile.transform.position).ToList();
                    bool isPlayerStart = false;
                    foreach(Vector3 playerPosition in gameManager.playerPositions) {
                        foreach(Vector3 cityPosition in positions) {
                            if(playerPosition.x == cityPosition.x
                            && playerPosition.y == cityPosition.y 
                            && playerPosition.z == cityPosition.z) {
                                //leave claim for player
                                isPlayerStart = true;
                            }
                        }
                    }
                    if(isPlayerStart) {
                        continue;
                    } else {
                        InitCity(cityTiles, null, null, null); //eventually get a name from list of neutral names
                    }
                }
            }
        }
    }

    public void InitCity(List<CityTile> cityTiles, PlayerManager playerManager, string name, CityLoadData cityLoadData) {
        City city = new City();
        city.cityTiles = new List<CityTile>();
        city.turnCreated = gameManager.turnNumber;

        foreach(CityTile cityTile in cityTiles) {
            cityTile.ClaimStartingCityTile(playerManager, city);
            city.cityTiles.Add(cityTile);
        }

        if(playerManager) {
            playerManager.playerCitiesManager.AddCity(city);
        }

        city.InitCity(this, playerManager?.color, this.CityUIPrefab, name);

        if (cityLoadData != null)
        {
            gameManager.cityMenuManager.setValues(city);
            if(cityLoadData.unitInProduction != null && cityLoadData.unitInProduction != "")
            {
                gameManager.cityMenuManager.SelectProductionUnit(cityLoadData.unitInProduction);
                // + 1 is necessary because there is always a turn skip after city init
                city.UnitInProductionTurnsLeft = cityLoadData.unitInProductionTurnsLeft + 1;
                city.UI.SetTurnsLeft(cityLoadData.unitInProductionTurnsLeft + 1);
            }
        }
    }

    CityTile CityTileFromPosition(Vector3 position) {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z - 10), Vector3.forward, out hit)) {
            return hit.transform.GetComponent<CityTile>();
        }  
        return null;
    }

    public TileEntity TileEntityFromPosition(Vector3 position) {
        TileEntity tileEntity = MapEntity.Tile(position);
        return tileEntity;
    }

    public List<CityTile> GetCityTilesInPosition(Vector3 position) {
        List<CityTile> tiles = new List<CityTile>();
        CityTile cityTile = CityTileFromPosition(position);
        if(cityTile != null) {
            tiles.Add(cityTile);
            GetCitySurroundingTiles(tiles, TileEntityFromPosition(position));
        }  
        return tiles;
    }

    void GetCitySurroundingTiles(List<CityTile> tiles, TileEntity tile) {
        List<Vector3> surroundingTilesPositions = MapEntity.AreaExistedPositions(tile, 1);
        List<CityTile> newTiles = new List<CityTile>();
        foreach(Vector3 position in surroundingTilesPositions) {
            CityTile newTile = CityTileFromPosition(position);
            if(newTile != null) {
                if(!tiles.Contains(newTile)) {
                    newTiles.Add(newTile);
                }
            }
        }
        foreach(CityTile foundTile in newTiles) {
                tiles.Add(foundTile);
        }

        foreach(CityTile foundTile in newTiles) {
                GetCitySurroundingTiles(tiles, TileEntityFromPosition(foundTile.transform.position));
        }
    }

    public List<TileEntity> GetTilesSurroundingArea(List<TileEntity> areaTiles, int distance, bool includeAreaTiles) 
    {
        HashSet<TileEntity> surroundingTiles = new HashSet<TileEntity>();

        foreach(TileEntity areaTile in areaTiles)
        {
            var tilesSurroundingAreaTile = MapEntity.WalkableTiles(areaTile.Position, distance);
            surroundingTiles.UnionWith(tilesSurroundingAreaTile);
        }

        if (!includeAreaTiles)
        {
            surroundingTiles.ExceptWith(areaTiles);
        }

        return surroundingTiles.ToList();
    }

    public static Vector3 CalculateMidpoint(List<Vector3> points)
    {
        float sumX = 0f;
        float sumY = 0f;
        foreach (Vector3 point in points)
        {
            sumX += point.x;
            sumY += point.y;
        }

        return new Vector3(sumX / points.Count, sumY / points.Count, 0f);
    }

    public List<TileEntity> GetTilesInRange(int range , Vector3Int hexPosition)
    {
        List<TileEntity> tilesInRange = new List<TileEntity>();
        List<Vector3> positionsInRange = MapEntity.AreaExistedPositions(MapEntity.Tile(hexPosition), range);
        foreach(Vector3 position in positionsInRange)
        {
            TileEntity tile = MapEntity.Tile(position);
            if(tile != null)
            {
                tilesInRange.Add(tile);
            }
        }
        return tilesInRange;
    }
    
    public int CalculateDistanceBetweenTiles(Vector3Int tile1, Vector3Int tile2)
    {
        return MapEntity.PathTiles(tile1, tile2, float.MaxValue).Count;
    }
}
