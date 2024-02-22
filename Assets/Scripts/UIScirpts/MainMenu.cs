using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void NewGame()
    {
        SceneManager.LoadScene("NewGameSetup");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("ChooseLoadGame");
    }
    
    public void QuitGame()
    {
        Application.Quit(); 
    }
}
