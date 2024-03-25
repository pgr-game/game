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

    void Start()
    {
        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        
        mapDropdown.onValueChanged.AddListener(delegate {
            MapNameDropdownValueChanged(mapDropdown);
        });

        numberOfPlayersSlider.onValueChanged.AddListener(delegate {
            NumberOfPlayersSliderValueChanged(numberOfPlayersSlider);
        });
    }

    // Setting the map name in the GameSettings object
    void MapNameDropdownValueChanged(TMP_Dropdown change)
    {
        gameSettings.GetComponent<GameSettings>().SetMapName(change.options[change.value].text);
    }

    // Setting the number of players in the GameSettings object
    void NumberOfPlayersSliderValueChanged(Slider change)
    {
        gameSettings.GetComponent<GameSettings>().numberOfPlayers = (int)change.value;
        numberOfPlayersText.text = change.value.ToString();
    }

    public void OnFormLoad() {
        if(gameSettings==null) {
            Start();
        }
        gameSettings.mapName = "RiverDelta";
        gameSettings.numberOfPlayers = 2;
    }


}
