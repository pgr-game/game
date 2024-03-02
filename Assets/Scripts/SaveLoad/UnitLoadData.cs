using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLoadData
{
    public UnitLoadData(Vector3 position, string unitType, int maxHealth, int currentHealth, 
    int attack, int attackRange, int baseProductionCost, int turnsToProduce, 
    int turnProduced, int level)
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
    }
    public Vector3 position { get; private set; }
    string unitType;
    int maxHealth;
    int currentHealth;
    int attack;
    int attackRange;
    int baseProductionCost;
    int turnsToProduce;
    int turnProduced;
    int level;
}
