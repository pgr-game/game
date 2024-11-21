using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class PlayerCitiesManager : NetworkBehaviour
{
    public List<City> cities { get; private set; }
    private PlayerManager playerManager;
    private MapManager mapManager;
    private Dictionary<string, int> unitAmountTarget = new Dictionary<string, int>();
    public int attackedCitiesCount = 0;

    public void StartCitiesTurn() {
        cities.ForEach(city => city.StartTurn());
        attackedCitiesCount = cities.Count(city => city.isUnderAttack);
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
        FillUnitAmountTarget();
    }

    private void FillUnitAmountTarget()
    {
        unitAmountTarget.Add("Archer", 4);
        unitAmountTarget.Add("Catapult", 2);
        unitAmountTarget.Add("Chariot", 4);
        unitAmountTarget.Add("Elephant", 2);
        unitAmountTarget.Add("Hoplite", 6);
        unitAmountTarget.Add("LightInfantry", 6);
        unitAmountTarget.Add("Skirmisher", 4);
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

    public List<City> GetCities() {
        return cities;
    }

    public int GetNumberOfCities() {
        return cities.Count;
    }

    public void DoTurn()
    {
        var playerUnits = playerManager.playerUnitsManager.GetUnits();
        // count units by type
        var unitTypeCount = new Dictionary<string, int>();
        foreach (var unit in playerUnits)
        {
            if (unitTypeCount.ContainsKey(unit.unitType.ToString()))
            {
                unitTypeCount[unit.unitType.ToString()] += 1;
            }
            else
            {
                unitTypeCount[unit.unitType.ToString()] = 1;
            }
        }
        
        
        
        // make a list of types that are below target and how many more we need
        var unitsToProduce = new Dictionary<string, int>();
        foreach (var unitType in unitAmountTarget.Keys)
        {
            if (unitTypeCount.ContainsKey(unitType))
            {
                if (unitTypeCount[unitType] < unitAmountTarget[unitType])
                {
                    unitsToProduce[unitType] = unitAmountTarget[unitType] - unitTypeCount[unitType];
                }
            }
            else
            {
                unitsToProduce[unitType] = unitAmountTarget[unitType];
            }
        }
        
        if(!playerManager.gameManager.playerTreeManager.isUnitUnlocked("Chariot")) {
            unitsToProduce["Chariot"] = 0;
        }
        if(!playerManager.gameManager.playerTreeManager.isUnitUnlocked("Elephant")) {
            unitsToProduce["Elephant"] = 0;
        }
        if(!playerManager.gameManager.playerTreeManager.isUnitUnlocked("Catapult")) {
            unitsToProduce["Catapult"] = 0;
        }
        
        var citiesReadyToProduce = cities.Where(city => city.CanProduceUnit()).ToList();

        var howManyCitiesCanProduce = citiesReadyToProduce.Count;
        if(howManyCitiesCanProduce == 0)
        {
            return;
        }
        
        var weightedUnitList = new Dictionary<string, int>(unitsToProduce);

        var unitsAssignedToCities = new List<string>();

        if (unitsToProduce.Count != 0)
        {
            for (var i = 0; i < howManyCitiesCanProduce; i++)
            {
                // Get the total weight of remaining units
                int totalWeight = weightedUnitList.Values.Sum();

                // Generate a random number between 1 and the total weight
                int randomWeight = UnityEngine.Random.Range(1, totalWeight + 1);

                // Determine which unit to produce based on the random weight
                int currentWeight = 0;
                string selectedUnitType = null;

                foreach (var unit in weightedUnitList)
                {
                    currentWeight += unit.Value;

                    if (randomWeight <= currentWeight)
                    {
                        selectedUnitType = unit.Key;
                        break;
                    }
                }

                // Add the selected unit type to the list of units assigned to cities
                if (selectedUnitType != null)
                {
                    unitsAssignedToCities.Add(selectedUnitType);

                    // Decrease the count for the selected unit type
                    weightedUnitList[selectedUnitType]--;

                    // Remove the unit type if no more units of that type are needed
                    if (weightedUnitList[selectedUnitType] == 0)
                    {
                        weightedUnitList.Remove(selectedUnitType);
                    }
                }

                // If there are no more units to produce, stop the loop
                if (!weightedUnitList.Any())
                    break;
            }
        }

        // fill rest of the cities with random units
        if (howManyCitiesCanProduce != unitsAssignedToCities.Count)
        {
            // draw howManyCitiesCanProduce - unitsAssignedToCities.Count random units from UnitTypes enum
            var unitTypes = System.Enum.GetValues(typeof(UnitTypes)).Cast<UnitTypes>().ToList();
            // remove units that are not unlocked
            unitTypes.RemoveAll(unitType => !playerManager.gameManager.playerTreeManager.isUnitUnlocked(unitType.ToString()));
            
            var randomUnitTypes = new List<UnitTypes>();
            for (var i = 0; i < howManyCitiesCanProduce - unitsAssignedToCities.Count; i++)
            {
                var randomUnitType = unitTypes[UnityEngine.Random.Range(0, unitTypes.Count)];
                randomUnitTypes.Add(randomUnitType);
            }
            
            // add randomUnitTypes to unitsAssignedToCities with ToString() mapping
            unitsAssignedToCities.AddRange(randomUnitTypes.Select(unitType => unitType.ToString()));
        }
        
        
        // produce units
        if(unitsAssignedToCities.Count == 0)
        {
            return;
        }
        StartProductionInCities(citiesReadyToProduce, unitsAssignedToCities);
        
        
    }

    private void StartProductionInCities(List<City> citiesReadyToProduce, List<string> unitsToProduce)
    {
        var unitPrefabs = playerManager.gameManager.unitPrefabs;
        foreach (var city in citiesReadyToProduce)
        {
            // select and remove first unit from the list
            var unitType = unitsToProduce[0];
            unitsToProduce.RemoveAt(0);
            
            // get prefab
            var unitPrefab = unitPrefabs.FirstOrDefault(prefab => prefab.GetComponent<UnitController>().unitType.ToString() == unitType);
            if (unitPrefab == null)
            {
                Debug.LogError("Unit prefab not found for unit type: " + unitType);
                return;
            }
            UnitController unit = unitPrefab.GetComponent<UnitController>();
            
            city.SetUnitInProduction(unit, unitPrefab);
        }
        
    }
}