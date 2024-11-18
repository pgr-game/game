using RedBjorn.ProtoTiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UnitController : NetworkBehaviour, INetworkSerializable
{
    public PlayerUnitsManager playerUnitsManager;
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
    public UnitStatsMenuController unitStatsMenuController;

    public List<AudioClip> moveSounds = new List<AudioClip>();

    public GameObject fortButton;

    public bool attacked;
    public int experience = 0;
    public bool canPlaceFort;
    public int turnsSinceFortPlaced = 10;

    // Multiplayer
    public NetworkVariable<int> ownerIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
	    base.OnNetworkSpawn();

		// This is only to initiate unit on client
		if (owner) return;

		owner = FindObjectsByType<PlayerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			.FirstOrDefault(playerManager => playerManager.index == ownerIndex.Value);

		if (owner)
		{
			// TODO arguments should not be 0
			Init(owner, owner.mapManager, owner.gameManager, owner.gameManager.unitStatsMenuController, null, null);
		}
    }

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager, UnitStatsMenuController unitStatsMenuController, float? rangeLeft, Vector3? longPathClickPosition)
    {
        this.owner = playerManager;
        if(IsServer)
			ownerIndex.Value = owner.index;
		this.playerUnitsManager = playerManager.playerUnitsManager;
        this.mapManager = mapManager;
        this.unitStatsMenuController = unitStatsMenuController;
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

        // Add to city garrison if in city
        var path = playerManager.mapManager.MapEntity.PathTiles(transform.position, transform.position, 1);
        var tile = path.Last();
        if (tile.CityTilePresent)
        {
	        tile.CityTilePresent.city.AddToGarrison(this);
        }

        playerUnitsManager.AddIfNotInList(this);
	}

	public void Activate()
    {
        unitMove.Activate();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsMenuController.activeUnit = this;
        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        unitStatsMenuController.ShowUnitBox();
    }

    public void Deactivate()
    {
        unitMove.Deactivate();
        unitStatsMenuController.activeUnit = null;
        unitStatsMenuController.HideUnitBox();
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
        this.unitStatsMenuController.UpdateUnitStatisticsWindow(this);
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
        this.unitStatsMenuController.UpdateUnitStatisticsWindow(this);
    }

    private bool CanAttackCity(City city)
    {
        city.UpdateBesiegedStatus();
        if(city.attackingPlayers.Contains(owner))
        {
            return true;
        }
        else return false;
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
        playerUnitsManager.RemoveUnit(this);
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
        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        return null;
    }
    public Action UpgradeHealth()
    {
        maxHealth += 5;
        currentHealth += 5;
        unitUI.HideUpgradeUnitMenu();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        return null;
    }
    public Action UpgradeDefence()
    {
        defense += 2;
        unitUI.HideUpgradeUnitMenu();
        unitUI.UpdateUnitUI(currentHealth, maxHealth);
        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
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
            this.unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        }
    }

    public void CommitToBuildingFort()
    {
        var tile = GetCurrentTile();
        if (!IsInFort(tile)) return;
        if (tile.FortPresent.owner != this.owner) return;
        if (tile.FortPresent.isBuilt) return;
        tile.FortPresent.ProgressBuild(tile);
    }

    void OnMouseOver()
    {
        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        unitStatsMenuController.ShowUnitBox();
        unitMove.ShowLongPath();
    }

    void OnMouseExit()
    {
        if(!unitMove.active)
        {
            unitMove.HideLongPath();
        }

        if (!unitStatsMenuController.activeUnit)
        {
            unitStatsMenuController.HideUnitBox();
        }
        else
        {
            unitStatsMenuController.UpdateUnitStatisticsWindow(unitStatsMenuController.activeUnit);
        }
    }

    public void DoTurn()
    {
        Debug.Log(unitMove.hexPosition);
    }
    
    public List<UnitController> FindUnitsInRange(int range, Vector3Int hexPosition)
    {
        var tilesInRange = owner.mapManager.GetTilesInRange(range, hexPosition);
        var unitsInRange = (from tile in tilesInRange where tile.UnitPresent != null select tile.UnitPresent).ToList();
        
        // remove self
        unitsInRange.Remove(this);
        return unitsInRange;
    }
    
    public List<City> FindCitiesInRange(int range, Vector3Int hexPosition)
    {
        var tilesInRange = owner.mapManager.GetTilesInRange(range, hexPosition);
        var citiesInRange = (from tile in tilesInRange where tile.CityTilePresent != null select tile.CityTilePresent.city).ToList();
        
        // remove repetition
        citiesInRange = citiesInRange.Distinct().ToList();
        return citiesInRange;
    }

    public List<Fort> FindFortsInRange(int range, Vector3Int hexPosition)
    {
        var tilesInRange = owner.mapManager.GetTilesInRange(range, hexPosition);
        return (from tile in tilesInRange where tile.FortPresent != null select tile.FortPresent).ToList();
    }
    
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref maxHealth);
		serializer.SerializeValue(ref currentHealth);
		serializer.SerializeValue(ref attack);
		serializer.SerializeValue(ref attackRange);
		serializer.SerializeValue(ref baseProductionCost);
		serializer.SerializeValue(ref turnsToProduce);
		serializer.SerializeValue(ref turnProduced);
		serializer.SerializeValue(ref level);
		serializer.SerializeValue(ref defense);
		serializer.SerializeValue(ref experience);
		serializer.SerializeValue(ref attacked);
		serializer.SerializeValue(ref canPlaceFort);
		serializer.SerializeValue(ref turnsSinceFortPlaced);
	}

    public float GetRiskOfAttacking(UnitController enemy)
    {
        // high risk -> 1, don't attack, low risk -> 0, attakck
        
        // things to consider:
        // 1. enemy attack, enemy defense, enemy health,
        // 2. player attack, player defense, player health
        // 3. distance between units
        // 4. enemy and friendly units nearby
        // 5. forts or cities nearby enemy (healing and defense bonus)
        
        // how many hits for enemy to kill player/enemy
        int hitsToKillPlayer = (int)Math.Ceiling(this.currentHealth / (float)(enemy.GetAttack() - this.GetDefense()));
        int hitsToKillEnemy = (int)Math.Ceiling(enemy.currentHealth / (float)(this.GetAttack() - enemy.GetDefense()));
        float hitsRatio = hitsToKillEnemy / (float)hitsToKillPlayer;
        
        // distance between units
        var distance = mapManager.MapEntity.Distance(this.unitMove.hexPosition, enemy.unitMove.hexPosition);
        int howManyTurnsToGetThere = (int) Math.Ceiling(distance / this.unitMove.Range);
        
        // units close (5 tiles) to player and enemy
        var unitsInRange5OfPlayer = FindUnitsInRange(5, this.unitMove.hexPosition);
        var unitsInRange5OfEnemy = FindUnitsInRange(5, enemy.unitMove.hexPosition);

        List<UnitController> friendlyUnitsClose = new List<UnitController>();
        List<UnitController> enemyUnitsClose = new List<UnitController>();
        foreach (var unit in unitsInRange5OfPlayer)
        {
            if (unit.owner == this.owner)
            {
                friendlyUnitsClose.Add(unit);
            }
            else
            {
                enemyUnitsClose.Add(unit);
            }
        }
        foreach (var unit in unitsInRange5OfEnemy)
        {
            if (unit.owner == this.owner)
            {
                friendlyUnitsClose.Add(unit);
            }
            else
            {
                enemyUnitsClose.Add(unit);
            }
        }
        float closeUnitsRatio = 
            enemyUnitsClose.Count * enemyUnitsClose.Sum(unit => unit.currentHealth) * enemyUnitsClose.Sum(unit => unit.attack) / 
            (float)friendlyUnitsClose.Count * friendlyUnitsClose.Sum(unit => unit.currentHealth) * friendlyUnitsClose.Sum(unit => unit.attack);
        
        // units far (10 tiles) to player and enemy
        var unitsInRange10OfPlayer = FindUnitsInRange(10, this.unitMove.hexPosition);
        var unitsInRange10OfEnemy = FindUnitsInRange(10, enemy.unitMove.hexPosition);
        
        List<UnitController> friendlyUnitsFar = new List<UnitController>();
        List<UnitController> enemyUnitsFar = new List<UnitController>();
        foreach (var unit in unitsInRange10OfPlayer)
        {
            if (unit.owner == this.owner)
            {
                friendlyUnitsFar.Add(unit);
            }
            else
            {
                enemyUnitsFar.Add(unit);
            }
        }
        foreach (var unit in unitsInRange10OfEnemy)
        {
            if (unit.owner == this.owner)
            {
                friendlyUnitsFar.Add(unit);
            }
            else
            {
                enemyUnitsFar.Add(unit);
            }
        }
        float farUnitsRatio = 
            enemyUnitsFar.Count * enemyUnitsFar.Sum(unit => unit.currentHealth) * enemyUnitsFar.Sum(unit => unit.attack)/ 
            (float)friendlyUnitsFar.Count * friendlyUnitsFar.Sum(unit => unit.currentHealth) * friendlyUnitsFar.Sum(unit => unit.attack);
        
        // cities and forts of enemy close
        var citiesInRange5OfEnemy = FindCitiesInRange(5, enemy.unitMove.hexPosition)
            .Where(city => city.Owner == enemy.owner)
            .ToList();

        var fortsInRange5OfEnemy = FindFortsInRange(5, enemy.unitMove.hexPosition)
            .Where(fort => fort.owner == enemy.owner)
            .ToList();
        
        var citiesInRange10OfEnemy = FindCitiesInRange(10, enemy.unitMove.hexPosition)
            .Where(city => city.Owner == enemy.owner)
            .ToList();
        
        var fortsInRange10OfEnemy = FindFortsInRange(10, enemy.unitMove.hexPosition)
            .Where(fort => fort.owner == enemy.owner)
            .ToList();
        
        float citiesAndFortsHelp = 0.1f*(citiesInRange5OfEnemy.Count + fortsInRange5OfEnemy.Count) +
                                   0.05f*(citiesInRange10OfEnemy.Count + fortsInRange10OfEnemy.Count);
        

    }
}
