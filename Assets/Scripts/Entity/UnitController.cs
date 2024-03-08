using RedBjorn.ProtoTiles;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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
    public int baseProductionCost;
    public int turnsToProduce;
    public int turnProduced;
    public int level = 1;
    public int defense;
    public GameManager gameManager;
    public GameObject unitUI;
    public GameObject damageRecivedUI;
    public GameObject lvlUPMenu;

    public GameObject fortButton;

    public bool attacked;
    public int experience = 0;
    public bool canPlaceFort;
    public int turnsSinceFortPlaced = 5;
    public int expirience = 0;

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager) {
        this.owner = playerManager;
        this.mapManager = mapManager;
        CreateUI();
        ApplyColor();
        unitMove.Init(mapManager,this);
        this.gameManager = gameManager;
        if(currentHealth == null || currentHealth == 0) {
            currentHealth = maxHealth;
        }

        if(turnProduced == null) {
            turnProduced = this.gameManager.turnNumber;
        }

        if(level == null || level == 0) {
            level = 1;
        }
        canPlaceFort = true;
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

        GameObject dejm = myUI.transform.Find("Frame").gameObject;
        Image dejmimage = dejm.GetComponent<Image>();
        dejmimage.color = this.owner.color;
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
        ChangeUnitTexts();
        this.gameManager.ShowUnitBox();
    }

    public void ChangeUnitTexts() {
        this.gameManager.setUnitTypeText(unitType.ToString());
        this.gameManager.setUnitAttackText(attack.ToString());
        this.gameManager.setUnitLevelText(level.ToString());
        this.gameManager.setUnitHealthText(currentHealth.ToString());
        this.gameManager.setUnitDefenseText(defense.ToString());
    }

    public void Deactivate() 
    {
        unitMove.Deactivate();
        this.gameManager.HideUnitBox();

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

    public int GetDefense() {
        if(IsInFortOrCity()) {
            if(defense == 0) {
                return 1;
            }
            return defense*2;
        }
        return defense;
    }

    public void Attack(UnitController enemy)
    {
        if (!this.attacked)
        {
            int damage = this.attack - enemy.GetDefense();
            if (damage < 0) damage = 0;
            this.attacked = true;
            enemy.reciveDamage(damage,this);
        }

    }

    public void reciveDamage(int incomingDamage, UnitController attacker)
    {
        this.currentHealth = this.currentHealth - incomingDamage;
        GameObject unitUI = this.transform.Find("UnitDefaultBar(Clone)").gameObject;
        GameObject damageUI = Instantiate(damageRecivedUI, unitUI.transform.position, Quaternion.identity, unitUI.transform);
        damageUI.transform.Find("Damage").gameObject.GetComponent<TextMeshProUGUI>().text = incomingDamage.ToString();
        damageUI.GetComponent<DamageAnimation>().angle = this.transform.position - attacker.transform.position;

        this.UpdateUnitUI();
        if (this.currentHealth <= 0)
        {
            this.Death(attacker);
        }
    }
    public void Death(UnitController killer) {
        owner.allyUnits.Remove(this);
        gameManager.units.Remove(this);
        killer.owner.AddGold(CalculateGoldValue());
        TileEntity oldTile = this.mapManager.MapEntity.Tile(this.unitMove.hexPosition);
        oldTile.UnitPresent = null;
        killer.GainXP(this.level);
        Destroy(gameObject);
    }

    public void GainXP(int ammountGot)
    {
        experience += ammountGot;
        if (experience >= System.Math.Pow(2, level - 1))
        {
            Debug.Log("lvl up");
            level++;
            experience = 0;
            this.UpgradeUnit();
        }
    }

    public void UpgradeUnit()
    {
        //TODO this not work work me sad. still displays are and path (for some reason onyl sonetimes)
        this.unitMove.Deactivate();

        GameObject unitUI = this.transform.Find("UnitDefaultBar(Clone)").gameObject;
        GameObject lvlUP = Instantiate(lvlUPMenu, this.transform.position, Quaternion.identity, unitUI.transform);
        lvlUP.transform.position += new Vector3(0,0,-2);
        lvlUP.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        GameObject buttons = lvlUP.transform.Find("Buttons").gameObject;

        GameObject defButton = buttons.transform.Find("DefenceAddButton").gameObject;
        UnityEngine.UI.Button button = defButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { UbgradeDefence(lvlUP); });

        GameObject HPButton = buttons.transform.Find("HPAddButton").gameObject;
        button = HPButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { UbgradeHealth(lvlUP); });

        GameObject attackButton = buttons.transform.Find("AttackAddButton").gameObject;
        button = attackButton.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { UbgradeAttack(lvlUP); });

    }

    public void UbgradeAttack(GameObject lvlUPMenu)
    {
        this.attack += 5;
        Destroy(lvlUPMenu);
        UpdateUnitUI();
        ChangeUnitTexts();
    }
    public void UbgradeHealth(GameObject lvlUPMenu)
    {
        this.maxHealth += 5;
        this.currentHealth += 5;
        Destroy(lvlUPMenu);
        UpdateUnitUI();
        ChangeUnitTexts();
    }
    public void UbgradeDefence(GameObject lvlUPMenu)
    {
        this.defense += 2;
        Destroy(lvlUPMenu);
        UpdateUnitUI();
        ChangeUnitTexts();
    }

    public int CalculateGoldValue() {
        // algorithm based on turn produced, unit type and level
        int goldValue = this.level*2;
        goldValue += gameManager.turnNumber - this.turnProduced;
    
        if(unitType == UnitTypes.Archer) goldValue+=10;
        else if(unitType == UnitTypes.Catapult) goldValue+=20;
        else if(unitType == UnitTypes.Chariot) goldValue+=15;
        else if(unitType == UnitTypes.Elephant) goldValue+=25;
        else if(unitType == UnitTypes.Hoplite) goldValue+=10;
        else if(unitType == UnitTypes.LightInfantry) goldValue+=5;
        else if(unitType == UnitTypes.Skirmisher) goldValue+=5;

        return goldValue;
    }

    public int CalculateProductionCost(City city) {
        // algorithm based on unity type, city level and age
        int productionCost = this.baseProductionCost; //based on unit type
        productionCost -= city.Level;
        productionCost -= (gameManager.turnNumber - city.turnCreated);
        return productionCost;
    }
    public int GetProductionTurns() {
        return turnsToProduce;
    }

    public bool IsInFortOrCity() {
        var tile = this.mapManager.MapEntity.Tile(unitMove.hexPosition);
        if(tile.FortPresent != null) return true;
        var isCity = false;
        tile.Preset.Tags.ForEach(tag => {
            if(tag == gameManager.cityTag) isCity = true;
        });
        return isCity;
    }

    public void Heal() {
        if(currentHealth == maxHealth) return;
        currentHealth += (int)(0.2f*maxHealth);
        if(currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUnitUI();
        ChangeUnitTexts();
        Debug.Log("Healing unit");
    }
}
