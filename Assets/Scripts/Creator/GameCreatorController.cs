using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class GameCreatorController : MonoBehaviour
{
    public GameSettings gameSettings;
    public TMP_Dropdown mapDropdown;
    public Slider numberOfPlayersSlider;
    public TMP_Text numberOfPlayersText;
    public TMP_Dropdown difficultyDropdown;
    public TMP_Dropdown[] colorDropdowns = new TMP_Dropdown[4];
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
        Color color;

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
                return new string[] { "City1", "City2", "City3", "City4" };
            default:
                return new string[] { "City1", "City2", "City3", "City4" };
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
        gameSettings.playerColors = new Color[numberOfPlayers];
        for(int i=0; i<numberOfPlayers; i++) {
            gameSettings.playerColors[i] = GetColorOfPlayerFromDropdown(colorDropdowns[i]);
        }

        // todo player positions

        // Cities names
        gameSettings.citiesNames = getCitiesNamesByMap(gameSettings.mapName);

        // validation and starting game if valid
        ValidateGameSettings();
    }

    public void ValidateGameSettings() {
        Color[] colors = gameSettings.playerColors;
        int[] duplicates = CheckForDuplicateColors(colors);
        if(duplicates.Length == 0) {
            this.gameObject.GetComponent<MainMenu>().StartGame();
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
            //Debug.Log(message);
        }
    }

    int[] CheckForDuplicateColors(Color[] colors)
    {
        Dictionary<Color, List<int>> colorIndices = new Dictionary<Color, List<int>>();
        List<int> duplicates = new List<int>();

        for (int i = 0; i < colors.Length; i++)
        {
            Color color = colors[i];
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


}
