using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerCitiesManager
{
    private List<City> cities;
    private PlayerManager playerManager;
    RaycastHit hit;  

    public void Init(PlayerManager playerManager) {
        Debug.Log("Initializing player cities");
        this.playerManager = playerManager;
        ClaimStartingCity();
    }

    void ClaimStartingCity() {
        Debug.Log("Claiming starting city");
        City city = new City();
        city.cityTiles = new List<CityTile>();
        List<CityTile> startingCityTiles = GetCityTilesInPosition(playerManager.transform.position);
        foreach(CityTile cityTile in startingCityTiles) {
                cityTile.ClaimStartingCity(this.playerManager, city);
                city.cityTiles.Add(cityTile);
                Debug.Log("Claiming one city tile");
        }
    }

    CityTile CityTileFromPosition(Vector3 position) {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z - 10), Vector3.forward, out hit)) {
            return hit.transform.GetComponent<CityTile>();
        }  
        //Debug.Log("CityTile not found at position");
        return null;
    }

    TileEntity TileEntityFromPosition(Vector3 position) {
        TileEntity tileEntity = playerManager.mapManager.MapEntity.Tile(position);
        if(tileEntity == null) {
            Debug.Log("TileEntity not found at position");
        }
        return tileEntity;
    }

    List<CityTile> GetCityTilesInPosition(Vector3 position) {
        List<CityTile> tiles = new List<CityTile>();
        CityTile startingCityTile = CityTileFromPosition(position);
        if(startingCityTile != null) {
            Debug.Log("Found city in position");
            tiles.Add(startingCityTile);
            GetCitySurroundingTiles(tiles, TileEntityFromPosition(position));
        }  
        return tiles;
    }

    void GetCitySurroundingTiles(List<CityTile> tiles, TileEntity tile) {
        List<Vector3> surroundingTilesPositions = playerManager.mapManager.MapEntity.AreaExistedPositions(tile, 1);
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
                GetCitySurroundingTiles(tiles, TileEntityFromPosition(foundTile.transform.position));
        }
    }
}