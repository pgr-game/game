using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        SaveRoot.saveRoot = null;
        SaveRoot.mapName = null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame(bool isMultiplayer)
    {
        var gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        if (gameSettings.mapName == null)
        {
            gameSettings.mapName = "RiverDelta";
        }

        if (SaveRoot.mapName != null)
        {
            // loaded map overrides default and selection
            gameSettings.mapName = SaveRoot.mapName;
        }

        if (isMultiplayer)
        {
            //TODO loading screen for host
            HostSingleton.Instance.StartHostAsync();
        }
        else
        {
            SceneManager.LoadScene(gameSettings.mapName);
        }
    }

    public void JoinGame()
    {
        SceneManager.LoadScene("MainMenu1", LoadSceneMode.Single);
    }
}
