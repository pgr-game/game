using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public PlayerManager owner;
    public MapManager mapManager;
    public UnitMove unitMove;
    public UnitTypes unitType;
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int attackRange;
    public int baseProductionCost;
    public int turnProduced;
    public int level;
    public GameManager gameManager;

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager) {
        this.owner = playerManager;
        this.mapManager = mapManager;
        ApplyColor();
        unitMove.Init(mapManager);
        this.gameManager = gameManager;
        currentHealth = maxHealth;
        turnProduced = this.gameManager.turnNumber;
        level = 1;
    }

    public void Activate() 
    {
        unitMove.Activate();
        this.gameManager.setUnitTypeText(unitType.ToString());

    }

    public void Deactivate() 
    {
        unitMove.Deactivate();
        this.gameManager.setUnitTypeText("");

    }

    private void ApplyColor()
    {
        GameObject body = transform.Find("RotationNode/Body").gameObject;
        if(body == null) {
            Debug.Log("Unit body not found, likely the prefab structure was changed!");
            return;
        }
        Debug.Log("Setting unit color");
        body.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", owner.color);
    }

    public void Death(UnitController killer) {
        Destroy(gameObject);
        owner.allyUnits.Remove(this);
        gameManager.units.Remove(this);
        killer.owner.AddGold(CalculateGoldValue());
    }

    public int CalculateGoldValue() {
        // algorithm based on turn produced, unit type and level
        int goldValue = this.level*2;
        goldValue += gameManager.turnNumber - this.turnProduced;
    
        if(unitType == UnitTypes.Archer) goldValue+=10;
        else if(unitType == UnitTypes.Catapult) goldValue+=20;
        else if(unitType == UnitTypes.Chariot) goldValue+=15;
        else if(unitType == UnitTypes.Elephant) goldValue+=25;
        else if(unitType == UnitTypes.Hoplite) goldValue+=10;
        else if(unitType == UnitTypes.LightInfantry) goldValue+=5;
        else if(unitType == UnitTypes.Skirmisher) goldValue+=5;

        return goldValue;
    }

    public int CalculateProductionCost(City city) {
        // algorithm based on unity type, city level and age
        int productionCost = this.baseProductionCost; //based on unit type
        productionCost -= city.Level;
        productionCost -= (gameManager.turnNumber - city.turnCreated);
        return productionCost;
    }
}
