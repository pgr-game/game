using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    //these should only be used for instantiation and will be imported from game launcher later on
    public int InNumberOfPlayers = 3;
    public Vector3[] InPlayerPositions;
    public StartingResources[] InStartingResources;
    public Color32[] InPlayerColors;
    public string[] InStartingCityNames;
    //end of game launcher variables

    public MapManager mapManager;
    public SaveManager saveManager;
    public LoadManager loadManager;
    public CityMenuManager cityMenuManager;
    public GameObject playerPrefab;

    public int turnNumber = 1;
    public GameObject turnText;
    
    public int activePlayerIndex = 0;
    public PlayerManager activePlayer;
    public int numberOfPlayers;
    public PlayerManager[] players;
    public Vector3[] playerPositions;


    // is it really usefulll for anything ?
    public List<UnitController> units = new List<UnitController>();
    // Unit types
    private const int amountOfUnitTypes = 7;
    public GameObject[] unitPrefabs = new GameObject[amountOfUnitTypes];
    public GameObject unitTypeText;
    public GameObject unitAttackText;
    public GameObject unitLevelText;
    public GameObject unitHealthText;
    public GameObject unitDefenseText;
    public GameObject unitBox;
    public Image nextTurnButtonImage;
    public GameObject UI;

    void Start()
    {
        string saveRoot = PlayerPrefs.GetString("saveRoot");
        SceneLoadData sceneLoadData = new SceneLoadData();

        saveManager.Init(this);
        //this should later be called directly from game creator and not the Start function
        //there should also be error handling for when saveRoot is wrong
        if (saveRoot == null) {
            sceneLoadData = new SceneLoadData(InNumberOfPlayers, InPlayerPositions, InStartingResources, InPlayerColors, InStartingCityNames, 1, 0);
        } else {
            Debug.Log("Loading game manager from save");
            //saveManager.SetSaveRoot(SceneLoadData.saveRoot);
            //sceneLoadData = saveManager.Load();
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
            players[i].mapManager = mapManager;
            players[i].startingResources = InStartingResources[i];
            players[i].color = playerColors[i];
            if(i == activePlayerIndex) {
                players[i].gameObject.SetActive(true);
            }
            else {
                players[i].gameObject.SetActive(false);
            }
            players[i].Init(this, startingCityNames[i], i);
            units.AddRange(players[i].startingResources.units);
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
