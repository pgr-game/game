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
    }

    public void Activate() 
    {
        unitMove.Activate();
    }

    public void Deactivate() 
    {
        unitMove.Deactivate();
    }
}
