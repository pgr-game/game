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
    public int index;

    //selecting units and settlements
    private bool isInMenu = false;
    private GameObject selected;
    private GameObject newSelected;
    private Ray ray;
    RaycastHit hit;  

    //player's assets
    public List<UnitController> allyUnits = new List<UnitController>();
    public PlayerCitiesManager playerCitiesManager;
    public PlayerFortsManager playerFortsManager;
    public GameObject fortPrefab;

    // currency
    public int gold;
    public GameObject goldText;
    public int goldIncome = 5;     // amount given to player every round independently of cities, units etc.
    public const int costOfFort = 100;

    public void Init(GameManager gameManager, string startingCityName, int index)
    {
        Debug.Log("Player manager instantiated!");
        this.index = index;
        this.gameManager = gameManager;
        InitCities(startingCityName);
        InitForts();
        InitUnits();
        this.gold = startingResources.gold;
        GameObject[] texts = GameObject.FindGameObjectsWithTag("currencyText");
        this.goldText = texts[0];
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
            if (Input.GetKeyDown(KeyCode.B) && selected != null) {
                Debug.Log("Trying to place a fort");
                if(selected.GetComponent<UnitController>().IsInFortOrCity()) {
                    Debug.Log("Fort can't be placed here");
                }
                else if(!selected.GetComponent<UnitController>().canPlaceFort) {
                    Debug.Log("Unit can't place fort yet");
                }
                else {
                    CreateFort();
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

    void InitForts() {
        playerFortsManager = new PlayerFortsManager();
        playerFortsManager.Init(this);
    }

    public void CreateFort() {
        int result = playerFortsManager.AddFort(selected.GetComponent<UnitController>());
        if(result == 1) {
            selected.GetComponent<UnitController>().canPlaceFort = false;
            selected.GetComponent<UnitController>().turnsSinceFortPlaced = 0;
            gold -= costOfFort;
        }
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

    public void DeactivateUnitsRange()
    {
        foreach (UnitController unit in allyUnits)
        {
            unit.Deactivate();
        }
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
        allyUnits.ForEach((unit) => unit.attacked = false);
        if (gameManager.turnNumber != 1) {
            allyUnits.ForEach((unit) => {
                unit.turnsSinceFortPlaced++;
                if(unit.turnsSinceFortPlaced == 5) unit.canPlaceFort = true;
            });
        } 

        allyUnits.ForEach((unit) => {
            if(unit.IsInFortOrCity()) unit.Heal();
            
        });


        if (gameManager.turnNumber != 1) {
            AddGold(playerCitiesManager.GetGoldIncome());
        }
        SetGoldText(gold.ToString());
        SetGoldIncome();
        gold += goldIncome;
        allyUnits.ForEach(unit => unit.unitMove.ResetRange());
        playerCitiesManager.StartCitiesTurn();
    }

    public UnitController getSelectedUnit() {
        if(selected == null) {
            return null;
        }
        return selected.GetComponent<UnitController>();
    }

}
