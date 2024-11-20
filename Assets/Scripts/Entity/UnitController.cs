using System;
using System.Collections.Generic;
using System.Linq;
using RedBjorn.ProtoTiles;
using Unity.Netcode;
using UnityEngine;

//using UnityEditor.Experimental.GraphView;

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

    public List<AudioClip> moveSounds = new();

    public GameObject fortButton;

    public bool attacked;
    public int experience;
    public bool canPlaceFort;
    public int turnsSinceFortPlaced = 10;

    // Multiplayer
    public NetworkVariable<int> ownerIndex = new();

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // This is only to initiate unit on client
        if (owner) return;

        owner = FindObjectsByType<PlayerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(playerManager => playerManager.index == ownerIndex.Value);

        if (owner)
            // TODO: arguments should not be 0
            Init(owner, owner.mapManager, owner.gameManager, owner.gameManager.unitStatsMenuController, null, null);
    }

    public void Init(PlayerManager playerManager, MapManager mapManager, GameManager gameManager,
        UnitStatsMenuController unitStatsMenuController, float? rangeLeft, Vector3? longPathClickPosition)
    {
        owner = playerManager;
        if (IsServer)
            ownerIndex.Value = owner.index;
        playerUnitsManager = playerManager.playerUnitsManager;
        this.mapManager = mapManager;
        this.unitStatsMenuController = unitStatsMenuController;
        unitUI.Init(this, owner.color, unitType, attack);
        unitMove.Init(mapManager, this, rangeLeft, longPathClickPosition);

        this.gameManager = gameManager;
        if (currentHealth == 0) currentHealth = maxHealth;

        if (turnProduced == 0) turnProduced = this.gameManager.turnNumber;

        if (level == 0) level = 1;
        canPlaceFort = true;
        unitUI.UpdateUnitUI(currentHealth, maxHealth);

        // Add to city garrison if in city
        var path = playerManager.mapManager.MapEntity.PathTiles(transform.position, transform.position, 1);
        var tile = path.Last();
        if (tile.CityTilePresent) tile.CityTilePresent.city.AddToGarrison(this);

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
        owner.Deselect();
    }

    public bool IsSuppliedLined(TileEntity tile)
    {
        return tile.SupplyLineProvider == owner;
    }



    public int GetDefense()
    {
        var bonusDefese = 0;
        var tile = GetCurrentTile();
        if (IsSuppliedLined(tile)) bonusDefese = 1;
        if (CanHealOrGetDefenceBonus() && gameManager.playerTreeManager.isNodeResearched(2, "Strategy"))
        {
            if (defense == 0) bonusDefese = 1;
            bonusDefese += defense;
        }

        return defense + bonusDefese;
    }

    public int GetAttack()
    {
        var bonusAttack = 0;
        var tile = GetCurrentTile();
        if (IsSuppliedLined(tile)) bonusAttack = 1;
        return bonusAttack + attack;
    }

    public void Attack(UnitController enemy)
    {
        if (!attacked)
        {
            var damage = GetAttack() - enemy.GetDefense();
            if (damage < 0) damage = 0;
            attacked = true;
            enemy.ReceiveDamage(damage, this);
        }

        unitStatsMenuController.UpdateUnitStatisticsWindow(this);
    }

    public void Attack(City enemy)
    {
        if (!this.attacked)
        {
            if(CanAttackCity(enemy))
            {
                var damage = GetAttack() - enemy.GetDefense();
                if (damage < 0) damage = 0;
                attacked = true;
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
        if (city.attackingPlayers.Contains(owner)) return true;

        return false;
    }

    public void ReceiveDamage(int incomingDamage, UnitController attacker)
    {
        currentHealth = currentHealth - incomingDamage;

        unitUI.ShowDamageEffect(incomingDamage, attacker.transform.position);
        unitUI.UpdateUnitUI(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            gameManager.soundManager.GetComponent<SoundManager>().PlayKillSound();
            Death(attacker);
        }
    }

    public void Death(UnitController killer)
    {
        playerUnitsManager.RemoveUnit(this);
        killer.owner.AddGold(CalculateGoldValue());
        var oldTile = mapManager.MapEntity.Tile(unitMove.hexPosition);
        oldTile.UnitPresent = null;
        killer.GainXP(level);

        if (oldTile.CityTilePresent && oldTile.CityTilePresent.city.Owner == owner)
            oldTile.CityTilePresent.city.RemoveFromGarrison(this);

        Destroy(gameObject);
    }

    public void GainXP(int ammountGot)
    {
        if (!gameManager.playerTreeManager.isNodeResearched(1, "Power"))
            //lvl up not researched so no lvl uping
            return;
        experience += ammountGot;
        if (experience >= Math.Pow(2, level) && level < 5)
        {
            level++;
            experience = 0;
            unitMove.Deactivate();
            if (level == 2)
                // instantiating at level 2 as some units will die before it
                // and not need the instantiated menu
                unitUI.InitUpgradeUnitMenu(UpgradeAttack(), UpgradeHealth(), UpgradeDefence());
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
        var goldValue = level * 2;
        goldValue += gameManager.turnNumber - turnProduced;

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
        var productionCost = baseProductionCost; //based on unit type
        productionCost -= city.Level;
        productionCost -= gameManager.turnNumber - city.turnCreated;
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
        return tile.FortPresent != null;
    }

    public bool IsInCity(TileEntity tile)
    {
        return tile.CityTilePresent != null;
    }

    public bool CanPlaceFortOnTile()
    {
        var tile = GetCurrentTile();
        return !(IsInCity(tile) || IsInFort(tile));
    }

    public bool CanHealOrGetDefenceBonus()
    {
        var tile = GetCurrentTile();
        if (IsInCity(tile)) return tile.CityTilePresent.city.Owner == owner;
        if (IsInFort(tile)) return tile.FortPresent.owner == owner && tile.FortPresent.isBuilt;
        return false;
    }

    public bool CanStackUnits(TileEntity tile)
    {
        if (IsInCity(tile)) return tile.CityTilePresent.city.Owner == owner;
        if (IsInFort(tile))
        {
            if (tile.FortPresent.owner == owner) Debug.Log("Fort owner is the same as unit owner");
            return tile.FortPresent.owner == owner;
        }

        return false;
    }

    public void Heal()
    {
        if (gameManager.playerTreeManager.isNodeResearched(3, "Strategy"))
        {
            if (currentHealth == maxHealth) return;
            currentHealth += (int)(0.2f * maxHealth);
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            unitUI.UpdateUnitUI(currentHealth, maxHealth);
            unitStatsMenuController.UpdateUnitStatisticsWindow(this);
        }
    }

    public void CommitToBuildingFort()
    {
        var tile = GetCurrentTile();
        if (!IsInFort(tile)) return;
        if (tile.FortPresent.owner != owner) return;
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
        Debug.Log("**AI** Unit turn: " + unitType);
        
        
        var enemiesInRange = FindUnitsInRange(10, unitMove.hexPosition).
            Where(unit => unit.owner != this.owner)
            .ToList();

        foreach (var enemyUnit in enemiesInRange)
        {
            Debug.Log("Enemy unit in range: " + enemyUnit.unitType + ", risk of attacking: " + GetRiskOfAttacking(enemyUnit));
        }
        
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
        var citiesInRange =
            (from tile in tilesInRange where tile.CityTilePresent != null select tile.CityTilePresent.city).ToList();

        // remove repetition
        citiesInRange = citiesInRange.Distinct().ToList();
        return citiesInRange;
    }

    public List<Fort> FindFortsInRange(int range, Vector3Int hexPosition)
    {
        var tilesInRange = owner.mapManager.GetTilesInRange(range, hexPosition);
        return (from tile in tilesInRange where tile.FortPresent != null select tile.FortPresent).ToList();
    }

    // high risk -> 1, don't attack, low risk -> 0, attack
    public float GetRiskOfAttacking(UnitController enemy)
    {
        // include catapult needed to attack city, if this player doesn't have a catapult unlocked and there is a city close to enemy, don't attack (risk = 1)
        var isCatapultUnlocked = owner.gameManager.playerTreeManager.isNodeResearched(4, "Power");
        var isCityNearEnemy = FindCitiesInRange(5, enemy.unitMove.hexPosition).Any(city => city.Owner == enemy.owner);
        
        if(!isCatapultUnlocked && isCityNearEnemy) return 1;

        
        // how many hits for enemy to kill player/enemy
        var hitsRatio = GetHitsRatio(enemy);

        // distance between units
        var howManyTurnsToGetThere = GetTurnsDistanceToEnemy(enemy);

        // units close (5 tiles) to player and enemy
        var closeUnitsRatio = GetUnitsRatio(enemy, 5);

        // units far (10 tiles) to player and enemy
        var farUnitsRatio = GetUnitsRatio(enemy, 10);

        // cities and forts help ratio
        var citiesFortsHelpRatio = GetCitiesAndFortsHelpRatio(enemy);


        // max value of each
        const int maxHitsRatio = 10;
        const int maxUnitsRatio = 200;
        const int maxCitiesFortsHelpRatio = 5;

        // normalize
        hitsRatio /= maxHitsRatio;
        closeUnitsRatio /= maxUnitsRatio;
        farUnitsRatio /= maxUnitsRatio;
        citiesFortsHelpRatio /= maxCitiesFortsHelpRatio;


        // TODO: adjust the weights if needed
        // IF turns to get there is less than 2, then close units ratio and hits ratio is more important, if its more than 10
        if (howManyTurnsToGetThere < 2)
            return (0.5f * hitsRatio + 0.3f * closeUnitsRatio + 0.2f * farUnitsRatio + 0.2f * citiesFortsHelpRatio) /
                   1.2f;
        if (howManyTurnsToGetThere < 10)
            return (0.2f * hitsRatio + 0.3f * closeUnitsRatio + 0.5f * farUnitsRatio + 0.2f * citiesFortsHelpRatio) /
                   1.2f;
        return 0.1f * hitsRatio + 0.1f * closeUnitsRatio + 0.2f * farUnitsRatio + 0.6f * citiesFortsHelpRatio;
    }

    private float GetCitiesAndFortsHelpRatio(UnitController enemy)
    {
        // cities and forts of enemy
        var citiesAndFortsHelpEnemy = GetCitiesAndFortsHelpRatio(enemy, enemy);

        // cities and forts of player
        var citiesAndFortsHelpPlayer = GetCitiesAndFortsHelpRatio(enemy, this);

        float citiesFortsHelp;
        if (citiesAndFortsHelpPlayer == 0) citiesFortsHelp = 5f;
        else
            citiesFortsHelp = citiesAndFortsHelpEnemy / citiesAndFortsHelpPlayer;

        if (citiesFortsHelp > 5) citiesFortsHelp = 5;

        return citiesFortsHelp;
    }

    private float GetCitiesAndFortsHelpRatio(UnitController enemy, UnitController owner)
    {
        var citiesInRange5OfEnemy = FindCitiesInRange(5, enemy.unitMove.hexPosition)
            .Where(city => city.Owner == owner.owner)
            .ToList();

        var fortsInRange5OfEnemy = FindFortsInRange(5, enemy.unitMove.hexPosition)
            .Where(fort => fort.owner == owner.owner)
            .ToList();

        var citiesInRange10OfEnemy = FindCitiesInRange(10, enemy.unitMove.hexPosition)
            .Where(city => city.Owner == owner.owner)
            .ToList();

        var fortsInRange10OfEnemy = FindFortsInRange(10, enemy.unitMove.hexPosition)
            .Where(fort => fort.owner == owner.owner)
            .ToList();

        var citiesAndFortsHelp = 0.1f * (citiesInRange5OfEnemy.Count + fortsInRange5OfEnemy.Count) +
                                 0.05f * (citiesInRange10OfEnemy.Count + fortsInRange10OfEnemy.Count);
        return citiesAndFortsHelp;
    }

    // TODO: check if units near player should count less than units near enemy
    private float GetUnitsRatio(UnitController enemy, int range)
    {
        var unitsInRangeOfPlayer = FindUnitsInRange(range, unitMove.hexPosition);
        var unitsInRangeOfEnemy = FindUnitsInRange(range, enemy.unitMove.hexPosition);

        var friendlyUnitsClose = new List<UnitController>();
        var enemyUnitsClose = new List<UnitController>();

        foreach (var unit in unitsInRangeOfPlayer)
            if (unit.owner == owner)
            {
                if (unit != this)
                    friendlyUnitsClose.Add(unit);
            }
            else
            {
                enemyUnitsClose.Add(unit);
            }

        foreach (var unit in unitsInRangeOfEnemy)
            if (unit.owner == owner)
            {
                if (unit != this)
                    friendlyUnitsClose.Add(unit);
            }
            else
            {
                enemyUnitsClose.Add(unit);
            }

        float closeUnitsRatio;

        if (friendlyUnitsClose.Count * friendlyUnitsClose.Sum(unit => unit.currentHealth) *
            friendlyUnitsClose.Sum(unit => unit.attack) == 0) closeUnitsRatio = enemyUnitsClose.Count * enemyUnitsClose.Sum(unit => unit.currentHealth) *
            enemyUnitsClose.Sum(unit => unit.attack)/0.001f;
        else
            closeUnitsRatio =
                enemyUnitsClose.Count * enemyUnitsClose.Sum(unit => unit.currentHealth) *
                enemyUnitsClose.Sum(unit => unit.attack) /
                (float)friendlyUnitsClose.Count * friendlyUnitsClose.Sum(unit => unit.currentHealth) *
                friendlyUnitsClose.Sum(unit => unit.attack);

        if (closeUnitsRatio > 200) closeUnitsRatio = 200;
        return closeUnitsRatio;
    }

    private int GetTurnsDistanceToEnemy(UnitController enemy)
    {
        var distance = mapManager.MapEntity.Distance(unitMove.hexPosition, enemy.unitMove.hexPosition);
        var howManyTurnsToGetThere = (int)Math.Ceiling(distance / unitMove.Range);

        return howManyTurnsToGetThere;
    }

    private float GetHitsRatio(UnitController enemy)
    {
        var hitsToKillPlayer = (int)Math.Ceiling(currentHealth / (float)(enemy.GetAttack() - GetDefense()));
        if (hitsToKillPlayer < 0) hitsToKillPlayer = 0;

        var hitsToKillEnemy = (int)Math.Ceiling(enemy.currentHealth / (float)(GetAttack() - enemy.GetDefense()));
        if (hitsToKillEnemy < 0) hitsToKillEnemy = 0;

        // edge case, both cannot kill each other, better not attack
        if (hitsToKillEnemy == 0 && hitsToKillPlayer == 0) return 10;
        
        float hitsRatio = 10;
        if (hitsToKillPlayer != 0)
            hitsRatio = hitsToKillEnemy / (float)hitsToKillPlayer;
        if (hitsRatio > 10) hitsRatio = 10;
        return hitsRatio;
    }
}