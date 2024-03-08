using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using RedBjorn.ProtoTiles;

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
    public CityMenuManager cityMenuManager;
    public GameObject playerPrefab;

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

    // UI elements
    public GameObject unitTypeText;
    public GameObject unitAttackText;
    public GameObject unitLevelText;
    public GameObject unitHealthText;
    public GameObject unitDefenseText;
    public GameObject unitBox;
    public Image nextTurnButtonImage;
    public GameObject UI;
    public TileTag cityTag;

    void Start()
    {
        //temp before new game tab (one for choosing difficulty, player colors etc)
        //these should only be used for instantiation and will be imported from game launcher later on
        int InNumberOfPlayers = 2;
        Vector3[] InPlayerPositions = new Vector3[2];
        InPlayerPositions[0] = new Vector3(-6.928203f, 0f, 0f);
        InPlayerPositions[1] = new Vector3(0f, 0f, 0f);
        StartingResources[] InStartingResources = new StartingResources[2] {
            new StartingResources(),
            new StartingResources()
        };
        
        InStartingResources[0].units = new List<UnitController>();
        InStartingResources[1].units = new List<UnitController>();

        InStartingResources[0].units.Add((Resources.Load("Units/Archer") as GameObject).GetComponent<UnitController>());
        InStartingResources[0].units.Add((Resources.Load("Units/Chariot") as GameObject).GetComponent<UnitController>());

        InStartingResources[1].units.Add((Resources.Load("Units/Skirmisher") as GameObject).GetComponent<UnitController>());

        Debug.Log("Starting resources units:");
        InStartingResources[0].units.ForEach(element => Debug.Log(element.defense));
        Debug.Log("End of starting resources units");

        InStartingResources[0].unitLoadData = new List<UnitLoadData>();
        InStartingResources[1].unitLoadData = new List<UnitLoadData>();

        InStartingResources[0].gold = 100;
        InStartingResources[1].gold = 100;


        Color32[] InPlayerColors = new Color32[2];
        InPlayerColors[0] = Color.red;
        InPlayerColors[1] = Color.blue;

        string[] InStartingCityNames = new string[2];
        InStartingCityNames[0] = "Ur";
        InStartingCityNames[1] = "Babylon";
        //end of game launcher variables

        string saveRoot = SaveRoot.saveRoot;
        SceneLoadData sceneLoadData = new SceneLoadData();

        saveManager.Init(this);
        loadManager.Init(this);
        //this should later be called directly from game creator and not the Start function
        //there should also be error handling for when saveRoot is wrong
        if (saveRoot == null) {
            sceneLoadData = new SceneLoadData(InNumberOfPlayers, InPlayerPositions, InStartingResources, InPlayerColors, InStartingCityNames, 1, 0);
        } else {
            Debug.Log("Loading game manager from save " + saveRoot);
            loadManager.SetSaveRoot(saveRoot);
            sceneLoadData = loadManager.Load();
            Debug.Log(sceneLoadData.ToString());
        }

        LoadGameData(sceneLoadData);

        mapManager.Init(this);
        cityMenuManager.Init(this);

        InstantiatePlayers(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, sceneLoadData.startingResources, sceneLoadData.playerColors, sceneLoadData.startingCityNames);
        players[activePlayerIndex].StartTurn();
        this.HideUnitBox();
    }

    public void LoadGameData(SceneLoadData sceneLoadData) {
        if(!IsInitialDataCorrect(sceneLoadData.numberOfPlayers, sceneLoadData.playerPositions, sceneLoadData.startingResources, sceneLoadData.playerColors)) {
            Debug.Log("Wrong initial data. Stopping game now!");
            return;
        }
        Debug.Log("LoadGameData");
        this.playerPositions = sceneLoadData.playerPositions;
        this.numberOfPlayers = sceneLoadData.numberOfPlayers;
    }

    private void InstantiatePlayers(int numberOfPlayers, Vector3[] playerPositions, StartingResources[] startingResources, Color32[] playerColors, string[] startingCityNames)
    {
        Debug.Log("Instantiating players");
        this.numberOfPlayers = numberOfPlayers;
        players = new PlayerManager[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            players[i] = Instantiate(playerPrefab, playerPositions[i], Quaternion.identity).GetComponent<PlayerManager>();
            players[i].Init(this, mapManager, startingResources[i], playerColors[i], startingCityNames[i], i);
            if(i == activePlayerIndex) {
                players[i].gameObject.SetActive(true);
            }
            else {
                players[i].gameObject.SetActive(false);
            }
        }
        activePlayer = players[activePlayerIndex];
        SetPlayerUIColor(players[activePlayerIndex].color);
    }

    public void NextPlayer()
    {
        players[activePlayerIndex].DeactivateUnitsRange();
        players[activePlayerIndex].gameObject.SetActive(false);
        GameObject unitList = UI.transform.Find("UnitList").gameObject;
        unitList.SetActive(false);
        if (activePlayerIndex + 1 == numberOfPlayers) {
            activePlayerIndex = 0;
        }
        else {
            activePlayerIndex++;
        }
        activePlayer = players[activePlayerIndex];
        players[activePlayerIndex].gameObject.SetActive(true);
        if(activePlayerIndex == 0) {
            turnNumber++;
            turnText.GetComponent<TMPro.TextMeshProUGUI>().text = turnNumber.ToString();
        }
        SetPlayerUIColor(players[activePlayerIndex].color);
        players[activePlayerIndex].StartTurn();
        Debug.Log("Player " + activePlayerIndex + " turn");
    }

    private void SetPlayerUIColor(Color color) {
        nextTurnButtonImage.color = color;
    }

    private bool IsInitialDataCorrect(int numberOfPlayers, Vector3[] playerPositions, StartingResources[] startingResources, Color32[] playerColors) 
    {
        Debug.Log(numberOfPlayers);
        Debug.Log(playerPositions);
        Debug.Log(startingResources);
        Debug.Log(playerColors);
        
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


    public void setUnitTypeText(string unitType) {
        unitTypeText.GetComponent<TMPro.TextMeshProUGUI>().text = unitType;
    }
    public void setUnitAttackText(string unitAttack)
    {
        unitAttackText.GetComponent<TMPro.TextMeshProUGUI>().text = unitAttack;
    }

    public void setUnitLevelText(string unitLevel)
    {
        unitLevelText.GetComponent<TMPro.TextMeshProUGUI>().text = unitLevel;
    }

    public void setUnitHealthText(string unitHealth)
    {
        unitHealthText.GetComponent<TMPro.TextMeshProUGUI>().text = unitHealth;
    }

    public void setUnitDefenseText(string unitDefense)
    {
        unitDefenseText.GetComponent<TMPro.TextMeshProUGUI>().text = unitDefense;
    }

    public void HideUnitBox() {
        unitBox.SetActive(false);
    }

    public void ShowUnitBox() {
        unitBox.SetActive(true);
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

}
