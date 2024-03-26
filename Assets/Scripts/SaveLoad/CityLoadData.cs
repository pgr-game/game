using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityLoadData
{
    public CityLoadData(Vector3 position, string name, int level, 
        string unitInProduction, int unitInProductionTurnsLeft)
    {
        this.position = position;
        this.name = name;
        this.level = level;
        this.unitInProduction = unitInProduction;
        this.unitInProductionTurnsLeft = unitInProductionTurnsLeft;
    }
    public Vector3 position;
    public string name;
    public int level;
    public string unitInProduction;
    public int unitInProductionTurnsLeft;
}
