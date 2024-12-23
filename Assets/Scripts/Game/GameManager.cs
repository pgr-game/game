using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using RedBjorn.ProtoTiles;
using TMPro;
using UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


// TYPES OF UNITS
public enum UnitTypes {
    Archer,
    Catapult,
    Chariot,
    Elephant,
    Hoplite,
    LightInfantry,
    Skirmisher
}

[GenerateSerializationForType(typeof(SceneLoadData))]
[GenerateSerializationForType(typeof(StartingResources))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public MapManager mapManager;
    public SaveManager saveManager;
    public LoadManager loadManager;
    public GameObject soundManager;
    public CityMenuManager cityMenuManager;
    public PauseMenu pauseMenu;
    public GameObject playerPrefab;
    public GameSettings gameSettings;
    public PlayerTreeManager playerTreeManager;
    public FortButtonManager fortButtonManager;

    // Turn elements
    public int turnNumber = 1;
    public GameObject turnText;
    
    // Player elements
    public int activePlayerIndex = 0;
    public PlayerManager activePlayer;
    public int numberOfPlayers;
    public PlayerManager[] players;
    public Vector3[] playerPositions;

    // Unit types
    private const int amountOfUnitTypes = 7;
    public GameObject[] unitPrefabs = new GameObject[amountOfUnitTypes];
    public Sprite[] unitSprites = new Sprite[amountOfUnitTypes];

    // UI elements
    public UnitStatsMenuController unitStatsMenuController;
    public NextTurnMenuController nextTurnMenuController;
    public GameObject UI;
    public TileTag cityTag;
    public DialogController dialogController;

    // Initial game state
    public SceneLoadData sceneLoadData { get; private set; }
    public StartingResources[] startingResources;
    public StartingUnits[] startingUnits;

    // Multiplayer
    public bool isMultiplayer;
    private NetworkVariable<SceneLoadData> networkSceneLoadData = new NetworkVariable<SceneLoadData>();
    private NetworkVariable<StartingResourcesList> networkStartingResources = new NetworkVariable<StartingResourcesList>();
    public bool isInit = false;

    void Start()
    {
        Instance = this;

        if (!isInit)
	    {
		    Init();
	    }
	}

    [GenerateSerializationForType(typeof(SceneLoadData))]
    [GenerateSerializationForType(typeof(StartingResources))]
    public void Init()
	{
		isInit = true;
        playerTreeManager = UI.gameObject.transform.Find("EvolutionTreeInterface").GetComponent<PlayerTreeManager>();
        fortButtonManager = UI.gameObject.transform.Find("CreateFortButton").GetComponent<FortButtonManager>();
        dialogController = UI.gameObject.transform.Find("DialogController").GetComponent<DialogController>();
        unitStatsMenuController = UI.gameObject.GetComponent<UnitStatsMenuController>();
        nextTurnMenuController = UI.gameObject.GetComponent<NextTurnMenuController>();
        InitStaticVariables();
        soundManager = Instantiate(soundManager, new Vector3(0,0,0), Quaternion.identity);
        gameSettings = GameObject.Find("GameSettings")?.GetComponent<GameSettings>();
        string saveRoot = SaveRoot.saveRoot;

        sceneLoadData = networkSceneLoadData?.Value;

        if (sceneLoadData == null)
        {
	        sceneLoadData = new SceneLoadData();
        }
        else
        {
            isMultiplayer = sceneLoadData.isMultiplayer;
            startingResources = GetDeepCopyFromNetwork();
        }

		saveManager.Init(this);
        loadManager.Init(this);

		//there should also be error handling for when saveRoot is wrong
		if (sceneLoadData.playerColors == null)
		{
			if (saveRoot == null)
			{
				sceneLoadData = LoadDataFromSettingsCreator();
            }
			else
			{
				loadManager.SetSaveRoot(saveRoot);
				sceneLoadData = loadManager.Load();
                startingResources = loadManager.LoadStartingResources(sceneLoadData.numberOfPlayers);
                startingUnits = loadManager.LoadStartingUnits(sceneLoadData.numberOfPlayers);
            }

            isMultiplayer = sceneLoadData.isMultiplayer;
            networkSceneLoadData.Value = sceneLoadData;
            SetNetworkStartingResources();
        }

        LoadGameData(sceneLoadData);

        playerTreeManager.Init(this);
        mapManager.Init(this);
        cityMenuManager.Init(this);
        fortButtonManager.Init(this);

        players = new PlayerManager[sceneLoadData.numberOfPlayers];

        if (!isMultiplayer || IsServer)
        {
	        InstantiatePlayers(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, startingResources, sceneLoadData.playerColors, sceneLoadData.startingCityNames, sceneLoadData.isComputer, sceneLoadData.isMultiplayer);
		}

        if (!IsServer)
        {
	        InitPlayersThatSpawnedBeforeThis();
        }

        if(players[activePlayerIndex] != null && players[activePlayerIndex].IsOwner)
        {
            players[activePlayerIndex].StartFirstTurn();
        }
	}

    private void InitPlayersThatSpawnedBeforeThis()
	{
		// Players that have spawned before GameManager need initialization

		for (int i = 0; i < players.Length; i++)
		{
			var playerManager = players[i];
			if (!playerManager)
			{
				playerManager =
					FindObjectsByType<PlayerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
						.FirstOrDefault(player => player.index == i);
				players[i] = playerManager;
				activePlayer = players[0];
			}

			if (playerManager && !playerManager.isInit)
			{
				playerManager.Init(this);
			}
		}
	}

    private void InitStaticVariables()
    {
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            unitSprites[i] = unitPrefabs[i].transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
        }
    }

    public SceneLoadData LoadDataFromSettingsCreator() {
        int InNumberOfPlayers = gameSettings?.numberOfPlayers ?? 2;
        Vector3[] InPlayerPositions = gameSettings?.playerPositions ?? new [] { new Vector3(-6.93f, 0.00f, 0.00f), new Vector3(12.12f, 0.00f, 0.00f)};
        Color32[] InPlayerColors = gameSettings?.playerColors ?? new[] { new Color32(255, 0, 0, 255), new Color32(0, 0, 255, 255) };
        string[] InStartingCityNames = gameSettings?.citiesNames ?? new [] { "Babylon", "Alexandria", "Carthage", "Persepolis" };
        bool[] isComputer = gameSettings?.isComputer ?? new [] { false, false };
        isMultiplayer = gameSettings?.isMultiplayer ?? true;
        string difficulty = gameSettings?.difficulty ?? "Medium";
        startingResources = new StartingResources[InNumberOfPlayers];
        startingUnits = new StartingUnits[InNumberOfPlayers];
        for (int i = 0; i < InNumberOfPlayers; i++)
        {
            (startingResources[i], startingUnits[i]) = getStartingResourcesByDifficulty(difficulty);
        }

        return new SceneLoadData(InNumberOfPlayers, InPlayerPositions, InPlayerColors, InStartingCityNames, 1,
            0, isComputer, isMultiplayer, difficulty);
    }

    public (StartingResources, StartingUnits) getStartingResourcesByDifficulty(string difficulty) {
        if(difficulty == "Easy") {
            return getStartingResourcesEasy();
        } else if(difficulty == "Medium") {
            return getStartingResourcesMedium();
        } else if(difficulty == "Hard") {
            return getStartingResourcesHard();
        } else {
            return getStartingResourcesEasy();
        }

    }

    public (StartingResources, StartingUnits) getStartingResourcesEasy() {
        StartingResources InStartingResources = StartingResources.NewTransferable();
        StartingUnits inStartingUnits = new StartingUnits();
        inStartingUnits.units = new List<UnitController>();
        inStartingUnits.units.Add(unitPrefabs.ElementAt(5).GetComponent<UnitController>());
        inStartingUnits.units.Add(unitPrefabs.ElementAt(6).GetComponent<UnitController>());
        inStartingUnits.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 300;

        return (InStartingResources, inStartingUnits);
    }

    public (StartingResources, StartingUnits) getStartingResourcesMedium() {
        StartingResources InStartingResources = StartingResources.NewTransferable();
        StartingUnits inStartingUnits = new StartingUnits();
        inStartingUnits.units = new List<UnitController>();
        inStartingUnits.units.Add(unitPrefabs.ElementAt(5).GetComponent<UnitController>());
        inStartingUnits.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 200;

        return (InStartingResources, inStartingUnits);
    }

    public (StartingResources, StartingUnits) getStartingResourcesHard() {
        StartingResources InStartingResources = StartingResources.NewTransferable();
        StartingUnits inStartingUnits = new StartingUnits();
        inStartingUnits.units = new List<UnitController>();
        inStartingUnits.units.Add(unitPrefabs.ElementAt(6).GetComponent<UnitController>());
        inStartingUnits.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 100;

        return (InStartingResources, inStartingUnits);
    }
    
    public void LoadGameData(SceneLoadData sceneLoadData) {
        if(!IsInitialDataCorrect(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, new StartingResources[sceneLoadData.numberOfPlayers], sceneLoadData.playerColors)) {
            Debug.Log("Wrong initial data. Stopping game now!");
            return;
        }
        this.playerPositions = sceneLoadData.playerPositions;
        this.numberOfPlayers = sceneLoadData.numberOfPlayers;
        this.turnNumber = sceneLoadData.turnNumber;
        this.activePlayerIndex = sceneLoadData.activePlayerIndex;
        DisplayTurnNumber(turnNumber);
    }

    private void InstantiatePlayers(int numberOfPlayers, Vector3[] playerPositions, StartingResources[] startingResources, Color32[] playerColors, string[] startingCityNames, bool[] isComputer, bool isMultiplayer)
    {
        this.numberOfPlayers = numberOfPlayers;
        players = new PlayerManager[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            players[i] = Instantiate(playerPrefab, playerPositions[i], Quaternion.identity).GetComponent<PlayerManager>();
            if (!isMultiplayer)
            {
	            players[i].Init(this, mapManager, startingResources[i], startingUnits[i], playerColors[i], startingCityNames[i], isComputer[i], i);
			}
            players[i].index = i;
        }

        if (isMultiplayer)
        {
	        foreach (var playerManager in players)
	        {
                // clientId is likely always equal to index, but better play it safe
                var clientId = GetClientId(playerManager.index);
                if (clientId != null)
                {
	                playerManager.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)clientId);
				}
                else
                {
					// this is a player that never joined, convert them to computer player
                    // and spawn as a server owned object
					playerManager.GetComponent<NetworkObject>().Spawn();
				}
			}
        }
    }

    public void NextPlayer()
    {

	    if (!activePlayer.isSpectator && !activePlayer.isInMenu)
	    {
		    if (isMultiplayer)
		    {
			    NextTurnRpc();
			}
		    else
		    {
			    NextTurn();
			}
	    }
    }

    [Rpc(SendTo.Everyone)]
    public void NextTurnRpc()
    {
	    NextTurn();
    }

    public void NextTurn()
    {
	    this.activePlayer.playerSupplyManager.ClearSupplyLineCreator();
	    this.cityMenuManager.Deactivate();
	    // this needs to happen before the next player is activated, because next player may be dead
	    CheckIfGameIsEnded();
	    players[activePlayerIndex].playerUnitsManager.TryAutoMoveAll();
	    players[activePlayerIndex].playerUnitsManager.DeactivateAll();
	    players[activePlayerIndex].gameObject.SetActive(false);
	    GameObject unitList = UI.transform.Find("UnitList").gameObject;
	    unitList.SetActive(false);

	    activePlayerIndex = (activePlayerIndex + 1) % numberOfPlayers;

	    activePlayer = players[activePlayerIndex];
	    players[activePlayerIndex].gameObject.SetActive(true);
	    if (activePlayerIndex == 0)
	    {
		    turnNumber++;
		    DisplayTurnNumber(turnNumber);
	    }
	    SetPlayerUIColor(players[activePlayerIndex].color);
		players[activePlayerIndex].StartTurn();
	    playerTreeManager.reserachProgress();
        fortButtonManager.CheckIfFortResearched(activePlayer);
	}

    public void CheckIfGameIsEnded() {
        bool gameEnded = false;
        int indexOfWinner = -1;
        bool[] playersAlive = new bool[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            playersAlive[i] = players[i].isAlive();
        }
        if(playersAlive.Count(x => x) == 1) {
            gameEnded = true;
            indexOfWinner = Array.IndexOf(playersAlive, true);
        }
        if(gameEnded) {
            EndGame(indexOfWinner);
        }
        else if(playersAlive.Count(x=>x) < numberOfPlayers) {

            //count how many players died this turn
            int howManyDied = numberOfPlayers - playersAlive.Count(x=>x);
            int[] playersDied = new int[howManyDied];


            //someone died this turn, kill them, start from the end so that deleting players from list doesnt affect order of players
            for(int i=numberOfPlayers-1, j=0; i>=0; i--) {
                if(!playersAlive[i]) {
                    playersDied[j++] = players[i].index + 1;
                    KillPlayer(i);
                }
            }

            //display who died
            string diedString = "Player";
            if(howManyDied > 1) {
                diedString += "s";
            }
            diedString += " ";
            for(int i=0; i<howManyDied; i++) {
                diedString += playersDied[i];
                if(i < howManyDied - 1) {
                    diedString += ", ";
                }
            }
            diedString += " lost game in this turn!";
            DisplayWhoLost(diedString);
        }
    }

    public void EndGame(int indexOfWinner) {
        // TODO: ADD NAMES OF PLAYERS TO BE DISPLAYED
        PauseMenu.isPaused = true;
        int idxOfWinner = players[indexOfWinner].index + 1;
        string whoWon = "Game ended!\n Player " + idxOfWinner + " won!";
        GameObject endGameScreen = UI.transform.Find("GameEnd").gameObject;
        endGameScreen.SetActive(true);
        endGameScreen.transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>().text = whoWon;
        endGameScreen.transform.Find("ExitGameButton").Find("Button").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "EXIT TO MENU";
        endGameScreen.transform.Find("ExitGameButton").Find("Button").GetComponent<Button>().onClick.AddListener(() => { 
            SceneManager.LoadScene("MainMenu");
         });
    }

    public void DisplayWhoLost(string message) {
        PauseMenu.isPaused = true;
        GameObject endGameScreen = UI.transform.Find("GameEnd").gameObject;
        endGameScreen.SetActive(true);
        endGameScreen.transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>().text = message;
        endGameScreen.transform.Find("ExitGameButton").Find("Button").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "OK";
        endGameScreen.transform.Find("ExitGameButton").Find("Button").GetComponent<Button>().onClick.AddListener(() => { 
            GameObject endGameScreen = UI.transform.Find("GameEnd").gameObject;
            endGameScreen.SetActive(false);
            PauseMenu.isPaused = false;
        });
    }

    public void KillPlayer(int playerIndex) {
        // REMOVE FROM LISTS
        numberOfPlayers -= 1;
        PlayerManager[] newPlayers = new PlayerManager[numberOfPlayers];
        for (int i = 0, j = 0; i < numberOfPlayers + 1; i++) {
            if (i != playerIndex) {
                newPlayers[j++] = players[i];
            }
        }
        players = newPlayers;

        Vector3[] newPlayersPositions = new Vector3[numberOfPlayers];
        for (int i = 0, j = 0; i < numberOfPlayers + 1; i++) {
            if (i != playerIndex) {
                newPlayersPositions[j++] = playerPositions[i];
            }
        }
        playerPositions = newPlayersPositions;        
    }

    public void SetPlayerUIColor(Color color) {
	    nextTurnMenuController.SetColor(color);
    }

    public void SetNextTurnButtonText()
    {
	    if (activePlayer == null || activePlayer.isSpectator)
	    {
		    nextTurnMenuController.SetText("WAIT FOR OTHER PLAYER");
	    }
	    else
	    {
		    nextTurnMenuController.SetText("NEXT TURN");
	    }
	}

    private void DisplayTurnNumber(int turnNumber)
    {
        turnText.GetComponent<TMPro.TextMeshProUGUI>().text = turnNumber.ToString();
    }

    private bool IsInitialDataCorrect(int numberOfPlayers, Vector3[] playerPositions, StartingResources[] startingResources, Color32[] playerColors) 
    {
        if(startingResources.Length != numberOfPlayers) {
            Debug.Log("Wrong number of player starting resources!");
            return false;
        }
        if(playerPositions.Length != numberOfPlayers) {
            Debug.Log("Wrong number of player starting positions!");
            return false;
        }
        if(playerColors.Length != numberOfPlayers) {
            Debug.Log("Wrong number of playercolors!");
            return false;
        }
        //should also check if positions in bounds or sth
        return true;
    }

    public GameObject getUnitPrefabByName(String unitType) {
        if(Enum.IsDefined(typeof(UnitTypes), unitType)) {
            return unitPrefabs[(int)Enum.Parse(typeof(UnitTypes), unitType)];
        } else {
            Debug.Log("Invalid unit type");
            return null;
        }
    }

    public GameObject getUnitPrefab(UnitTypes unitType) {
        return unitPrefabs[(int)unitType];
    }
    public Sprite getUnitSprite(UnitTypes unitType)
    {
        return unitSprites[(int)unitType];
    }

    public ulong? GetClientId(int index)
    {
        return NetworkManager.ConnectedClientsList
	        .FirstOrDefault(client => client.PlayerObject.GetComponent<PlayerData>().index == index)?
	        .ClientId;
	}

    // The cursed section
    private void SetNetworkStartingResources()
    {
        int index = 0;
        networkStartingResources.Value = new StartingResourcesList();
        networkStartingResources.Value.list = startingResources.ToList();

        var tempStartingResources = new StartingResources[sceneLoadData.numberOfPlayers];
        for (int i = 0; i < sceneLoadData.numberOfPlayers; i++)
        {
            tempStartingResources[i] = startingResources[i].DeepCopy();
        }

        startingResources = tempStartingResources;
    }

    private StartingResources[] GetDeepCopyFromNetwork()
    {
        var newStartingResources = new StartingResources[sceneLoadData.numberOfPlayers];

        int index = 0;
        foreach (var resources in networkStartingResources.Value.list)
        {
            newStartingResources[index] = resources.DeepCopy();
            index++;
        }

        return newStartingResources;
    }
}