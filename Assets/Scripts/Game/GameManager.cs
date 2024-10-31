using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using RedBjorn.ProtoTiles;
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

public class GameManager : MonoBehaviour
{
    public MapManager mapManager;
    public SaveManager saveManager;
    public LoadManager loadManager;
    public GameObject soundManager;
    public CityMenuManager cityMenuManager;
    public PauseMenu pauseMenu;
    public GameObject playerPrefab;
    public GameSettings gameSettings;
    public PlayerTreeManager playerTreeManager;

    // Turn elements
    public int turnNumber = 1;
    public GameObject turnText;
    
    // Player elements
    public int activePlayerIndex = 0;
    public PlayerManager activePlayer;
    public int numberOfPlayers;
    public PlayerManager[] players;
    public Vector3[] playerPositions;
    public List<UnitController> units = new List<UnitController>();
    // Unit types
    private const int amountOfUnitTypes = 7;
    public GameObject[] unitPrefabs = new GameObject[amountOfUnitTypes];
    public Sprite[] unitSprites = new Sprite[amountOfUnitTypes];

    // UI elements
    public UnitStatsMenuController unitStatsMenuController;
    public Image nextTurnButtonImage;
    public GameObject UI;
    public TileTag cityTag;

    // Multiplayer
    public bool isInitialized = false;
    public bool isInitializing = false;
    private SceneLoadData sceneLoadData;

    public void Start()
    {
        if (!isInitialized)
        {
            Init();
        }
    }

    public void Init()
    {
        isInitializing = true;

        playerTreeManager = UI.gameObject.transform.Find("EvolutionTreeInterface").GetComponent<PlayerTreeManager>();
        unitStatsMenuController = UI.gameObject.GetComponent<UnitStatsMenuController>();
        InitStaticVariables();
        soundManager = Instantiate(soundManager, new Vector3(0,0,0), Quaternion.identity);
        gameSettings = GameObject.Find("GameSettings")?.GetComponent<GameSettings>();
        string saveRoot = SaveRoot.saveRoot;
        sceneLoadData = new SceneLoadData();

        saveManager.Init(this);
        loadManager.Init(this);
        
        //there should also be error handling for when saveRoot is wrong
        if (saveRoot == null) {
            sceneLoadData = LoadDataFromSettingsCreator();
        } else {
            loadManager.SetSaveRoot(saveRoot);
            sceneLoadData = loadManager.Load();
        }

        LoadGameData(sceneLoadData);

        playerTreeManager.Init(this);
        mapManager.Init(this);
        cityMenuManager.Init(this);

        players = new PlayerManager[sceneLoadData.numberOfPlayers];

        InstantiatePlayers(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, sceneLoadData.startingResources, sceneLoadData.playerColors, sceneLoadData.startingCityNames, sceneLoadData.isComputer, sceneLoadData.isMultiplayer);
        players[activePlayerIndex].StartFirstTurn();

        isInitializing = false;
        isInitialized = true;
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
        StartingResources[] InStartingResources = new StartingResources[InNumberOfPlayers];

        for(int i = 0; i < InNumberOfPlayers; i++) {
            InStartingResources[i] = getStartingResourcesByDifficulty(gameSettings?.difficulty ?? "Medium");
        }

        return new SceneLoadData(InNumberOfPlayers, InPlayerPositions, InStartingResources, InPlayerColors, InStartingCityNames, 1, 0, isComputer, false);
    }

    public StartingResources getStartingResourcesByDifficulty(string difficulty) {
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

    public StartingResources getStartingResourcesEasy() {
        StartingResources InStartingResources = new StartingResources();
        InStartingResources.units = new List<UnitController>();
        InStartingResources.units.Add(unitPrefabs.ElementAt(5).GetComponent<UnitController>());
        InStartingResources.units.Add(unitPrefabs.ElementAt(6).GetComponent<UnitController>());
        InStartingResources.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 300;

        return InStartingResources;
    }

    public StartingResources getStartingResourcesMedium() {
        StartingResources InStartingResources = new StartingResources();
        InStartingResources.units = new List<UnitController>();
        InStartingResources.units.Add(unitPrefabs.ElementAt(5).GetComponent<UnitController>());
        InStartingResources.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 200;
        
        return InStartingResources;
    }

    public StartingResources getStartingResourcesHard() {
        StartingResources InStartingResources = new StartingResources();
        InStartingResources.units = new List<UnitController>();
        InStartingResources.units.Add(unitPrefabs.ElementAt(6).GetComponent<UnitController>());
        InStartingResources.unitLoadData = new List<UnitLoadData>();
        InStartingResources.gold = 100;
        
        return InStartingResources;
    }
    
    public void LoadGameData(SceneLoadData sceneLoadData) {
        if(!IsInitialDataCorrect(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, sceneLoadData.startingResources, sceneLoadData.playerColors)) {
            Debug.Log("Wrong initial data. Stopping game now!");
            return;
        }
        this.playerPositions = sceneLoadData.playerPositions;
        this.numberOfPlayers = sceneLoadData.numberOfPlayers;
        this.turnNumber = sceneLoadData.turnNumber;
        this.activePlayerIndex = sceneLoadData.activePlayerIndex;
        DisplayTurnNumber(turnNumber);
    }

    public void Register(PlayerManager player, int i)
    {
        if (players[i] == null)
        {
            players[i] = player;
            players[i].Init(this, mapManager, sceneLoadData.startingResources[i], sceneLoadData.playerColors[i], sceneLoadData.startingCityNames[i], sceneLoadData.isComputer[i], i);
        }
    }

    private void InstantiatePlayers(int numberOfPlayers, Vector3[] playerPositions, StartingResources[] startingResources, Color32[] playerColors, string[] startingCityNames, bool[] isComputer, bool isMultiplayer)
    {
        this.numberOfPlayers = numberOfPlayers;
        players = new PlayerManager[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            if (!isComputer[i] && isMultiplayer)
            {
                //registers self
                continue;
            }
            players[i] = Instantiate(playerPrefab, playerPositions[i], Quaternion.identity).GetComponent<PlayerManager>();
            players[i].Init(this, mapManager, startingResources[i], playerColors[i], startingCityNames[i], isComputer[i], i);
            
        }
    }

    public void NextPlayer()
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
        if(activePlayerIndex == 0) {
            turnNumber++;
            DisplayTurnNumber(turnNumber);
        }
        SetPlayerUIColor(players[activePlayerIndex].color);
        players[activePlayerIndex].StartTurn();
        playerTreeManager.reserachProgress();
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
        nextTurnButtonImage.color = color;
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

    StartingResources[] CreateExampleGameStart() {
        StartingResources[] InStartingResources = new StartingResources[2] {
            new StartingResources(),
            new StartingResources()
        };
        
        InStartingResources[0].units = new List<UnitController>();
        InStartingResources[1].units = new List<UnitController>();
        
        InStartingResources[0].units.Add(unitPrefabs.ElementAt(0).GetComponent<UnitController>());
        InStartingResources[0].units.Add(unitPrefabs.ElementAt(3).GetComponent<UnitController>());

        InStartingResources[1].units.Add(unitPrefabs.ElementAt(4).GetComponent<UnitController>());

        InStartingResources[0].unitLoadData = new List<UnitLoadData>();
        InStartingResources[1].unitLoadData = new List<UnitLoadData>();

        InStartingResources[0].gold = 100;
        InStartingResources[1].gold = 100;

        return InStartingResources;
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
}