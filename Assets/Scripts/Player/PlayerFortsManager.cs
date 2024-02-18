using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerFortsManager
{
    private List<Fort> forts;
    private PlayerManager playerManager;
    private MapManager mapManager;


    public void Init(PlayerManager playerManager) {
        this.forts = new List<Fort>();
        this.playerManager = playerManager;
        this.mapManager = playerManager.mapManager;
    }

    public void AddFort(UnitController unit) {
        Debug.Log("Adding fort");
        int id = forts.Count;
        Vector3Int hexPosition = unit.unitMove.hexPosition;
        
        Vector3 mapPos = playerManager.mapManager.MapEntity.WorldPosition(hexPosition);
        var position = new Vector3(mapPos.x, mapPos.y, -1);
        GameObject fort = GameObject.Instantiate(playerManager.fortPrefab, position, Quaternion.identity);
        int result = fort.GetComponent<Fort>().Init(id, playerManager, hexPosition);
        
        if(result == 1) forts.Add(fort.GetComponent<Fort>());
        else {
          Debug.LogError("Could not add fort");
          //Destroy(fort);  
        } 
    }
}