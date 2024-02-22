using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
    public GameObject loadContainer;
    public LoadManager loadManager;
    public GameObject ListItemPrefab;
    public GameObject selectedPrefab;
    public SaveGameDescription selectedSaveGameDescription;
    // Start is called before the first frame update
    void Start()
    {
        loadManager.PrepareExampleLoads();
        List<SaveGameDescription> saveGameDescriptions = loadManager.LoadSaveGameDecriptions();
        FillLoadList(saveGameDescriptions);
    }


    private void FillLoadList(List<SaveGameDescription> saveGameDescriptions) {
        foreach(Transform child in loadContainer.transform)
        {
             Destroy(child.gameObject);
        }

        int i = 0;
        foreach (SaveGameDescription saveGameDescription in saveGameDescriptions)
        {
            Debug.Log("Adding save to list");
            GameObject newEntry = Instantiate(ListItemPrefab, loadContainer.transform.position + new Vector3(100, 100, 0), 
            Quaternion.identity, loadContainer.transform);

            GameObject saveName = newEntry.transform.Find("name").gameObject;
            TMP_Text nameText = saveName.GetComponent<TMP_Text>();
            nameText.text = saveGameDescription.saveString;

            GameObject saveDate = newEntry.transform.Find("date").gameObject;
            TMP_Text saveText = saveDate.GetComponent<TMP_Text>();
            saveText.text = saveGameDescription.saveDate.ToString();

            if(selectedPrefab == newEntry) {
                SetEntryColorToSelected(newEntry);
            }

            GameObject  button = newEntry.transform.Find("button").gameObject;
            Button buttonEvent = button.GetComponent<Button>();
            buttonEvent.onClick.AddListener(delegate { SelectProductionUnit(newEntry); });
            i -= 80;
        }
    }

    private void SetEntryColorToSelected(GameObject clickedEntry) {
        //reset all entries' colors
        foreach(Transform child in loadContainer.transform)
        {
            Image background = child.transform.Find("button/Frame").GetComponent<Image>();
            if(child.gameObject == clickedEntry) {
                Debug.Log("setting entry color");
                background.color = new Color32(118, 99, 27, 255);
            } else {
                background.color = new Color32(240, 166, 63, 255);
            }
        }
    }

    public void SelectProductionUnit(GameObject clickedEntry) {
        string saveString = clickedEntry.transform.Find("name").gameObject.GetComponent<TMP_Text>().text;
        string dateString = clickedEntry.transform.Find("date").gameObject.GetComponent<TMP_Text>().text;
        selectedSaveGameDescription = new SaveGameDescription(saveString, dateString);
        SetEntryColorToSelected(clickedEntry);
        PlayerPrefs.SetString("saveRoot", saveString);
    }
}
