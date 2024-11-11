using RedBjorn.ProtoTiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City
{
    public string Name;
    public PlayerManager Owner;
    public MapManager mapManager;
    public int Level;
    public GameObject unitInProductionPrefab;
    public UnitController UnitInProduction;
    public int UnitInProductionTurnsLeft;
    public List<CityTile> cityTiles;
    public Vector3 uiAnchor;
    public int turnCreated;
    public CityUIController UI;

    public int health;
    public int maxHealth;
    private const int supplyBlockingRange = 3;
    private List<UnitController> garrisonUnits;
    public List<TileEntity> adjacentTiles { get; private set; }
    public bool besieged { get; private set; }
    public bool supplied { get; private set; }
    public List<PlayerManager> attackingPlayers { get; private set; }

    public void InitCity(MapManager mapManager, Color? playerColor, GameObject CityUIPrefab, string name)
    {
        this.mapManager = mapManager;
        garrisonUnits = new List<UnitController>();
        uiAnchor = MapManager.CalculateMidpoint(cityTiles.Select(cityTile => cityTile.transform.position).ToList());
        UI = UnityEngine.Object.Instantiate(CityUIPrefab, uiAnchor, Quaternion.identity).GetComponent<CityUIController>();
        UI.Init();
        if (playerColor != null)
        {
            UI.SetColor((Color)playerColor);
        }
        if (name != null)
        {
            this.Name = name;
            UI.SetName(name);
        }
        InitAdjacentTiles();
    }

    public void StartTurn()
    {
        if (UnitInProductionTurnsLeft != 0)
        {
            UnitInProductionTurnsLeft = UnitInProductionTurnsLeft - 1;
            if (UnitInProductionTurnsLeft == 0)
            {
	            if (Owner.gameManager.isMultiplayer)
	            {
		            Owner.playerUnitsManager.InstantiateUnitRpc(UnitInProduction.name, null, cityTiles.FirstOrDefault().transform.position);
				}
	            else
	            {
		            Owner.playerUnitsManager.InstantiateUnit(UnitInProduction.name, null, cityTiles.FirstOrDefault().transform.position);
				}
                
                UnitInProductionTurnsLeft = UnitInProduction.GetProductionTurns();

            }
            UI.SetTurnsLeft(UnitInProductionTurnsLeft);
        }
    }

    public void SetUnitInProduction(UnitController unit, GameObject unitInProductionPrefab)
    {
        this.UnitInProduction = unit;
        this.unitInProductionPrefab = unitInProductionPrefab;
        this.UnitInProductionTurnsLeft = unit.GetProductionTurns();
        UI.SetUnitInProduction(Owner.gameManager.getUnitSprite(unit.unitType));
        UI.SetTurnsLeft(UnitInProductionTurnsLeft);
    }

    private void UpdateProductionLock()
    {
        if (besieged && !supplied)
        {
            this.UnitInProduction = null;
            this.unitInProductionPrefab = null;
            this.UnitInProductionTurnsLeft = 0;
            UI.SetUnitInProduction(null);
            UI.SetTurnsLeft(-1);
        }
    }

    public void AddToGarrison(UnitController unit)
    {
        if (unit.owner == Owner)
        {
            garrisonUnits.Add(unit);
            UpdateHealth();
            Sprite sprite = Owner.gameManager.getUnitSprite(unit.unitType);
            UI.AddGarrisonedUnitIcon(sprite, unit.unitType);
        }
    }

    public void RemoveFromGarrison(UnitController unit)
    {
        garrisonUnits.Remove(unit);
        UI.RemoveGarrisonedUnitIcon(unit.unitType);
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        health = 0;
        maxHealth = 0;

        foreach (var unit in garrisonUnits)
        {
            health += unit.currentHealth;
            maxHealth += unit.maxHealth;
        }

        UI.SetHP(health, maxHealth);
    }

    public int GetDefense()
    {
        return 1;
    }

    public void ReceiveDamage(int incomingDamage, UnitController attacker)
    {
        for (int i = 0; i < garrisonUnits.Count; i++)
        {
            garrisonUnits[i].ReceiveDamage(incomingDamage / garrisonUnits.Count, attacker);
        }

        UpdateHealth();

        if (this.health <= 0)
        {
            this.Death(attacker);
        }
    }

    public void Death(UnitController killer)
    {
        if(Owner)
        {
            Owner.playerCitiesManager.cities.Remove(this);
        }
        killer.owner.playerCitiesManager.cities.Add(this);
        Owner = killer.owner;
        UI.SetColor(Owner.color);
        killer.owner.AddGold(Level * 100);
        killer.GainXP(this.Level);
        UpdateHealth();
        UpdateBesiegedStatus();
    }

    public void CreateSupplyLine()
    {
        Owner.playerSupplyManager.OpenSupplyLineDrawer(this);
    }

    private void InitAdjacentTiles()
    {
        List<TileEntity> tiles = cityTiles.Select(x => x.tile).ToList();
        adjacentTiles = mapManager.GetTilesSurroundingArea(tiles, 1, false);

        List<TileEntity> tilesBlockingSupply = mapManager.GetTilesSurroundingArea(tiles, supplyBlockingRange, false);
        foreach (var tile in tilesBlockingSupply)
        {
            tile.CitiesBlockingSupply.Add(this);
        }
    }

    public void UpdateSuppliedStatus()
    {
        supplied = cityTiles.Any(cityTile => cityTile.tile.SupplyLineProvider == Owner);

        UI.SetSuppliedStatus(supplied);
        UpdateProductionLock();
    }

    public void UpdateBesiegedStatus()
    {
        UpdateSuppliedStatus();

        attackingPlayers = new List<PlayerManager>();
        List<Fort> adjacentForts = adjacentTiles.Select(tile => tile.FortPresent).OfType<Fort>().ToList();
        foreach (var fort in adjacentForts)
        {
            if(fort?.owner != Owner)
            {
                attackingPlayers.Add(fort?.owner);
            }
        }

        besieged = attackingPlayers.Count != 0;

        UI.SetBesiegedStatus(besieged);

        UpdateProductionLock();
    }
}