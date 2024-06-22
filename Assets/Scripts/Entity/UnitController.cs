using RedBjorn.ProtoTiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UnitController : MonoBehaviour
{
    public PlayerManager owner;
    public MapManager mapManager;
    public UnitMove unitMove;
    public UnitUI unitUI;
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
    public UnitStatsUIController unitStatsUIController;

    public List<AudioClip> moveSounds = new List<AudioClip>();

    public GameObject fortButton;

    public bool attacked;
    public int experience = 0;
    public bool canPlaceFort;
    public int turnsSinceFortPlaced = 10;

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager, UnitStatsUIController unitStatsUIController, float? rangeLeft, Vector3? longPathClickPosition)
    {
        this.owner = playerManager;
        this.mapManager = mapManager;
        this.unitStatsUIController = unitStatsUIController;
        unitUI.Init(this, owner.color, unitType, attack);
        unitMove.Init(mapManager, this, rangeLeft, longPathClickPosition);

        this.gameManager = gameManager;
        if (currentHealth == 0)
        {
            currentHealth = maxHealth;
        }

        if (turnProduced == 0)
        {
            turnProduced = this.gameManager.turnNumber;
        }

        if (level == 0)
        {
            level = 1;
        }
        canPlaceFort = true;
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
    }


    public void Activate()
    {
        unitMove.Activate();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsUIController.activeUnit = this;
        unitStatsUIController.UpdateUnitStatisticsWindow(this);
        unitStatsUIController.ShowUnitBox();
    }

    public void Deactivate()
    {
        unitMove.Deactivate();
        unitStatsUIController.activeUnit = null;
        unitStatsUIController.HideUnitBox();
        this.owner.Deselect();

    }

    public bool IsSuppliedLined(TileEntity tile)
    {
        return (tile.SupplyLineProvider == this.owner);
    }



    public int GetDefense()
    {
        int bonusDefese = 0;
        var tile = GetCurrentTile();
        if (IsSuppliedLined(tile) == true)
        {
            bonusDefese = 1;
        }
        if (CanHealOrGetDefenceBonus() && this.gameManager.playerTreeManager.isNodeResearched(2, "Strategy"))
        {
            if (defense == 0)
            {
                bonusDefese = 1;
            }
            bonusDefese += defense;
        }
        return defense + bonusDefese;
    }

    public int GetAttack()
    {
        int bonusAttack = 0;
        var tile = GetCurrentTile();
        if (IsSuppliedLined(tile) == true)
        {
            bonusAttack = 1;
        }
        return bonusAttack + this.attack; 
    }

    public void Attack(UnitController enemy)
    {
        if (!this.attacked)
        {
            int damage = this.GetAttack() - enemy.GetDefense();
            if (damage < 0) damage = 0;
            this.attacked = true;
            enemy.ReceiveDamage(damage, this);
        }
        this.unitStatsUIController.UpdateUnitStatisticsWindow(this);
    }

    public void Attack(City enemy)
    {
        if (!this.attacked)
        {
            if(CanAttackCity(enemy))
            {
                int damage = this.GetAttack() - enemy.GetDefense();
                if (damage < 0) damage = 0;
                this.attacked = true;
                enemy.ReceiveDamage(damage, this);
            }
            else
            {
                //show message that no siege unit is in range
            }
        }
        else
        {
            //show message that already attacked? Or better yet remove the city from passable area
        }
        this.unitStatsUIController.UpdateUnitStatisticsWindow(this);
    }

    private bool CanAttackCity(City city)
    {
        List<UnitController> adjacentUnits = city.adjacentTiles.Select(tile => tile.UnitPresent).ToList();
        if (adjacentUnits.Any(item => (item?.unitType == UnitTypes.Catapult && item?.owner == owner)))
        {
            return true;
        }

        return false;
    }

    public void ReceiveDamage(int incomingDamage, UnitController attacker)
    {
        this.currentHealth = this.currentHealth - incomingDamage;

        unitUI.ShowDamageEffect(incomingDamage, attacker.transform.position);
        unitUI.UpdateUnitUI(currentHealth, maxHealth);

        if (this.currentHealth <= 0)
        {
            this.gameManager.soundManager.GetComponent<SoundManager>().PlayKillSound();
            this.Death(attacker);
        }
    }

    public void Death(UnitController killer)
    {
        owner.allyUnits.Remove(this);
        gameManager.units.Remove(this);
        killer.owner.AddGold(CalculateGoldValue());
        TileEntity oldTile = this.mapManager.MapEntity.Tile(this.unitMove.hexPosition);
        oldTile.UnitPresent = null;
        killer.GainXP(this.level);

        if (oldTile.CityTilePresent && oldTile.CityTilePresent.city.Owner == owner)
        {
            oldTile.CityTilePresent.city.RemoveFromGarrison(this);
        }

        Destroy(gameObject);
    }

    public void GainXP(int ammountGot)
    {
        if (!this.gameManager.playerTreeManager.isNodeResearched(1, "Power"))
        {//lvl up not researched so no lvl uping
            return;
        }
        experience += ammountGot;
        if (experience >= System.Math.Pow(2, level) && this.level < 5)
        {
            level++;
            experience = 0;
            unitMove.Deactivate();
            if (this.level == 2)
            {
                // instantiating at level 2 as some units will die before it
                // and not need the instantiated menu
                unitUI.InitUpgradeUnitMenu(UpgradeAttack(), UpgradeHealth(), UpgradeDefence());
            }
            unitUI.ShowUpgradeUnitMenu();
        }
    }

    public Action UpgradeAttack()
    {
        attack += 5;
        unitUI.HideUpgradeUnitMenu();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsUIController.UpdateUnitStatisticsWindow(this);
        return null;
    }
    public Action UpgradeHealth()
    {
        maxHealth += 5;
        currentHealth += 5;
        unitUI.HideUpgradeUnitMenu();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsUIController.UpdateUnitStatisticsWindow(this);
        return null;
    }
    public Action UpgradeDefence()
    {
        defense += 2;
        unitUI.HideUpgradeUnitMenu();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsUIController.UpdateUnitStatisticsWindow(this);
        return null;
    }

    public int CalculateGoldValue()
    {
        // algorithm based on turn produced, unit type and level
        int goldValue = this.level * 2;
        goldValue += gameManager.turnNumber - this.turnProduced;

        if (unitType == UnitTypes.Archer) goldValue += 10;
        else if (unitType == UnitTypes.Catapult) goldValue += 20;
        else if (unitType == UnitTypes.Chariot) goldValue += 15;
        else if (unitType == UnitTypes.Elephant) goldValue += 25;
        else if (unitType == UnitTypes.Hoplite) goldValue += 10;
        else if (unitType == UnitTypes.LightInfantry) goldValue += 5;
        else if (unitType == UnitTypes.Skirmisher) goldValue += 5;

        return goldValue;
    }

    public int CalculateProductionCost(City city)
    {
        // algorithm based on unity type, city level and age
        int productionCost = this.baseProductionCost; //based on unit type
        productionCost -= city.Level;
        productionCost -= (gameManager.turnNumber - city.turnCreated);
        return productionCost;
    }
    public int GetProductionTurns()
    {
        return turnsToProduce;
    }

    public TileEntity GetCurrentTile()
    {
        return mapManager.MapEntity.Tile(unitMove.hexPosition);
    }

    public bool IsInFort(TileEntity tile)
    {
        return (tile.FortPresent != null);
    }

    public bool IsInCity(TileEntity tile)
    {
        return (tile.CityTilePresent != null);
    }

    public bool CanPlaceFortOnTile()
    {
        var tile = GetCurrentTile();
        return !(IsInCity(tile) || IsInFort(tile));
    }

    public bool CanHealOrGetDefenceBonus()
    {
        var tile = GetCurrentTile();
        if (IsInCity(tile))
        {
            return (tile.CityTilePresent.city.Owner == this.owner);
        }
        if (IsInFort(tile))
        {
            return (tile.FortPresent.owner == this.owner && tile.FortPresent.isBuilt);
        }
        return false;
    }

    public bool CanStackUnits(TileEntity tile)
    {
        if (IsInCity(tile))
        {
            return (tile.CityTilePresent.city.Owner == this.owner);
        }
        if (IsInFort(tile))
        {
            if (tile.FortPresent.owner == this.owner) Debug.Log("Fort owner is the same as unit owner");
            return (tile.FortPresent.owner == this.owner);
        }
        return false;
    }

    public void Heal()
    {
        if (this.gameManager.playerTreeManager.isNodeResearched(3, "Strategy"))
        {
            if (currentHealth == maxHealth) return;
            currentHealth += (int)(0.2f * maxHealth);
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            unitUI.UpdateUnitUI(currentHealth, maxHealth);
            this.unitStatsUIController.UpdateUnitStatisticsWindow(this);
        }
    }

    public void CommitToBuildingFort()
    {
        var tile = GetCurrentTile();
        if (!IsInFort(tile)) return;
        if (tile.FortPresent.owner != this.owner) return;
        if (tile.FortPresent.isBuilt) return;
        tile.FortPresent.turnsUntilBuilt--;
        if (tile.FortPresent.turnsUntilBuilt == 0)
        {
            tile.FortPresent.BuildComplete();
        }
    }

    void OnMouseOver()
    {
        unitStatsUIController.UpdateUnitStatisticsWindow(this);
        unitStatsUIController.ShowUnitBox();
        unitMove.ShowLongPath();
    }

    void OnMouseExit()
    {
        unitMove.HideLongPath();
        if (!unitStatsUIController.activeUnit)
        {
            unitStatsUIController.HideUnitBox();
        }
        else
        {
            unitStatsUIController.UpdateUnitStatisticsWindow(unitStatsUIController.activeUnit);
        }
    }
}
