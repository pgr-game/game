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

    public void InitCityUI(PlayerManager player, GameObject CityUIPrefab, string name) {
        Debug.Log(cityTiles.Select(cityTile => cityTile.transform.position).ToList());
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

    public void SetUnitInProduction(UnitController unit, GameObject unitInProductionPrefab) {
        this.UnitInProduction = unit;
        this.unitInProductionPrefab = unitInProductionPrefab;
        UI.SetUnitInProduction(unitInProductionPrefab);
        UI.SetTurnsLeft(unit.GetProductionTurns());
    }
}