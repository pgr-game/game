using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityTile : MonoBehaviour
{
    public City city;
    //this is debug only and should not be used
    public PlayerManager owner;

    public GameManager gameManager;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(GameManager gameManager) {
        this.gameManager = gameManager;
        var tile = gameManager.mapManager.MapEntity.Tile(this.transform.position);
        tile.CityTilePresent = this.GetComponent<CityTile>();
    }

    public void ClaimStartingCityTile(PlayerManager playerManager, City city) {
        this.city = city;
        this.city.Owner = playerManager;
        this.owner = playerManager;
    }
}
