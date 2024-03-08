using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsUIController : MonoBehaviour
{
    public GameObject unitTypeText;
    public GameObject unitAttackText;
    public GameObject unitLevelText;
    public GameObject unitHealthText;
    public GameObject unitDefenseText;
    public GameObject unitBox;

    void Start()
    {
        HideUnitBox();
    }


    public void setUnitTypeText(string unitType) {
        unitTypeText.GetComponent<TMPro.TextMeshProUGUI>().text = unitType;
    }
    public void setUnitAttackText(string unitAttack)
    {
        unitAttackText.GetComponent<TMPro.TextMeshProUGUI>().text = unitAttack;
    }

    public void setUnitLevelText(string unitLevel)
    {
        unitLevelText.GetComponent<TMPro.TextMeshProUGUI>().text = unitLevel;
    }

    public void setUnitHealthText(string unitHealth)
    {
        unitHealthText.GetComponent<TMPro.TextMeshProUGUI>().text = unitHealth;
    }

    public void setUnitDefenseText(string unitDefense)
    {
        unitDefenseText.GetComponent<TMPro.TextMeshProUGUI>().text = unitDefense;
    }

    public void HideUnitBox() {
        unitBox.SetActive(false);
    }

    public void ShowUnitBox() {
        unitBox.SetActive(true);
    }

}
