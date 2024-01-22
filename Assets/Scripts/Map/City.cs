using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City
{
    public string Name;
    public PlayerManager Owner;
    public int Level {
        get {
            return Level;
        }
        set {
            Level = value;
            UI.SetLevel(Level.ToString());
        }
    }
    public UnitController UnitInProduction;
    public int UnitInProductionTurnsLeft;
    public List<CityTile> cityTiles;
    public Vector3 uiAnchor;
    public CityUIController UI;

    public void InitCityUI(PlayerManager player, GameObject CityUIPrefab) {
        Debug.Log(cityTiles.Select(cityTile => cityTile.transform.position).ToList());
        uiAnchor = MapManager.CalculateMidpoint(cityTiles.Select(cityTile => cityTile.transform.position).ToList());
        UI = UnityEngine.Object.Instantiate(CityUIPrefab, uiAnchor, Quaternion.identity).GetComponent<CityUIController>();
        UI.Init();
        if(Owner) {
            UI.SetColor(Owner.color);
        }
    }

}
