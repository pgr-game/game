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
    private bool isInMenu = false;
    private GameObject selected;
    private GameObject newSelected;
    private Ray ray;
    RaycastHit hit;  

    //player's assets
    public List<UnitController> allyUnits = new List<UnitController>();
    public PlayerCitiesManager playerCitiesManager;

    // currency
    private int gold;
    public GameObject goldText;
    public int goldIncome = 5;     // amount given to player every round independently of cities, units etc.

    public void Init(GameManager gameManager, string startingCityName)
    {
        Debug.Log("Player manager instantiated!");
        this.gameManager = gameManager;
        InitCities(startingCityName);
        InitUnits();
        gold = startingResources.gold;
        GameObject[] texts = GameObject.FindGameObjectsWithTag("currencyText");
        goldText = texts[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.isPaused) 
        { 
            newSelected = SelectObject();
            if(newSelected) {
                if(newSelected.GetComponent<UnitController>() && !isInMenu) {
                    UnitController currentUnit = newSelected.GetComponent<UnitController>();
                    HandleUnitClick(currentUnit);
                }
                else if(newSelected.GetComponent<CityTile>()) {
                    CityTile cityTile = newSelected.GetComponent<CityTile>();
                    HandleCityClick(cityTile.city);
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
                Debug.Log("Selected: " + hit.transform.name);
                return hit.transform.gameObject;
            }
            return null;
        }  
        return null;
    }

    void HandleSelectedUnit()
    {
        Debug.Log("Handling selected");
    }

    private void HandleUnitClick(UnitController currentUnit) {
        if(currentUnit.owner != this) {
            return;
        }
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
            HandleSelectedUnit();
        }
    }

    private void HandleCityClick(City city) {
        if(city.Owner == null) {
            return;
        }
        if(city.Owner != this) {
            return;
        }
        if(gameManager.cityMenuManager.city == city) {
            gameManager.cityMenuManager.setValues(null);
            gameManager.cityMenuManager.Deactivate();
            isInMenu = false;
            return;
        }
        Debug.Log(city.Name);
        isInMenu = false;
        gameManager.cityMenuManager.setValues(city);
        gameManager.cityMenuManager.Activate();
    }

    void InitUnits() {
        if(startingResources == null) {
            Debug.Log("No starting resources for player!");
            return;
        } 
        foreach(UnitController unit in startingResources.units) {
            Debug.Log("Adding starting unit");
            InstantiateUnit(unit);
        }
    }

    public void InstantiateUnit(UnitController unitController) {
        UnitController newUnit = Instantiate(unitController, transform.position, Quaternion.identity).GetComponent<UnitController>();
        allyUnits.Add(newUnit);
        newUnit.Init(this, mapManager, gameManager);
    }

    void InitCities(string startingCityName) {
        playerCitiesManager = new PlayerCitiesManager();
        playerCitiesManager.Init(this, startingCityName);
    }

    public void AddGold(int amount) {
        gold += amount;
        SetGoldText(gold.ToString());
    }

    public void RemoveGold(int amount) {
        gold -= amount;
        SetGoldText(gold.ToString());
    }

    public void SetGoldText(string gold) {
        goldText.GetComponent<TMPro.TextMeshProUGUI>().text = "gold: " + gold;
    }

    public void SetGoldIncome() {
        // option no 1:
        if(gameManager.turnNumber % 2 == 0) {
            goldIncome += 1;
        }
        // option no 2: (if the gold income would increase too fast with the first method)
        // goldIncome = 5 + gameManager.turn/2;

        // here we can do some more advanced calculations, for example based on type of unit
        int goldForUnits = this.allyUnits.Count / 2;
        goldIncome += goldForUnits;
        // here we can do some more advanced calculations, for example based on level of city
        int goldForCities = this.playerCitiesManager.GetNumberOfCities()*2;
        goldIncome += goldForCities;
    }

    public void StartTurn() {
        if(gameManager.turnNumber != 1) {
            AddGold(playerCitiesManager.GetGoldIncome());
        }
        SetGoldText(gold.ToString());
        SetGoldIncome();
        gold += goldIncome;
        allyUnits.ForEach(unit => unit.unitMove.ResetRange());
        playerCitiesManager.StartCitiesTurn();
    }

}
