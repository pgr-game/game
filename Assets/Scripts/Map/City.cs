using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City
{
    public string Name;
    public PlayerManager Owner;
    public int Level = 1;
    public UnitController UnitInProduction;
    public int UnitInProductionTurnsLeft;
    public List<CityTile> cityTiles;

}
