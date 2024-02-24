using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject cityMenu;
    public static bool isPaused;
    // Start is called before the first frame update
    void Start()
    {
        if(SaveRoot.saveRoot == null) {
            DisableQuickSave();
        }
        pauseMenu.SetActive(false);
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

    private void DisableQuickSave() {
        Button button = pauseMenu.transform.Find("Buttons/QuickSaveButton/Button_1 Gray").GetComponent<Button>();
        if(button != null) {
            Debug.Log("dupa");
        }
        button.interactable = false;
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
