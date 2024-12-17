using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using Unity.Netcode;
using UnityEngine.UIElements;

public class PlayerCitiesManager : NetworkBehaviour
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
                ClaimStartingCity(city.name, city.position, city);
            }
        } else
        {
            ClaimStartingCity(startingCityName, playerManager.transform.position, null);
        }
    }

    public void AddCity(City city) {
        cities.Add(city);
    }

    void ClaimStartingCity(string startingCityName, Vector3 position, CityLoadData cityLoadData) {
        List<CityTile> startingCityTiles = mapManager.GetCityTilesInPosition(position);
        if(startingCityTiles.Count != 0) {
            mapManager.InitCity(startingCityTiles, this.playerManager, startingCityName, cityLoadData);
        }
    }

    public int GetGoldIncome() {
        // TODO in the future: make it more advanced, based on level for example
        return cities.Count*10;
    }

    public bool Contains(City city) {
        return cities.Contains(city);
    }

    public int GetNumberOfCities() {
        return cities.Count;
    }
}