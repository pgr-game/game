using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerFortsManager
{
    public List<Fort> forts {get; private set;}
    private PlayerManager playerManager;
    private MapManager mapManager;


    public void Init(PlayerManager playerManager) {
        this.forts = new List<Fort>();
        this.playerManager = playerManager;
        this.mapManager = playerManager.mapManager;
    }

    public int AddFort(Vector3Int hexPosition, int id) {
        Debug.Log("Adding fort");

        if(id == 0) {
            id = forts.Count;
        }
        
        Vector3 mapPos = playerManager.mapManager.MapEntity.WorldPosition(hexPosition);
        var position = new Vector3(mapPos.x, mapPos.y, -1);
        GameObject fort = GameObject.Instantiate(playerManager.fortPrefab, position, Quaternion.identity);
        int result = fort.GetComponent<Fort>().Init(id, playerManager, hexPosition);
        
        if(result == 1) {
            forts.Add(fort.GetComponent<Fort>());
            return 1;
        }   
        else {
          Debug.LogError("Could not add fort");
          return 0;
          //Destroy(fort);  
        } 
    }
}