using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WIKIController : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject ButtonPrefab;
    public GameObject PauseMenuObject;
    public List<GameObject> Chapters;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ButtonPress()
    {
        if (gameManager.isMultiplayer && !gameManager.activePlayer.IsOwner)
        {
            return;
        }
        if (gameManager.activePlayer.isInMenu)
        {
            if (this.gameObject.activeSelf)
            {
                PauseMenu.isPaused = false;
                gameManager.activePlayer.isInMenu = false;
                this.gameObject.SetActive(!this.gameObject.activeSelf);
                return;
            }
            PauseMenuObject.SetActive(!PauseMenuObject.activeSelf);
        }
        GameObject content = this.gameObject.transform.Find("Scroll View/Viewport/Content").gameObject;
        this.gameObject.SetActive(!this.gameObject.activeSelf);
        if (this.gameObject.activeSelf)
        {
            gameManager.activePlayer.isInMenu = true;
            PauseMenu.isPaused = true;
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (GameObject unitData in Chapters)
            {
                GameObject newEntry = Instantiate(ButtonPrefab, transform.position, Quaternion.identity, content.transform);
                GameObject unitName = newEntry.transform.Find("name").gameObject;
                TMP_Text nameText = unitName.GetComponent<TMP_Text>();
                nameText.text = unitData.name;

                GameObject button = newEntry.transform.Find("button").gameObject;
                Button buttonEvent = button.GetComponent<Button>();
                buttonEvent.onClick.AddListener(delegate { Buka(unitData.name); });
            }

        }
        else
        {
            PauseMenu.isPaused = false;
            gameManager.activePlayer.isInMenu = false;
        }
    }

    private void Buka(string i)
    {
        GameObject content = this.gameObject.transform.Find("ScrollArea/TextContainer/Text (TMP)").gameObject;
        var save = content.transform.position;
        GameObject  nntext = Chapters.Find(x=> x.gameObject.name.Equals(i)).gameObject;
        content.GetComponent<TMP_Text>().text = nntext.GetComponent<TMP_Text>().text;
        content.GetComponent<TMP_Text>().fontSize = nntext.GetComponent<TMP_Text>().fontSize;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, nntext.GetComponent<RectTransform>().sizeDelta.y);
        content.transform.position = new Vector3(save.x, -10000, save.z);
        
    }
}
