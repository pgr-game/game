using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityTile : MonoBehaviour
{
    public City city;
    //this is debug only and should not be used
    public PlayerManager owner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClaimStartingCityTile(PlayerManager playerManager, City city) {
        this.city = city;
        this.city.Owner = playerManager;
        this.owner = playerManager;
    }
}
