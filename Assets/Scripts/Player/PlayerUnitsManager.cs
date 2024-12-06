using RedBjorn.ProtoTiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerUnitsManager : NetworkBehaviour
{
    private PlayerManager playerManager;
    private GameManager gameManager;
    private List<UnitController> units = new List<UnitController>();

    public void Init(PlayerManager playerManager, StartingResources startingResources, StartingUnits startingUnits)
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

        if (startingUnits != null && startingUnits.unitLoadData != null && startingUnits.unitLoadData.Count > 0)
        {
	        var unitControllerAndLoadData = startingUnits.units.Zip(
                startingUnits.unitLoadData, (u, d) => new { UnitController = u, UnitLoadData = d }
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
        else if (startingUnits != null)
        {
	        // instantiate new without loading health, movement left etc.
	        foreach (UnitController unit in startingUnits.units)
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

    public List<UnitController> GetUnitsForSave()
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
}
