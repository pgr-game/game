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
    public Color color;

    //selecting units and settlements
    private GameObject selected;
    private GameObject newSelected;
    private Ray ray;
    RaycastHit hit;  

    //player's assets
    private List<UnitController> units;

    public void Init(GameManager gameManager)
    {
        Debug.Log("Player manager instantiated!");
        this.gameManager = gameManager;
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
    }

    GameObject SelectObject()
    {
        if (Input.GetMouseButtonDown(0)) {  
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
            if (Physics.Raycast(ray, out hit)) {  
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
        units = new List<UnitController>();
        foreach(UnitController unit in startingResources.units) {
            Debug.Log("Adding starting unit");
            UnitController newUnit = Instantiate(unit, transform.position, Quaternion.identity).GetComponent<UnitController>();
            units.Add(newUnit);
            newUnit.Init(this, mapManager, gameManager);
        }
    }

}
