using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitListData
{
    public string unitType;
    public string health;
    public string attack;
    public UnitController unit;

    public UnitListData(string unitType, string health, string attack, UnitController unit)
    {
        this.unitType = unitType;
        this.health = health;
        this.attack = attack;
        this.unit = unit;
    }

}
