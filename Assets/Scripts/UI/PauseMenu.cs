using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public SaveManager saveManager;
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject cityMenu;
    public GameObject wikiMenu;
    public GameObject unitList;
    public static bool isPaused;

    public GameObject saveNameInputWindow;
    private string tempSaveRoot;

    // Start is called before the first frame update
    void Start()
    {
        if(SaveRoot.saveRoot == null) {
            DisableQuickSave();
        }

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            DisableBackToMenu();
        }
        pauseMenu.SetActive(false);
        saveNameInputWindow.SetActive(false);
        wikiMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            unitList.SetActive(false);
            cityMenu.SetActive(false);
            wikiMenu.SetActive(false);
            if (isPaused&& optionMenu.activeSelf==true) 
            {
                optionMenu.SetActive(false);
                saveNameInputWindow.SetActive(false);
                pauseMenu.SetActive(true);
            }
            else if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void SetTempSaveRoot(string tempSaveRoot) {
        Debug.Log("Entered save root: " + tempSaveRoot);
        this.tempSaveRoot = tempSaveRoot;
    }

    public void QuickSave() {
        //Overwrite save from saveRoot if it is not null
        if(saveManager.IsSaveRootNull()) {
            OpenSaveNameInput();
        }
        else {
            saveManager.Save();
        }
    }

    public void OpenSaveNameInput() {
        this.saveNameInputWindow.SetActive(true);
    }

    public void CreateNewSaveFile() {
        saveManager.CreateSaveFilesFile();
        saveManager.SetSaveRoot(tempSaveRoot);
        saveManager.Save();
        ResumeGame();
    }

    private void DisableBackToMenu()
    {
        GameObject button = pauseMenu.transform.Find("Buttons/MenuButton").gameObject;
        button.SetActive(false);
    }
    private void DisableQuickSave() {
        Button button = pauseMenu.transform.Find("Buttons/QuickSaveButton/Button_1 Gray").GetComponent<Button>();
        button.interactable = false;
    }

    public void EnableQuickSave() {
        Button button = pauseMenu.transform.Find("Buttons/QuickSaveButton/Button_1 Gray").GetComponent<Button>();
        button.interactable = true;
    }

    public void SettingsButtonPress()
    {
        if(saveManager.gameManager.activePlayer.isInMenu)
        {
            ResumeGame();
        }
        else
        {
            unitList.SetActive(false);
            cityMenu.SetActive(false);
            wikiMenu.SetActive(false);
            PauseGame();
        }
    }
    public void PauseGame()
    {
        saveManager.gameManager.activePlayer.isInMenu = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        saveManager.gameManager.activePlayer.isInMenu = false;
        pauseMenu.SetActive(false);
        saveNameInputWindow.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
