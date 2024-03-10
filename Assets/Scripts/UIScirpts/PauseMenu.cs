using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public SaveManager saveManager;
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject cityMenu;
    public static bool isPaused;

    public GameObject saveNameInputWindow;
    private string tempSaveRoot;

    // Start is called before the first frame update
    void Start()
    {
        if(SaveRoot.saveRoot == null) {
            DisableQuickSave();
        }
        pauseMenu.SetActive(false);
        saveNameInputWindow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            cityMenu.SetActive(false);
            if(isPaused&& optionMenu.activeSelf==true) 
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
        saveManager.SetSaveRoot(tempSaveRoot);
        saveManager.Save();
        ResumeGame();
    }

    private void DisableQuickSave() {
        Button button = pauseMenu.transform.Find("Buttons/QuickSaveButton/Button_1 Gray").GetComponent<Button>();
        button.interactable = false;
    }

    public void EnableQuickSave() {
        Button button = pauseMenu.transform.Find("Buttons/QuickSaveButton/Button_1 Gray").GetComponent<Button>();
        button.interactable = true;
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
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
