using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;

public class PlayerManager : MonoBehaviour
{
    //Evolution Tree Progress
    public Dictionary<int, List<string>> powerEvolution = new Dictionary<int, List<string>>();
    public Dictionary<int, List<string>> strategyEvolution = new Dictionary<int, List<string>>();
    public (int, string) researchNode = (-1, "NONE");

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

    public void Init(GameManager gameManager, MapManager mapManager, StartingResources startingResources, Color32 color, string startingCityName, int index)
    {
        this.index = index;
        this.gameManager = gameManager;
        this.mapManager = mapManager;
        this.startingResources = startingResources;
        this.color = color;
        InitTree(startingResources.treeLoadData);
        InitCities(startingCityName, startingResources.cityLoadData);
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
                else if(newSelected.GetComponent<CityTile>()&&selected == null) {
                    CityTile cityTile = newSelected.GetComponent<CityTile>();
                    HandleCityClick(cityTile.city);
                }
            }
            if (Input.GetKeyDown(KeyCode.B) && selected != null && this.gameManager.playerTreeManager.isNodeResearched(1,"Strategy")) {
                if(!selected.GetComponent<UnitController>().CanPlaceFortOnTile()) {
                    Debug.Log("Fort can't be placed here");     // maybe add dialog box in the future
                    return;
                }
                if(!selected.GetComponent<UnitController>().canPlaceFort) {
                    Debug.Log("Unit can't place fort yet");     // maybe add dialog box in the future
                    return;
                }
                CreateFort();
            }

        } 
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f), transform.TransformDirection(Vector3.forward), Color.green);
    }

    public void Deselect()
    {
        this.selected = null;
    }

    GameObject SelectObject()
    {
        if (Input.GetMouseButtonDown(0)) {  
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
            if (Physics.Raycast(ray, out hit)) {  
                return hit.transform.gameObject;
            }
            return null;
        }  
        return null;
    }

    private void HandleUnitClick(UnitController currentUnit) {
        if(currentUnit.owner != this) {
            return;
        }
        if(currentUnit && newSelected == selected) {
            //unselect
            selected = null;
            newSelected = null;
            currentUnit.Deactivate();
        }
        else if(currentUnit) {
            if(selected) {
                selected.GetComponent<UnitController>().Deactivate();
            }
            //select if nothing else is selected
            selected = newSelected;
            currentUnit.Activate();
        }
    }

    private void HandleCityClick(City city) {
        if(city.Owner == null) {
            return;
        }
        if(city.Owner != this) {
            return;
        }
        if(gameManager.cityMenuManager.city == city && gameManager.cityMenuManager.gameObject.activeSelf) {
            gameManager.cityMenuManager.setValues(null);
            gameManager.cityMenuManager.Deactivate();
            isInMenu = false;
            return;
        }
        isInMenu = false;
        gameManager.cityMenuManager.setValues(city);
        gameManager.cityMenuManager.Activate();
    }

    void InitUnits() {
        if(startingResources == null) {
            Debug.Log("No starting resources for player!");
            return;
        } 

        if(startingResources.unitLoadData.Count > 0) {
            var unitControllerAndLoadData = startingResources.units.Zip(
                startingResources.unitLoadData, (u, d) => new { UnitController = u, UnitLoadData = d }
            );

            foreach(var ud in unitControllerAndLoadData)
            {
                InstantiateUnit(ud.UnitController, ud.UnitLoadData, transform.position);
            }
        }
        else 
        {
            // instantiate new without loading health, movement left etc.
            foreach(UnitController unit in startingResources.units) 
            {
                InstantiateUnit(unit, null, transform.position);
            }
        }

        //add units to city garrison
        foreach(UnitController unit in allyUnits) 
        {
            var path = mapManager.MapEntity.PathTiles(unit.transform.position, unit.transform.position, 1);
            var tile = path.Last();
            if (tile.CityTilePresent)
            {
                tile.CityTilePresent.city.AddToGarrison(unit);
            }
        }
        
    }

    public UnitController InstantiateUnit(UnitController unitController, UnitLoadData unitLoadData, Vector3 position) {
        float? rangeLeft = null;
        Vector3? longPathClickPosition = null;
        if (unitLoadData != null) {
            position = unitLoadData.position;
            unitController.maxHealth = unitLoadData.maxHealth;
            unitController.currentHealth = unitLoadData.currentHealth;
            unitController.attack = unitLoadData.attack;
            unitController.attackRange = unitLoadData.attackRange;
            unitController.baseProductionCost = unitLoadData.baseProductionCost;
            unitController.turnsToProduce = unitLoadData.turnsToProduce;
            unitController.turnProduced = unitLoadData.turnProduced;
            unitController.level = unitLoadData.level;
            unitController.turnsToProduce = unitLoadData.turnsToProduce;
            unitController.experience = unitLoadData.experience;
            rangeLeft = unitLoadData.rangeLeft;
            unitController.attacked = unitLoadData.attacked;
            longPathClickPosition = unitLoadData.longPathClickPosition;
        }

        UnitController newUnit = Instantiate(unitController, position, Quaternion.identity).GetComponent<UnitController>();
        allyUnits.Add(newUnit);
        newUnit.Init(this, mapManager, gameManager, gameManager.unitStatsUIController, rangeLeft, longPathClickPosition);
        return newUnit;
    }
    void InitTree(TreeLoadData treeLoadData)
    {
        //call player tree manager here to init once it's implemented\
        if (treeLoadData != null)
        {
            powerEvolution = treeLoadData.powerEvolution;
            strategyEvolution = treeLoadData.strategyEvolution;
            researchNode = treeLoadData.researchNode;
        }

        this.gameManager.playerTreeManager.populateEvolutionTrees(this);
    }

    void InitCities(string startingCityName, List<CityLoadData> cityLoadData) {
        playerCitiesManager = new PlayerCitiesManager();
        playerCitiesManager.Init(this, startingCityName, cityLoadData);
    }

    void InitForts() {
        playerFortsManager = new PlayerFortsManager();
        playerFortsManager.Init(this);

        if(startingResources.fortLoadData != null) {
            foreach(FortLoadData fort in startingResources.fortLoadData) {
                playerFortsManager.AddFort(fort.hexPosition, fort.id);
            }
        }
    }

    public void CreateFort() {
        Vector3Int hexPosition = selected.GetComponent<UnitController>().unitMove.hexPosition;
        int result = playerFortsManager.AddFort(hexPosition, 0);
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

    public void StartFirstTurn()
    {
        //first turn after game start or load. Healing, forts and reset range disabled
        SetGoldText(gold.ToString());
        SetGoldIncome();
    }

    public void StartTurn() {
        allyUnits.ForEach((unit) => {
            unit.attacked = false;
            unit.unitMove.ResetRange();
            if(unit.CanHealOrGetDefenceBonus()) unit.Heal();
            unit.CommitToBuildingFort();
            if(gameManager.turnNumber != 1) {
                unit.turnsSinceFortPlaced++;
                if(unit.turnsSinceFortPlaced == 10) unit.canPlaceFort = true;
            }
        });

        if (gameManager.turnNumber != 1) {
            AddGold(playerCitiesManager.GetGoldIncome());
        }
        SetGoldText(gold.ToString());
        SetGoldIncome();
        gold += goldIncome;
        playerCitiesManager.StartCitiesTurn();
    }

    public UnitController getSelectedUnit() {
        if(selected == null) {
            return null;
        }
        return selected.GetComponent<UnitController>();
    }

    // probably will be deleted when we implement multiple units on a tile
    public void ResetUnitPresentOnTile(TileEntity tile, UnitController currentUnit) {
        allyUnits.ForEach((unit) => {
            if(currentUnit != unit && unit.GetCurrentTile() == tile) {
                tile.UnitPresent = unit;
                return;
            }
        });
    }

    public void SelectUnitFromList(UnitController selectedUnit) {
        if(selected) selected.GetComponent<UnitController>().Deactivate();
        selected = selectedUnit.gameObject;
        selectedUnit.Activate();
    }

    public bool IsPlayerAlive() {
        bool isAlive = false;
        if(allyUnits.Count > 0) {
            isAlive = true;
        }
        if(playerCitiesManager.GetNumberOfCities() > 0) {
            isAlive = true;
        }
        return isAlive;
    }

}
