using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    //Evolution Tree Progress
    public Dictionary<int, List<string>> powerEvolution = new Dictionary<int, List<string>>();
    public Dictionary<int, List<string>> strategyEvolution = new Dictionary<int, List<string>>();
    public (int, string) researchNode = (-1, "NONE");

    //assigned by game manager
    public MapManager mapManager;
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
    public PlayerUnitsManager playerUnitsManager;
    public PlayerCitiesManager playerCitiesManager;
    public PlayerFortsManager playerFortsManager;
    public PlayerSupplyManager playerSupplyManager;

    // Prefabs
    public PathDrawer pathPrefab;
    public AreaOutline passableAreaPrefab;
    public GameObject fortPrefab;
    public GameObject supplyLinePrefab;
    public GameObject hexHighlitPrefab;

    // currency
    public int gold;
    public GameObject goldText;
    public int goldIncome = 5;     // amount given to player every round independently of cities, units etc.
    public const int costOfFort = 100;

    // Multiplayer
    private PlayerData playerNetworkData;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        var gameManager = FindAnyObjectByType<GameManager>();
        if (!gameManager.isInit)
        {
	        gameManager.Init();
        }

        int index = 0;
        for (int i = 0; i < gameManager.sceneLoadData.playerPositions.Length; i++)
        {
	        if (gameManager.sceneLoadData.playerPositions[i] == transform.position)
	        {
		        index = i;
		        break;
	        }
        }
		Init(gameManager, gameManager.mapManager, gameManager.startingResources[index], 
			gameManager.sceneLoadData.playerColors[index], gameManager.sceneLoadData.startingCityNames[index], 
			gameManager.sceneLoadData.isComputer[index], index);
    }

    public void Init(GameManager gameManager, MapManager mapManager, StartingResources startingResources, Color32 color, string startingCityName, bool isComputer, int index)
    {
        this.index = index;
        this.gameManager = gameManager;
        this.mapManager = mapManager;
        this.color = color;
        if (gameManager.isMultiplayer)
        {
            var playerData = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerData>();
            
            if (playerData && this.index == playerData.index)
            {
                this.playerNetworkData = playerData;
                this.color = playerNetworkData.color;
            }
            else if (playerData && index < playerData.otherPlayersColors.Count)
            {
	            this.color = playerData.otherPlayersColors[index];
            }
        }
        this.isComputer = isComputer;
        InitTree(startingResources.treeLoadData);
        InitCities(startingCityName, startingResources.cityLoadData);
        InitForts(startingResources);
        InitSupplyLines(startingResources.supplyLoadData);
        InitUnits(startingResources);
        this.gold = startingResources.gold;
        GameObject[] texts = GameObject.FindGameObjectsWithTag("currencyText");
        this.goldText = texts[0];

        if (index == gameManager.activePlayerIndex)
        {
            gameObject.SetActive(true);
            gameManager.activePlayer = this;
            gameManager.SetPlayerUIColor(this.color);
        }
        else
        {
            gameObject.SetActive(false);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.isPaused) 
        {
            if (playerSupplyManager.drawingSupplyLine && !playerSupplyManager.justActivated)
            {
                playerSupplyManager.UpdateSupplyLineDrawer();

                if (MyInput.GetOnWorldUp(mapManager.MapEntity.Settings.Plane()))
                {
                    Vector3 clickPos = MyInput.GroundPosition(mapManager.MapEntity.Settings.Plane());
                    playerSupplyManager.CreateSupplyLine(null, clickPos);
                }
            }
            if (playerSupplyManager.drawingSupplyLine && playerSupplyManager.justActivated)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    playerSupplyManager.justActivated = false;
                }
            }
            newSelected = SelectObject();
            if(newSelected && !playerSupplyManager.drawingSupplyLine) {
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
        else
        {
            // Paused
            playerSupplyManager.ClearSupplyLineCreator();
        }
        
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f), transform.TransformDirection(Vector3.forward), Color.green);
    }

    public void DoTurn()
    {
        // TODO computer player actions
        // Now computer player just skips his turn
        Debug.Log("Computer player " + index + " turn");
        SkipTurn();
    }
    public void SkipTurn()
    {
        gameManager.NextPlayer();
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

    void InitUnits(StartingResources startingResources) {
        playerUnitsManager = new PlayerUnitsManager();
        playerUnitsManager.Init(this, startingResources);
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

    void InitSupplyLines(SupplyLoadData supplyLoadData)
    {
        playerSupplyManager = new PlayerSupplyManager();
        playerSupplyManager.Init(this, supplyLoadData,hexHighlitPrefab);
    }

    void InitForts(StartingResources startingResources) {
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

    public void SetGoldIncome() {
        // option no 1:
        if(gameManager.turnNumber % 2 == 0) {
            goldIncome += 1;
        }
        // option no 2: (if the gold income would increase too fast with the first method)
        // goldIncome = 5 + gameManager.turn/2;

        // here we can do some more advanced calculations, for example based on type of unit
        int goldForUnits = playerUnitsManager.GetUnitCount() / 2;
        goldIncome += goldForUnits;
        // here we can do some more advanced calculations, for example based on level of city
        int goldForCities = playerCitiesManager.GetNumberOfCities()*2;
        goldIncome += goldForCities;
    }

    public void StartFirstTurn()
    {
        //first turn after game start or load. Healing, forts and reset range disabled
        SetGoldText(gold.ToString());
        SetGoldIncome();
        HandleComputerOrMultiplayerActions();
    }

    private void HandleComputerOrMultiplayerActions()
    {
        if (gameManager.isMultiplayer)
        {
            if (isComputer && NetworkManager.Singleton.IsHost)
            {
                DoTurn();
            }
            if (playerNetworkData == null || playerNetworkData?.index != this.index)
            {
                Debug.Log("Another player plays his turn");
            }
        } else if (isComputer) DoTurn();
    }

    public void StartTurn() {
        playerSupplyManager.CheckSupplyLines();
        playerUnitsManager.StartUnitsTurn();

        if (gameManager.turnNumber != 1) {
            AddGold(playerCitiesManager.GetGoldIncome());
        }
        SetGoldText(gold.ToString());
        SetGoldIncome();
        gold += goldIncome;
        HandleComputerOrMultiplayerActions();
    }

    public UnitController getSelectedUnit() {
        if(selected == null) {
            return null;
        }
        return selected.GetComponent<UnitController>();
    }

    public void SelectUnitFromList(UnitController selectedUnit) {
        if(selected) selected.GetComponent<UnitController>().Deactivate();
        selected = selectedUnit.gameObject;
        selectedUnit.Activate();
    }

    public bool isAlive() {
	    bool isAlive = playerUnitsManager.GetUnitCount() > 0;
	    if(playerCitiesManager.GetNumberOfCities() > 0) {
		    isAlive = true;
	    }
	    return isAlive;
    }
}
