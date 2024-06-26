using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsUIController : MonoBehaviour
{
    public GameObject unitTypeText;
    public GameObject unitAttackText;
    public GameObject unitLevelText;
    public GameObject unitHealthText;
    public GameObject unitDefenseText;
    public GameObject unitBox;
    public GameObject xpSlider;
    public GameObject maxLvlText;
    public GameObject unitOwnerColor;
    public GameObject healthBackground;

    public UnitController activeUnit;
    void Start()
    {
        HideUnitBox();
    }

    private void Update()
    {
        if (activeUnit != null)
        {
            this.UpdateUnitStatisticsWindow(activeUnit);
        }
    }
    public void UpdateUnitStatisticsWindow(UnitController unitController)
    {
        unitBox.SetActive(true);

        unitTypeText.GetComponent<TMPro.TextMeshProUGUI>().text = unitController.unitType.ToString();

        unitAttackText.GetComponent<TMPro.TextMeshProUGUI>().text = unitController.GetAttack().ToString();

        unitLevelText.GetComponent<TMPro.TextMeshProUGUI>().text = unitController.level.ToString();

        unitHealthText.GetComponent<TMPro.TextMeshProUGUI>().text = unitController.currentHealth.ToString()+"/"+ unitController.maxHealth.ToString();

        unitDefenseText.GetComponent<TMPro.TextMeshProUGUI>().text = unitController.GetDefense().ToString();

        unitOwnerColor.GetComponent<Image>().color = unitController.owner.color;

        healthBackground.GetComponent<Image>().color = unitController.owner.color;

        if (unitController.level >= 5 )
        {
            maxLvlText.SetActive(true);
            xpSlider.SetActive(false);
        }
        else
        {
            maxLvlText.SetActive(false);
            xpSlider.SetActive(true);
            xpSlider.GetComponent<Slider>().value = (float)unitController.experience / (float)System.Math.Pow(2, unitController.level);
        }
        

    }

    public void HideUnitBox() {
        unitBox.SetActive(false);
    }

    public void ShowUnitBox() {
        unitBox.SetActive(true);
    }

}
