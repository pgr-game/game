using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerManager : MonoBehaviour
{
    //assigned by game manager
    public MapManager mapManager;
    public StartingResources startingResources;
    public GameManager gameManager;
    public bool isComputer;
    public Color32 color;

    //selecting units and settlements
    private GameObject selected;
    private GameObject newSelected;
    private Ray ray;
    RaycastHit hit;  

    //player's assets
    private List<UnitController> allyUnits = new List<UnitController>();
    public PlayerCitiesManager playerCitiesManager;

    public void Init(GameManager gameManager, string startingCityName)
    {
        Debug.Log("Player manager instantiated!");
        this.gameManager = gameManager;
        InitCities(startingCityName);
        InitUnits();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.isPaused) 
        { 
            newSelected = SelectObject();
            if(newSelected) {
                UnitController currentUnit = newSelected.GetComponent<UnitController>();
                if(currentUnit && newSelected == selected) {
                    //unselect
                    Debug.Log("Deactivating unit");
                    selected = null;
                    newSelected = null;
                    currentUnit.Deactivate();
                }
                else if(currentUnit && !selected) {
                    //select if nothing else is selected
                    Debug.Log("Activating unit");
                    selected = newSelected;
                    currentUnit.Activate();
                    HandleSelected();
                }
            }
        }
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f), transform.TransformDirection(Vector3.forward), Color.green);
    }

    GameObject SelectObject()
    {
        if (Input.GetMouseButtonDown(0)) {  
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
            if (Physics.Raycast(ray, out hit)) {  
                CityTile city = hit.transform.GetComponent<CityTile>();
                if(city != null) {
                    Debug.Log("City clicked!");
                }

                UnitController unit = hit.transform.GetComponent<UnitController>();
                if(unit == null) {
                    return null;
                } else if(unit.owner == this) {
                    Debug.Log("Selected: " + hit.transform.name);
                    return hit.transform.gameObject;
                }
            }  
        }  
        return null;
    }

    void HandleSelected()
    {
        Debug.Log("Handling selected");
    }

    void InitUnits() {
        if(startingResources == null) {
            Debug.Log("No starting resources for player!");
            return;
        } 
        foreach(UnitController unit in startingResources.units) {
            Debug.Log("Adding starting unit");
            UnitController newUnit = Instantiate(unit, transform.position, Quaternion.identity).GetComponent<UnitController>();
            allyUnits.Add(newUnit);
            newUnit.Init(this, mapManager, gameManager);
        }
    }

    void InitCities(string startingCityName) {
        playerCitiesManager = new PlayerCitiesManager();
        playerCitiesManager.Init(this, startingCityName);
    }

}
