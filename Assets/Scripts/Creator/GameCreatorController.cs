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

    void Start()
    {
        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        
        mapDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(mapDropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        gameSettings.GetComponent<GameSettings>().SetMapName(change.options[change.value].text);
    }

    public void OnFormLoad() {
        gameSettings.mapName = "RiverDelta";
    }


}
