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
			    foreach (var city in allCities) cityWithChance.Add(city, calculateCityAttackChance(city));
		    }
	    }
	    
	    
	    // reassign states if needed
	    // AssignStatesToUnits();
	    
	    // TODO: assign units targets (attacking/protecting city)
	    
	    units.ForEach(unit => {
		    unit.DoTurn();
	    });
	    
    }

    public float calculateCityAttackChance(City city)
    {
	    return 0.5f;
    }

    // TODO: make it more complex, for example with only one city left that is being attacked, more units should go and protect it, or when city is being surrounded by enemies units should also come back
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
	    // check if current state needs changing
	    int attackingCount = 0, protectingCount = 0, freeWanderingCount = 0;

	    foreach (var unit in units)
	    {
		    if(unit.unitState == UnitState.AttackingCity)
		    {
			    attackingCount++;
		    }
		    else if(unit.unitState == UnitState.ProtectingCity)
		    {
			    protectingCount++;
		    }
		    else if(unit.unitState == UnitState.FreeWandering)
		    {
			    freeWanderingCount++;
		    }
	    }
	    
	    float currentAttackingRatio = (float)attackingCount / units.Count;
	    float currentProtectingRatio = (float)protectingCount / units.Count;
	    float currentFreeWanderingRatio = (float)freeWanderingCount / units.Count;
	    
	    // calculate diff between target and current ratios
	    float attackingDiff = attackingCityRatio - currentAttackingRatio;
	    float protectingDiff = protectingCityRatio - currentProtectingRatio;
	    float freeWanderingDiff = freeWanderingRatio - currentFreeWanderingRatio;

	    // list of units that need to change state
	    List<UnitController> unitsToAttacking = new List<UnitController>();
	    List<UnitController> unitsToProtecting = new List<UnitController>();
	    List<UnitController> unitsToFreeWandering = new List<UnitController>();

	    // categorization
	    foreach (var unit in units)
	    {
	        if (unit.unitState == UnitState.AttackingCity && attackingDiff < 0)
	            unitsToFreeWandering.Add(unit);
	        else if (unit.unitState == UnitState.ProtectingCity && protectingDiff < 0)
	            unitsToFreeWandering.Add(unit);
	        else if (unit.unitState == UnitState.FreeWandering)
	        {
	            if (attackingDiff > 0)
	                unitsToAttacking.Add(unit);
	            else if (protectingDiff > 0)
	                unitsToProtecting.Add(unit); 
	        }
	    }

	    // change units states
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
