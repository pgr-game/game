using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    public PlayerManager owner;
    public MapManager mapManager;
    public UnitMove unitMove;
    public UnitTypes unitType;
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int attackRange;
    public GameManager gameManager;
    public GameObject unitUI;

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager) {
        this.owner = playerManager;
        this.mapManager = mapManager;
        CreateUI();
        ApplyColor();
        unitMove.Init(mapManager);
        this.gameManager = gameManager;
        currentHealth = maxHealth;
    }

    private void CreateUI()
    {
        GameObject myUI = Instantiate(unitUI, transform.position + new Vector3(0, 2, 0), Quaternion.identity, gameObject.transform);
        myUI.transform.localScale = new Vector3(0.1f,0.1f,0.1f);

        GameObject unitName = myUI.transform.Find("UnitName").gameObject;
        TMP_Text nameText = unitName.GetComponent<TMP_Text>();
        nameText.text = unitType.ToString();

        GameObject unitAttack = myUI.transform.Find("AttackValue").gameObject;
        TMP_Text attackText = unitAttack.GetComponent<TMP_Text>();
        attackText.text = attack.ToString();

        GameObject hpMeter = myUI.transform.Find("HpMeter").gameObject;
        Image hpMeterValue = hpMeter.GetComponent<Image>();
        float value = 1;
        hpMeterValue.fillAmount = value;
    }

    private void UpdateUnitUI()
    {
        GameObject myUI = gameObject.transform.Find("UnitDefaultBar(Clone)").gameObject;

        GameObject unitAttack = myUI.transform.Find("AttackValue").gameObject;
        TMP_Text attackText = unitAttack.GetComponent<TMP_Text>();
        attackText.text = attack.ToString();

        GameObject hpMeter = myUI.transform.Find("HpMeter").gameObject;
        Image hpMeterValue = hpMeter.GetComponent<Image>();
        float value = (float)currentHealth/(float)maxHealth;
        hpMeterValue.fillAmount = value;
    }

    public void Activate()
    {
        unitMove.Activate();
        UpdateUnitUI();
        this.gameManager.setUnitTypeText(unitType.ToString());
        this.gameManager.setUnitAttackText(attack.ToString());
    }

    public void Deactivate() 
    {
        unitMove.Deactivate();
        this.gameManager.setUnitTypeText("");
        this.gameManager.setUnitAttackText("");

    }

    private void ApplyColor()
    {
        GameObject body = transform.Find("RotationNode/Body").gameObject;
        if(body == null) {
            Debug.Log("Unit body not found, likely the prefab structure was changed!");
            return;
        }
        Debug.Log("Setting unit color");
        body.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", owner.color);
    }

    public void death() {
        Destroy(gameObject);
        owner.allyUnits.Remove(this);
        gameManager.units.Remove(this);
    }
}
