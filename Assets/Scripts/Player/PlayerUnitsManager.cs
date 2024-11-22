using System;
using RedBjorn.ProtoTiles;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerUnitsManager : NetworkBehaviour
{
    private PlayerManager playerManager;
    private GameManager gameManager;
    private List<UnitController> units = new List<UnitController>();
    
    // AI PARAMS, units state ratio, all ratios sum to 1
    public float attackingCityRatio = 0.0f;
    public float protectingCityRatio = 0.5f;
    public float freeWanderingRatio = 0.5f;
    private static readonly Dictionary<UnitTypes, int[]> unitPriorityMatrix = new Dictionary<UnitTypes, int[]>
    {
	    { UnitTypes.Archer,        new int[] { 2, 1, 0 } }, // FreeWandering, ProtectingCity, AttackingCity
	    { UnitTypes.Catapult,      new int[] { 1, 0, 2 } },
	    { UnitTypes.Chariot,       new int[] { 1, 0, 2 } },
	    { UnitTypes.Elephant,      new int[] { 0, 2, 1 } },
	    { UnitTypes.Hoplite,       new int[] { 0, 1, 2 } },
	    { UnitTypes.LightInfantry, new int[] { 1, 2, 0 } },
	    { UnitTypes.Skirmisher,    new int[] { 2, 1, 0 } }
    };

    public void Init(PlayerManager playerManager, StartingResources startingResources)
    {
        this.playerManager = playerManager;
        this.gameManager = playerManager.gameManager;

        if (startingResources == null)
        {
            Debug.Log("No starting resources for player!");
            return;
        }

        if (gameManager.isMultiplayer && !IsServer)
        {
            // Pick up spawned starting units
            var units = FindObjectsByType<UnitController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
	            .Where(unit => unit.owner == null);

            foreach (var unitController in units)
            {
	            var ownerIndex = unitController.ownerIndex;
				var owner = gameManager.players[ownerIndex.Value];
				unitController.Init(owner, owner.mapManager, owner.gameManager, 
					owner.gameManager.unitStatsMenuController, null, null);
			}
	        return;
        };

        if (startingResources.unitLoadData.Count > 0)
        {
	        var unitControllerAndLoadData = startingResources.units.Zip(
		        startingResources.unitLoadData, (u, d) => new { UnitController = u, UnitLoadData = d }
	        );

	        foreach (var ud in unitControllerAndLoadData)
	        {
		        if (gameManager.isMultiplayer)
		        {
			        InstantiateUnitRpc(ud.UnitController.name, ud.UnitLoadData, playerManager.transform.position);
				}
		        else
		        {
			        InstantiateUnit(ud.UnitController.name, ud.UnitLoadData, playerManager.transform.position);
				}
			}
        }
        else
        {
	        // instantiate new without loading health, movement left etc.
	        foreach (UnitController unit in startingResources.units)
	        {
		        if (gameManager.isMultiplayer)
		        {
					InstantiateUnitRpc(unit.name, null, playerManager.transform.position);
				}
		        else
		        {
					InstantiateUnit(unit.name, null, playerManager.transform.position);
				}
	        }
        }
	}

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void InstantiateUnitRpc(string unitName, UnitLoadData unitLoadData, Vector3 position)
    {
	    InstantiateUnit(unitName, unitLoadData, position);
    }

    public void InstantiateUnit(string unitName, UnitLoadData unitLoadData, Vector3 position)
    {
	    UnitController unitController = gameManager.unitPrefabs
		    .FirstOrDefault(prefab => prefab.name == unitName)?
		    .GetComponent<UnitController>();
	    float? rangeLeft = null;
	    Vector3? longPathClickPosition = null;
	    if (unitLoadData != null)
	    {
		    position = unitLoadData.position;
		    unitController.maxHealth = unitLoadData.maxHealth;
		    unitController.currentHealth = unitLoadData.currentHealth;
		    unitController.attack = unitLoadData.attack;
		    unitController.attackRange = unitLoadData.attackRange;
		    unitController.baseProductionCost = unitLoadData.baseProductionCost;
		    unitController.turnsToProduce = unitLoadData.turnsToProduce;
		    unitController.turnProduced = unitLoadData.turnProduced;
		    unitController.level = unitLoadData.level;
		    unitController.turnsToProduce = unitLoadData.turnsToProduce;
		    unitController.experience = unitLoadData.experience;
		    rangeLeft = unitLoadData.rangeLeft;
		    unitController.attacked = unitLoadData.attacked;
		    longPathClickPosition = unitLoadData.longPathClickPosition;
	    }

	    UnitController newUnit = GameObject.Instantiate(unitController, position, Quaternion.identity).GetComponent<UnitController>();
	    units.Add(newUnit);
	    newUnit.Init(playerManager, playerManager.mapManager, playerManager.gameManager, playerManager.gameManager.unitStatsMenuController,
		    rangeLeft, longPathClickPosition);

	    if (gameManager.isMultiplayer)
	    {
		    newUnit.ownerIndex.Initialize(newUnit);
		    newUnit.ownerIndex.Value = newUnit.owner.index;
		    var instanceNetworkObject = newUnit.GetComponent<NetworkObject>();
		    var clientId = gameManager.GetClientId(playerManager.index);
		    if (clientId != null)
		    {
			    instanceNetworkObject.SpawnWithOwnership((ulong)clientId);
		    }
		    else
		    {
			    instanceNetworkObject.Spawn();
		    }
	    }
	}

    public void RemoveUnit(UnitController unit)
    {
        units.Remove(unit);
    }

    public void StartUnitsTurn()
    {
        units.ForEach((unit) => {
            unit.attacked = false;
            unit.unitMove.ResetRange();
            if (unit.CanHealOrGetDefenceBonus()) unit.Heal();
            unit.CommitToBuildingFort();
            if (gameManager.turnNumber != 1)
            {
                unit.turnsSinceFortPlaced++;
                if (unit.turnsSinceFortPlaced == 10) unit.canPlaceFort = true;
            }
        });
    }

    public void TryAutoMoveAll()
    {
        foreach (UnitController unit in units)
        {
            unit.unitMove.TryAutoMove();
        }
    }
    public void DeactivateAll()
    {
        foreach (UnitController unit in units)
        {
            unit.Deactivate();
        }
    }

    public int GetUnitCount() { 
        return units.Count;
    }

    public List<UnitController> GetUnits()
    {
        return units;
    }

    // probably will be deleted when we implement multiple units on a tile
    public void ResetUnitPresentOnTile(TileEntity tile, UnitController currentUnit)
    {
        units.ForEach((unit) => {
            if (currentUnit != unit && unit.GetCurrentTile() == tile)
            {
                tile.UnitPresent = unit;
                return;
            }
        });
    }

    public List<UnitListData> GetUnitListData()
    {
        List<UnitListData> unitData = new List<UnitListData>();
        units.ForEach((unit) => {
            unitData.Add(new UnitListData(unit.unitType.ToString(), unit.currentHealth.ToString(), unit.attack.ToString(), unit));
        });
        return unitData;
    }

    public void AddIfNotInList(UnitController unit)
    {
	    if (!units.Contains(unit))
	    {
		    units.Add(unit);
	    }
	}

    public int HighlitUnits()
    {
        int availableUnits = 0;
        foreach (UnitController unit in units)
        {
            if (!unit.CanPlaceFortOnTile())
            {
                continue;
            }
            if (!unit.canPlaceFort)
            {
                continue;
            }
            unit.unitMove.UnitShow();
            availableUnits++;
        }
        return availableUnits;
    }

    public void UnhighlitUnits()
    {
        foreach (UnitController unit in units)
        {
            unit.unitMove.Deactivate();
        }
    }

    public void DoTurn()
    {
	    if(gameManager.turnNumber == 1)
	    {
		    AssignStatesToUnits();
	    }
	    
	    CalculateRatios();
	    
	    AssignStatesToUnits();
	    
	    // decide if should attack a new city
	    if (playerManager.gameManager.playerTreeManager.isUnitUnlocked("Catapult"))
	    {
		    // get cities possible for attack
		    var allCities = gameManager.players.SelectMany(player => player.playerCitiesManager.cities).ToList();
		    allCities.RemoveAll(city => city.Owner == playerManager);	// remove own cities
		    allCities.RemoveAll(playerManager.citiesAttacked.Contains);		// remove cities already attacking
		    var freeWanderingUnits = units.Where(unit => unit.unitState == UnitState.FreeWandering).ToList();
	    
		    if(freeWanderingUnits.Count > 0 && allCities.Count > 0)
		    {
			    var cityWithChance = new Dictionary<City, float>();
			    foreach (var city in allCities) cityWithChance.Add(city, CalculateCityAttackChance(city));
		    }
	    }
	    
	    
	    // reassign states if needed
	    // AssignStatesToUnits();
	    
	    // TODO: assign units targets (attacking/protecting city)
	    
	    units.ForEach(unit => {
		    unit.DoTurn();
	    });
	    
    }

    // TODO: add more variables if possible
    public float CalculateCityAttackChance(City city)
    {
	    if (city.Owner == null) return 1f;

	    var cityHealth = city.health;
	    var freeWanderingUnits = units.Where(unit => unit.unitState == UnitState.FreeWandering).ToList();
	    
	    var freeWanderingUnitsNearCity = city.FindUnitsInRangeOfCity(5).Where(unit => freeWanderingUnits.Contains(unit)).ToList();
	    var freeWanderingUnitsFarFromCity = freeWanderingUnits.Except(freeWanderingUnitsNearCity).ToList();
	    var freeCatapults = freeWanderingUnits.Where(unit => unit.unitType == UnitTypes.Catapult).ToList();
	    
	    // no chance if there is no free catapult
	    if(freeCatapults.Count==0) return 0f;
	    
	    var combinedAttackNear = freeWanderingUnitsNearCity.Sum(unit => unit.attack);
	    var combinedAttackFar = freeWanderingUnitsFarFromCity.Sum(unit => unit.attack);

	    var healthAttackRatio = (int) Math.Floor(cityHealth / (combinedAttackNear + combinedAttackFar * 0.5f));

	    if (healthAttackRatio > 10) return 0f;
	    if (healthAttackRatio < 1) return 1f;

	    var chance = 0.6f;
	    
	    if(playerManager.gameManager.playerTreeManager.isNodeOfPlayerResearched(3, "Strategy", city.Owner)) chance-=0.2f;
	    if(playerManager.gameManager.playerTreeManager.isNodeOfPlayerResearched(2, "Strategy", city.Owner)) chance-=0.2f;
	    if(city.isUnderAttack) chance+=0.2f;
	    if(city.supplied) chance-=0.2f;
	    if(playerManager.gameManager.playerTreeManager.isNodeOfPlayerResearched(1, "Strategy", playerManager)) chance+=0.2f;
	    
	    return chance;
    }

    // TODO: make it more complex, for example with only one city left that is being attacked, more units should go and protect it, or when city is being surrounded by enemies units should also come back, add maximum free wandering ratio
    private void CalculateRatios()
    {
	    var citiesUnderAttack = playerManager.playerCitiesManager.attackedCitiesCount;
	    var citiesAttacking = playerManager.citiesAttacked.Count;
	    var citiesOwned = playerManager.playerCitiesManager.cities.Count;
	    
	    int initialAttackingCityRatio = 0, initialProtectingCityRatio = 0, initialFreeWanderingRatio = 20;

	    for(var i = 0; i < citiesOwned; i++)
	    {
		    initialProtectingCityRatio += 1;
		    initialFreeWanderingRatio -= 1;
	    }

	    for(var i = 0; i < citiesUnderAttack; i++)
	    {
		    initialProtectingCityRatio += 2;
		    initialFreeWanderingRatio -= 2;
		    if(initialFreeWanderingRatio <= 0) break;
	    }
	    
	    if(initialFreeWanderingRatio > 0) {
		    for(var i = 0; i< citiesAttacking; i++)
		    {
			    initialAttackingCityRatio += 1;
			    initialFreeWanderingRatio -= 1;
			    if(initialFreeWanderingRatio <= 0) break;
		    }
	    }
	    
	    attackingCityRatio = (float)initialAttackingCityRatio / 20;
	    protectingCityRatio = (float)initialProtectingCityRatio / 20;
	    freeWanderingRatio = (float)initialFreeWanderingRatio / 20;
    }

    public void AssignStatesToUnits()
	{
	    // Obliczenie aktualnych proporcji
	    int attackingCount = 0, protectingCount = 0, freeWanderingCount = 0;

	    foreach (var unit in units)
	    {
	        switch (unit.unitState)
	        {
	            case UnitState.AttackingCity: attackingCount++; break;
	            case UnitState.ProtectingCity: protectingCount++; break;
	            case UnitState.FreeWandering: freeWanderingCount++; break;
	        }
	    }

	    float currentAttackingRatio = (float)attackingCount / units.Count;
	    float currentProtectingRatio = (float)protectingCount / units.Count;
	    float currentFreeWanderingRatio = (float)freeWanderingCount / units.Count;

	    // Obliczenie różnic pomiędzy docelowymi a aktualnymi proporcjami
	    float attackingDiff = attackingCityRatio - currentAttackingRatio;
	    float protectingDiff = protectingCityRatio - currentProtectingRatio;
	    float freeWanderingDiff = freeWanderingRatio - currentFreeWanderingRatio;

	    // Kolekcje jednostek do zmiany stanu
	    List<UnitController> unitsToAttacking = new List<UnitController>();
	    List<UnitController> unitsToProtecting = new List<UnitController>();
	    List<UnitController> unitsToFreeWandering = new List<UnitController>();

	    // Kategoryzacja jednostek uwzględniająca macierz priorytetów
	    foreach (var unit in units)
	    {
	        var priorities = unitPriorityMatrix[unit.unitType];

	        if (unit.unitState == UnitState.AttackingCity && attackingDiff < 0)
	        {
	            if (priorities[0] > priorities[2]) // Lepsze do FreeWandering
	                unitsToFreeWandering.Add(unit);
	        }
	        else if (unit.unitState == UnitState.ProtectingCity && protectingDiff < 0)
	        {
	            if (priorities[0] > priorities[1]) // Lepsze do FreeWandering
	                unitsToFreeWandering.Add(unit);
	        }
	        else if (unit.unitState == UnitState.FreeWandering)
	        {
	            if (attackingDiff > 0 && priorities[2] > priorities[0]) // Lepsze do AttackingCity
	                unitsToAttacking.Add(unit);
	            else if (protectingDiff > 0 && priorities[1] > priorities[0]) // Lepsze do ProtectingCity
	                unitsToProtecting.Add(unit);
	        }
	    }

	    // Zmiana stanów jednostek z uwzględnieniem priorytetów
	    unitsToAttacking = unitsToAttacking.OrderByDescending(u => unitPriorityMatrix[u.unitType][2]).ToList();
	    unitsToProtecting = unitsToProtecting.OrderByDescending(u => unitPriorityMatrix[u.unitType][1]).ToList();
	    unitsToFreeWandering = unitsToFreeWandering.OrderByDescending(u => unitPriorityMatrix[u.unitType][0]).ToList();

	    while (attackingDiff > 0 && unitsToAttacking.Count > 0)
	    {
	        var unit = unitsToAttacking[0];
	        unitsToAttacking.RemoveAt(0);
	        unit.unitState = UnitState.AttackingCity;
	        attackingDiff -= 1f / units.Count;
	    }

	    while (protectingDiff > 0 && unitsToProtecting.Count > 0)
	    {
	        var unit = unitsToProtecting[0];
	        unitsToProtecting.RemoveAt(0);
	        unit.unitState = UnitState.ProtectingCity;
	        protectingDiff -= 1f / units.Count;
	    }

	    while (freeWanderingDiff > 0 && unitsToFreeWandering.Count > 0)
	    {
	        var unit = unitsToFreeWandering[0];
	        unitsToFreeWandering.RemoveAt(0);
	        unit.unitState = UnitState.FreeWandering;
	        freeWanderingDiff -= 1f / units.Count;
	    }
	}
}
