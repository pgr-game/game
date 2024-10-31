using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System;


public class GameCreatorController : MonoBehaviour
{
    public bool IsMultiplayer;
    public GameSettings gameSettings;
    public TMP_Dropdown mapDropdown;
    public Slider numberOfPlayersSlider;
    public TMP_Text numberOfPlayersText;
    public TMP_Dropdown difficultyDropdown;
    public TMP_Dropdown[] colorDropdowns = new TMP_Dropdown[4];
    public Toggle[] IsComputerToggles = new Toggle[4];
    public TMP_Text colorDuplicateText;
    void Start()
    {
        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        numberOfPlayersSlider.onValueChanged.AddListener(delegate {
            NumberOfPlayersSliderValueChanged(numberOfPlayersSlider);
        });
    }

    void NumberOfPlayersSliderValueChanged(Slider change)
    {
        numberOfPlayersText.text = change.value.ToString();
        setColorSelectorsVisibility((int)change.value);
    }

    public void setColorSelectorsVisibility(int numberOfPlayers)
    {
        for(int i=0; i<numberOfPlayers; i++) {
            colorDropdowns[i].gameObject.SetActive(true);
        }
        for(int i=numberOfPlayers; i<4; i++) {
            colorDropdowns[i].gameObject.SetActive(false);
        }
    } 

    Color GetColorOfPlayerFromDropdown(TMP_Dropdown change)
    {
        var colorName = change.options[change.value].text;
        Color32 color;

        switch (colorName.ToLower())
        {
            case "red":
                color = Color.red;
                break;
            case "blue":
                color = Color.blue;
                break;
            case "green":
                color = Color.green;
                break;
            case "magenta":
                color = Color.magenta;
                break;
            case "cyan":
                color = Color.cyan;
                break;
            case "yellow":
                color = Color.yellow;
                break;
            default:
                Debug.LogWarning($"Nieznany kolor: {colorName}");
                color = Color.white;
                break;
        }
        return color;
    }

    public string[] getCitiesNamesByMap(string mapName) {
        switch (mapName) {
            case "RiverDelta":
                return new string[] { "Babylon", "Alexandria", "Carthage", "Persepolis" };
            case "BigSea":
                return new string[] { "Teotihuacan", "Knossos", "Olympia ", "Petra" };
            case "Lakes":
                return new string[] { "Nineveh", "Tikal", "Memphis", "Pataliputra" };
            case "Mountains":
                return new string[] { "Ur", "Hattusa", "Chan Chan", "Mycenae" };
            default:
                return new string[] { "City1", "City2", "City3", "City4" };
        }
    }

    public Vector3[] getPlayerPositionsByMap(string mapName) {        
        switch (mapName) {
            case "RiverDelta":
                return new Vector3[] { new Vector3(-6.928203f, 0, 0), new Vector3(12.12436f, 0, 0), new Vector3(-8.660254f, -18f, 0), new Vector3(12.12436f, -9f, 0) };
            case "BigSea":
                return new Vector3[] { new Vector3(-6.928203f, 0, 0), new Vector3(12.12436f, 0, 0), new Vector3(12.99038f, -22.5f, 0), new Vector3(-14.72243f, -25.5f, 0) };
            case "Lakes":
                return new Vector3[] { new Vector3(-6.928203f, 0, 0), new Vector3(12.12436f, 0, 0), new Vector3(13.85641f, -21f, 0), new Vector3(-19.91858f, -16.5f, 0) };
            case "Mountains":
                return new Vector3[] { new Vector3(-6.928203f, 0, 0), new Vector3(12.99038f, 1.5f, 0), new Vector3(-21.65063f, -4.5f, 0), new Vector3(-11.25833f, -13.5f, 0) };
            default:
                return new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        }
    }

    public void SetupGameSettings() {
        // Map name
        gameSettings.GetComponent<GameSettings>().SetMapName(mapDropdown.options[mapDropdown.value].text);

        // Difficulty level
        gameSettings.GetComponent<GameSettings>().difficulty = difficultyDropdown.options[difficultyDropdown.value].text;

        // Number of players
        int numberOfPlayers = (int)numberOfPlayersSlider.value;
        gameSettings.GetComponent<GameSettings>().numberOfPlayers = numberOfPlayers;

        // Player colors
        gameSettings.playerColors = new Color32[numberOfPlayers];
        for(int i=0; i<numberOfPlayers; i++) {
            gameSettings.playerColors[i] = GetColorOfPlayerFromDropdown(colorDropdowns[i]);
        }

        // Player positions
        Vector3[] playerPositions = getPlayerPositionsByMap(gameSettings.mapName).Take(numberOfPlayers).ToArray();
        gameSettings.playerPositions = playerPositions;

        // Cities names
        gameSettings.citiesNames = getCitiesNamesByMap(gameSettings.mapName);

        // IsComputer
        gameSettings.isComputer = new bool[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        {
            gameSettings.isComputer[i] = GetIsComputer(i);
        }

        // validation and starting game if valid
        ValidateGameSettings();
    }

    public void ValidateGameSettings() {
        Color32[] colors = gameSettings.playerColors;
        int[] duplicates = CheckForDuplicateColors(colors);
        if(duplicates.Length == 0) {
            if (IsMultiplayer)
            {
                FindObjectOfType<MultiplayerMainMenuController>().StartHost();
            }
            else
            {
                this.gameObject.GetComponent<MainMenu>().StartGame();
            }
        }
        else {
            string message = "Colors for players: ";
            foreach(int i in duplicates) {
                message += (i+1) + ", ";
            }
            message = message.Substring(0, message.Length - 2);
            message += " are duplicated! Please change before proceeding.";
            colorDuplicateText.text = message;
            colorDuplicateText.gameObject.SetActive(true);
        }
    }

    int[] CheckForDuplicateColors(Color32[] colors)
    {
        Dictionary<Color32, List<int>> colorIndices = new Dictionary<Color32, List<int>>();
        List<int> duplicates = new List<int>();

        for (int i = 0; i < colors.Length; i++)
        {
            Color32 color = colors[i];
            if (colorIndices.ContainsKey(color))
            {
                if (colorIndices[color].Count == 1)
                {
                    duplicates.Add(colorIndices[color][0]);
                }
                duplicates.Add(i); 
                colorIndices[color].Add(i);
            }
            else
            {
                colorIndices[color] = new List<int> { i };
            }
        }

        return duplicates.ToArray();
    }

    private bool GetIsComputer(int index)
    {
        return IsComputerToggles[index].isOn;
    }
}
