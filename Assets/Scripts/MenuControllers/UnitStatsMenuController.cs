using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsMenuController : MonoBehaviour
{
    public TextMeshProUGUI unitTypeText;
    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI unitLevelText;
    public TextMeshProUGUI unitHealthText;
    public TextMeshProUGUI unitDefenseText;
    public GameObject unitBox;
    public GameObject xpSlider;
    public GameObject maxLvlText;
    public Image unitOwnerColor;
    public Image healthBackground;

    public UnitController activeUnit;
    void Start()
    {
        HideUnitBox();
    }

    public void UpdateUnitStatisticsWindow(UnitController unitController)
    {
        unitBox.SetActive(true);

        unitTypeText.text = unitController.unitType.ToString();

        unitAttackText.text = unitController.GetAttack().ToString();

        unitLevelText.text = unitController.level.ToString();

        unitHealthText.text = unitController.currentHealth.ToString()+"/"+ unitController.maxHealth.ToString();

        unitDefenseText.text = unitController.GetDefense().ToString();

        unitOwnerColor.color = unitController.owner.color;

        healthBackground.color = unitController.owner.color;

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
