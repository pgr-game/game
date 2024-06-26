using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLoadData
{
    public UnitLoadData(Vector3 position, string unitType, int maxHealth, int currentHealth, 
    int attack, int attackRange, int baseProductionCost, int turnsToProduce, 
    int turnProduced, int level, int experience, float rangeLeft, bool attacked,
    Vector3 longPathClickPosition)
    {
        this.position = position;
        this.unitType = unitType;
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;
        this.attack = attack;
        this.attackRange = attackRange;
        this.baseProductionCost = baseProductionCost;
        this.turnsToProduce = turnsToProduce;
        this.turnProduced = turnProduced;
        this.level = level;
        this.experience = experience;
        this.rangeLeft = rangeLeft;
        this.attacked = attacked;
        this.longPathClickPosition = longPathClickPosition;
    }
    public Vector3 position { get; private set; }
    public string unitType;
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int attackRange;
    public int baseProductionCost;
    public int turnsToProduce;
    public int turnProduced;
    public int level;
    public int experience;
    public float rangeLeft;
    public bool attacked;
    public Vector3 longPathClickPosition;
}
