using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerCitiesManager
{
    private List<City> cities;
    private PlayerManager playerManager;
    private MapManager mapManager;

    public void StartCitiesTurn() {
        cities.ForEach(city => city.StartTurn());
    }

    public void Init(PlayerManager playerManager, string startingCityName) {
        Debug.Log("Initializing player cities");
        this.cities = new List<City>();
        this.playerManager = playerManager;
        this.mapManager = playerManager.mapManager;
        ClaimStartingCity(startingCityName);
    }

    public void AddCity(City city) {
        Debug.Log("Adding city");
        cities.Add(city);
    }

    void ClaimStartingCity(string startingCityName) {
        Debug.Log("Claiming starting city");
        List<CityTile> startingCityTiles = mapManager.GetCityTilesInPosition(playerManager.transform.position);
        mapManager.InitCity(startingCityTiles, this.playerManager, startingCityName);
    }

    public int GetGoldIncome() {
        // TODO in the future: make it more advanced, based on level for example
        return cities.Count*10;
    }

    public List<City> GetCities() {
        return cities;
    }

    public int GetNumberOfCities() {
        return cities.Count;
    }
}