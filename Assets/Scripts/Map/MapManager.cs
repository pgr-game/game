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

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        if (!MapView)
        {
            MapView = GameObject.FindObjectOfType<MapView>();
        }
        MapEntity = new MapEntity(Map, MapView);
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
        Debug.Log("Map manager initiated");
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
            sprite.sortingOrder = -(int)child.transform.position.y;
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
                        Debug.Log("Leave city claim for player");
                        continue;
                    } else {
                        Debug.Log("Init neutral city");
                        InitCity(cityTiles, null);
                    }
                }
            }
        }
    }

    public void InitCity(List<CityTile> cityTiles, PlayerManager playerManager) {
        City city = new City();
        city.cityTiles = new List<CityTile>();

        foreach(CityTile cityTile in cityTiles) {
            cityTile.ClaimStartingCityTile(playerManager, city);
            city.cityTiles.Add(cityTile);
            Debug.Log("Init one city tile");
        }

        if(playerManager) {
            playerManager.playerCitiesManager.AddCity(city);
        }

        //assign colors and UI
    }

    CityTile CityTileFromPosition(Vector3 position) {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z - 10), Vector3.forward, out hit)) {
            return hit.transform.GetComponent<CityTile>();
        }  
        //Debug.Log("CityTile not found at position");
        return null;
    }

    TileEntity TileEntityFromPosition(Vector3 position) {
        TileEntity tileEntity = MapEntity.Tile(position);
        if(tileEntity == null) {
            Debug.Log("TileEntity not found at position");
        }
        return tileEntity;
    }

    public List<CityTile> GetCityTilesInPosition(Vector3 position) {
        List<CityTile> tiles = new List<CityTile>();
        CityTile cityTile = CityTileFromPosition(position);
        if(cityTile != null) {
            Debug.Log("Found city in position");
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

    public static Vector3 CalculateMidpoint(List<Vector3> points)
    {
        float sumX = 0f;
        float sumY = 0f;
        foreach (Vector3 point in points)
        {
            sumX += point.x;
            sumY += point.y;
        }
        // Assuming z should remain 0
        return new Vector3(sumX / points.Count, sumY / points.Count, 0f);
    }
}
