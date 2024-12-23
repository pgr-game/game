using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using Unity.Netcode;

public class PlayerFortsManager : NetworkBehaviour
{
    public List<Fort> forts { get; private set; }
    private PlayerManager playerManager;
    private MapManager mapManager;
    public AreaOutline AreaPrefab;

    public bool creatingFort = false;
    public bool deletingFort = false;
    public bool justActivated = false;


    public void Init(PlayerManager playerManager)
    {
        this.forts = new List<Fort>();
        this.playerManager = playerManager;
        this.mapManager = playerManager.mapManager;
    }

    [Rpc(SendTo.Everyone)]
    public void AddFortRpc(Vector3Int hexPosition, int id)
    {
        AddFort(hexPosition, id);
    }

    public void AddFort(Vector3Int hexPosition, int id)
    {
        Debug.Log("Adding fort");

        if (id == 0)
        {
            id = forts.Count;
        }
        Vector3 mapPos = playerManager.mapManager.MapEntity.WorldPosition(hexPosition);
        var position = new Vector3(mapPos.x, mapPos.y, -1);
        GameObject fort = GameObject.Instantiate(playerManager.fortPrefab, position, Quaternion.identity);
        fort.GetComponent<Fort>().Init(id, playerManager, hexPosition, AreaPrefab);

        forts.Add(fort.GetComponent<Fort>());
    }

    public int HighlightAllForts()
    {
        int availableForts = 0;
        foreach (var fort in forts)
        {
            fort.UnitShow();
            availableForts++;
        }
        return availableForts;
    }

    public void UnhighlightAllForst()
    {
        foreach (var fort in forts)
        {
            fort.AreaHide();
        }
    }

    public void RemoveFort(Fort fort, City adjacentCity)
    {
        forts.Remove(fort);
        GameObject.Destroy(fort.gameObject);

        if (adjacentCity != null)
        {
            adjacentCity.UpdateBesiegedStatus();
        }
    }
}