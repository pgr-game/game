using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City
{
    public string Name;
    public PlayerManager Owner;
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
    private List<UnitController> garrisonUnits;

    public void InitCityUI(PlayerManager player, GameObject CityUIPrefab, string name) {
        garrisonUnits = new List<UnitController>();
        uiAnchor = MapManager.CalculateMidpoint(cityTiles.Select(cityTile => cityTile.transform.position).ToList());
        UI = UnityEngine.Object.Instantiate(CityUIPrefab, uiAnchor, Quaternion.identity).GetComponent<CityUIController>();
        UI.Init();
        if(Owner) {
            UI.SetColor(Owner.color);
        }
        if(name != null) {
            this.Name = name;
            UI.SetName(name);
        }
    }

    public void StartTurn() {
        if(UnitInProductionTurnsLeft != 0) {
            UnitInProductionTurnsLeft = UnitInProductionTurnsLeft - 1;
            if(UnitInProductionTurnsLeft == 0) {
                UnitController newUnit = Owner.InstantiateUnit(UnitInProduction, null);
                AddToGarrison(newUnit);
                UnitInProductionTurnsLeft = UnitInProduction.GetProductionTurns();
                
            }
            UI.SetTurnsLeft(UnitInProductionTurnsLeft);
        }
    }

    public void SetUnitInProduction(UnitController unit, GameObject unitInProductionPrefab) {
        this.UnitInProduction = unit;
        this.unitInProductionPrefab = unitInProductionPrefab;
        this.UnitInProductionTurnsLeft = unit.GetProductionTurns();
        UI.SetUnitInProduction(unitInProductionPrefab);
        UI.SetTurnsLeft(UnitInProductionTurnsLeft);
    }

    public void AddToGarrison(UnitController unit)
    {
        garrisonUnits.Add(unit);
        UpdateHealth();
    }

    public void RemoveFromGarrison(UnitController unit)
    {
        garrisonUnits.Remove(unit);
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
}
