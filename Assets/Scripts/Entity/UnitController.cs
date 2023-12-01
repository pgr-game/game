using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public PlayerManager owner;
    public MapManager mapManager;
    public UnitMove unitMove;

    public void Init(PlayerManager playerManager, MapManager mapManager) {
        this.owner = playerManager;
        this.mapManager = mapManager;
        ApplyColor();
        unitMove.Init(mapManager);
    }

    public void Activate() 
    {
        unitMove.Activate();
    }

    public void Deactivate() 
    {
        unitMove.Deactivate();
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
}
