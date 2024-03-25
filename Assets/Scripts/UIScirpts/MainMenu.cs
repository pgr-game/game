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
        // todo: implement saving and loading a map selection
        var gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        SceneManager.LoadScene(gameSettings.mapName);
    }
}
