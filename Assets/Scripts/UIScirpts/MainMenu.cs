using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit(); 
    }



    public void StartGame() {
        var gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        if(gameSettings.mapName == null) {
            gameSettings.mapName = "RiverDelta";
        }

        if (SaveRoot.mapName != null)
        {
            // loaded map overrides default and selection
            gameSettings.mapName = SaveRoot.mapName;
        }

        SceneManager.LoadScene(gameSettings.mapName);
    }
}
