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
    public GameManager gameManager;

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager) {
        this.owner = playerManager;
        this.mapManager = mapManager;
        ApplyColor();
        unitMove.Init(mapManager);
        this.gameManager = gameManager;
        currentHealth = maxHealth;
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

    public void death() {
        Destroy(gameObject);
        owner.allyUnits.Remove(this);
        gameManager.units.Remove(this);
    }
}
