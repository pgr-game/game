using RedBjorn.ProtoTiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerUnitsManager
{
    private PlayerManager playerManager;
    private GameManager gameManager;
    private List<UnitController> units = new List<UnitController>();
    public void Init(PlayerManager playerManager, StartingResources startingResources)
    {
        this.playerManager = playerManager;
        this.gameManager = playerManager.gameManager;

        if (startingResources == null)
        {
            Debug.Log("No starting resources for player!");
            return;
        }

        if (startingResources.unitLoadData.Count > 0)
        {
            var unitControllerAndLoadData = startingResources.units.Zip(
                startingResources.unitLoadData, (u, d) => new { UnitController = u, UnitLoadData = d }
            );

            foreach (var ud in unitControllerAndLoadData)
            {
                InstantiateUnit(ud.UnitController, ud.UnitLoadData, playerManager.transform.position);
            }
        }
        else
        {
            // instantiate new without loading health, movement left etc.
            foreach (UnitController unit in startingResources.units)
            {
                InstantiateUnit(unit, null, playerManager.transform.position);
            }
        }

        //add units to city garrison
        foreach (UnitController unit in units)
        {
            var path = playerManager.mapManager.MapEntity.PathTiles(unit.transform.position, unit.transform.position, 1);
            var tile = path.Last();
            if (tile.CityTilePresent)
            {
                tile.CityTilePresent.city.AddToGarrison(unit);
            }
        }
    }

    //[Rpc(SendTo.Server)]
    public UnitController InstantiateUnit(UnitController unitController, UnitLoadData unitLoadData, Vector3 position)
    {
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
        newUnit.Init(playerManager, playerManager.mapManager, playerManager.gameManager, playerManager.gameManager.unitStatsMenuController, rangeLeft, longPathClickPosition);

        if (gameManager.isMultiplayer)
        {
	        //var instanceNetworkObject = newUnit.GetComponent<NetworkObject>();
	        //instanceNetworkObject.Spawn();
        }

		return newUnit;
    }

    public void RemoveUnit(UnitController unit)
    {
        units.Remove(unit);
        gameManager.units.Remove(unit);
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
}
