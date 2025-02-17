using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public GameObject unitUIPrefab;
    private GameObject unitUIObject;
    private UnitController unitController;
    private Color color;

    GameObject infoBar;
    GameObject lvlUpMenu;

    Image hpMeterValue;
    Color hpColor;

    public GameObject damageReceivedUIPrefab;
    public GameObject lvlUPMenuPrefab;

    // Start is called before the first frame update
    public void Init(UnitController unitController, Color color, UnitTypes unitType, int attack)
    {
        this.unitController = unitController;
        this.color = color;
        CreateUI(unitType, attack);
        ApplyColor();
    }

    private void CreateUI(UnitTypes unitType, int attack)
    {
        unitUIObject = Instantiate(unitUIPrefab, transform.position + new Vector3(0, 0, 0), Quaternion.identity, gameObject.transform);
        infoBar = this.transform.Find("UnitInfoBarDefault(Clone)").gameObject;
        unitUIObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        Color hpColor = color;
        hpColor.a = 0.8f;

        GameObject hpMeter = unitUIObject.transform.Find("HpMeter").gameObject;
        hpMeterValue = hpMeter.GetComponent<Image>();
        hpMeterValue.fillAmount = 1.0f;
        hpMeterValue.color = hpColor;

        hpColor.a = 0.3f;

        GameObject HpMeterBackground = unitUIObject.transform.Find("HpMeterBackground").gameObject;
        Image HpMeterBackgroundValue = HpMeterBackground.GetComponent<Image>();
        HpMeterBackgroundValue.color = hpColor;

        hpColor.a = 0.5f;

        GameObject HpMeterRim = unitUIObject.transform.Find("HpMeterRim").gameObject;
        Image HpMeterRimValue = HpMeterRim.GetComponent<Image>();
        HpMeterRimValue.color = hpColor;
    }

    private void ApplyColor()
    {
        GameObject body = transform.Find("RotationNode/Body").gameObject;
        if (body == null)
        {
            Debug.Log("Unit body not found, likely the prefab structure was changed!");
            return;
        }
        body.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", color);
    }

    public void UpdateUnitUI(int currentHealth, int maxHealth)
    {
        float value = (float)currentHealth / (float)maxHealth;
        hpMeterValue.fillAmount = value;
    }

    public void ShowDamageEffect(int incomingDamage, Vector3 attackerPosition)
    {
        GameObject damageUI = Instantiate(damageReceivedUIPrefab, infoBar.transform.position, Quaternion.identity, infoBar.transform);
        damageUI.transform.Find("Damage").gameObject.GetComponent<TextMeshProUGUI>().text = incomingDamage.ToString();
        damageUI.GetComponent<DamageAnimation>().angle = this.transform.position - attackerPosition;
    }

    public void ShowUpgradeUnitMenu()
    {
        lvlUpMenu.SetActive(true);
    }

    public void HideUpgradeUnitMenu()
    {
        lvlUpMenu.SetActive(false);
    }

    public void InitUpgradeUnitMenu(Action UpgradeDefence, Action UpgradeHealth, Action UpgradeAttack)
    {
        lvlUpMenu = Instantiate(lvlUPMenuPrefab, this.transform.position, Quaternion.identity, unitUIObject.transform);
        lvlUpMenu.transform.position += new Vector3(0, 0, -2);
        lvlUpMenu.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        GameObject buttons = lvlUpMenu.transform.Find("Buttons").gameObject;

        GameObject defenseButton = buttons.transform.Find("DefenceAddButton").gameObject;
        UnityEngine.UI.Button button = defenseButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { unitController.UpgradeDefence(); });

        GameObject HPButton = buttons.transform.Find("HPAddButton").gameObject;
        button = HPButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { unitController.UpgradeHealth(); });

        GameObject attackButton = buttons.transform.Find("AttackAddButton").gameObject;
        button = attackButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { unitController.UpgradeAttack(); });

        HideUpgradeUnitMenu();
    }
}