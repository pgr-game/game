using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using UnityEngine.UIElements;

public class PlayerCitiesManager
{
    public List<City> cities { get; private set; }
    private PlayerManager playerManager;
    private MapManager mapManager;

    public void StartCitiesTurn() {
        cities.ForEach(city => city.StartTurn());
    }

    public void Init(PlayerManager playerManager, string startingCityName, List<CityLoadData> cityLoadData) {
        this.cities = new List<City>();
        this.playerManager = playerManager;
        this.mapManager = playerManager.mapManager;
        if (cityLoadData != null && cityLoadData.Count != 0)
        {
            foreach (var city in cityLoadData)
            {
                ClaimStartingCity(city.name, city.position);
            }
        } else
        {
            ClaimStartingCity(startingCityName, playerManager.transform.position);
        }
    }

    public void AddCity(City city) {
        cities.Add(city);
    }

    void ClaimStartingCity(string startingCityName, Vector3 position) {
        List<CityTile> startingCityTiles = mapManager.GetCityTilesInPosition(position);
        if(startingCityTiles.Count != 0) {
            mapManager.InitCity(startingCityTiles, this.playerManager, startingCityName);
        }
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