using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI.Dialogs;

public class CityMenuManager : MonoBehaviour
{
    private GameManager gameManager;
    public RectTransform dialogContainer;
    public GameObject UnitEntryPrefab;
    public City city;
    public GameObject createSupplyLineButton;

    private Text cityNameText;
    private GameObject unitsContainer;
    private List<GameObject> unitEntriesInList;



    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        this.unitEntriesInList = new List<GameObject>();
        this.cityNameText = this.gameObject.transform.Find("Info/CityName/Text").GetComponent<Text>();
        this.unitsContainer = this.gameObject.transform.Find("Scroll View/Viewport/Content").gameObject;
        this.dialogContainer = GameObject.Find("UI/DialogContainer").GetComponent<RectTransform>();
    }

    public void Deactivate()
    {
        gameManager.activePlayer.isInMenu = false;
        this.gameObject.SetActive(false);
        PauseMenu.isPaused = false;
        Time.timeScale = 1f;
    }

    public void Activate()
    {
        gameManager.activePlayer.isInMenu = true;
        this.gameObject.SetActive(true);
        PauseMenu.isPaused = true;
        Time.timeScale = 0f;
    }

    public void setValues(City city)
    {
        this.city = city;
        if (city != null)
        {
            this.cityNameText.text = city.Name;
            FillUnitsList(gameManager.unitPrefabs);
        }
        bool isSupplyLineReaserched = this.gameManager.playerTreeManager.isNodeResearched(4, "Strategy");
        createSupplyLineButton.GetComponent<Button>().interactable = isSupplyLineReaserched;

        if (city.besieged && !city.supplied)
        {
            foreach (var item in unitEntriesInList)
            {
                if(item != null)
                {
                    item.transform.Find("button").gameObject.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void ClickCreateSupplyLine()
    {
        Deactivate();
        city.CreateSupplyLine();
    }

    public void ClickSelectProductionUnit(GameObject clickedEntry, UnitController unitController, GameObject prefab)
    {
        if (city.UnitInProduction == null)
        {
            //no unit was previously selected
            SelectProductionUnit(clickedEntry, unitController, prefab);
        }
        else if (unitController == city.UnitInProduction)
        {
            if(city.UnitInProductionTurnsLeft != city.UnitInProduction.GetProductionTurns())
            {
                uDialog.NewDialog()
                    .SetTitleText("Cancelling production")
                    .SetContentText("Are you sure? Production progress for currently produced unit will be lost!")
                    .SetDimensions(468, 192)
                    .SetModal()
                    .SetShowTitleCloseButton(false)
                    .AddButton("Cancel production", () =>
                    {
                        ClearProduction();
                    })
                    .AddButton("Don't cancel", () => { })
                    .SetCloseWhenAnyButtonClicked(true)
                    .SetDestroyAfterClose(true)
                    .SetShowAnimation(eShowAnimation.None)
                    .SetCloseAnimation(eCloseAnimation.None)
                    .SetParent(dialogContainer);
            }
            else
            {
                ClearProduction();
            }
        }
        else if (city.UnitInProductionTurnsLeft == city.UnitInProduction.GetProductionTurns())
        {
            //changing unit that has not progressed its production does not require dialog confirmation
            SelectProductionUnit(clickedEntry, unitController, prefab);
        }
        else
        {
            uDialog.NewDialog()
                   .SetTitleText("Changing production")
                   .SetContentText("Are you sure? Production progress for currently produced unit will be lost!")
                   .SetDimensions(468, 192)
                   .SetModal()
                   .SetShowTitleCloseButton(false)
                   .AddButton("Change production", () => { SelectProductionUnit(clickedEntry, unitController, prefab); })
                   .AddButton("Cancel", () => { })
                   .SetCloseWhenAnyButtonClicked(true)
                   .SetDestroyAfterClose(true)
                   .SetShowAnimation(eShowAnimation.None)
                   .SetCloseAnimation(eCloseAnimation.None)
                   .SetParent(dialogContainer);
        }
    }

    private void ClearProduction()
    {
        city.ClearProduction();
        SetEntryColorToSelected(null);
    }

    public void SelectProductionUnit(string name)
    {
        GameObject clickedEntry = unitEntriesInList.Find(entry => entry.name == name);
        GameObject prefab = gameManager.getUnitPrefabByName(name);
        UnitController unit = prefab.GetComponent<UnitController>();

        SelectProductionUnit(clickedEntry, unit, prefab);
    }

    public void SelectProductionUnit(GameObject clickedEntry, UnitController unitController, GameObject prefab)
    {
        city.SetUnitInProduction(unitController, prefab);
        SetEntryColorToSelected(clickedEntry);
    }

    private void SetEntryColorToSelected(GameObject clickedEntry)
    {
        //reset all entries' colors
        foreach (Transform child in unitsContainer.transform)
        {
            Image background = child.transform.Find("button/Frame").GetComponent<Image>();
            if (child.gameObject == clickedEntry)
            {
                background.color = new Color32(118, 99, 27, 255);
            }
            else
            {
                background.color = new Color32(240, 166, 63, 255);
            }
        }
    }

    private void FillUnitsList(GameObject[] unitPrefabs)
    {
        foreach (Transform child in unitsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int i = 0;
        foreach (GameObject prefab in unitPrefabs)
        {
            UnitController unit = prefab.GetComponent<UnitController>();
            GameObject newEntry = Instantiate(UnitEntryPrefab, unitsContainer.transform.position + new Vector3(160, i, 0), Quaternion.identity, unitsContainer.transform);

            GameObject unitName = newEntry.transform.Find("name").gameObject;
            TMP_Text nameText = unitName.GetComponent<TMP_Text>();
            nameText.text = unit.unitType.ToString();

            GameObject unitCurrectntHp = newEntry.transform.Find("hp").gameObject;
            TMP_Text hpText = unitCurrectntHp.GetComponent<TMP_Text>();
            hpText.text = unit.GetProductionTurns().ToString();

            GameObject unitCurrectntAttack = newEntry.transform.Find("attack").gameObject;
            TMP_Text attackText = unitCurrectntAttack.GetComponent<TMP_Text>();
            attackText.text = "turns";

            GameObject unitCurrectntImage = newEntry.transform.Find("Image").gameObject;
            Image image = unitCurrectntImage.GetComponent<Image>();
            image.sprite = prefab.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;

            if (city.unitInProductionPrefab == prefab)
            {
                SetEntryColorToSelected(newEntry);
            }

            GameObject button = newEntry.transform.Find("button").gameObject;
            button.GetComponent<Button>().interactable = this.gameManager.playerTreeManager.isUnitUnlocked(nameText.text);
            Button buttonEvent = button.GetComponent<Button>();
            buttonEvent.onClick.AddListener(delegate { ClickSelectProductionUnit(newEntry, unit, prefab); });

            unitEntriesInList.Add(newEntry);
            i -= 80;
        }
    }

}
