using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City : MonoBehaviour
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
                Owner.InstantiateUnit(UnitInProduction, null);
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
}
