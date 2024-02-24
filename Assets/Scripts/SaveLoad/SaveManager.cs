using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;

public class SaveManager : MonoBehaviour
{
    private GameManager gameManager;
    private string saveRoot;

    public void Init(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void SetSaveRoot(string saveRoot) { 
        this.saveRoot = saveRoot;
    }

    public void QuickSave() {
        //Overwrite save from saveRoot if it is not null
        if(saveRoot == null) {
            CreateNewSaveFile();
        }
        else {
            Save();
        }
    }

    public void CreateNewSaveFile() {
        
    }

    // Save order:
    // - Save() entry point, calls game manager save and then player state saves
    // - SaveGameManager(...) saves GameManager data
    // - SavePlayer(...) saves player data into starting resources and calls unit saves
    // - SaveUnit(...) saves single unit data as if it was part of player's StartingResources
    

    public void Save() 
    {
        QuickSaveWriter quickSaveWriter = QuickSaveWriter.Create(saveRoot);
        SaveGameManager(this.gameManager, quickSaveWriter);

        foreach(PlayerManager player in this.gameManager.players) {
            SavePlayer(player, quickSaveWriter);
        }
    }

    private void SaveGameManager(GameManager gameManager, QuickSaveWriter quickSaveWriter)
    {
        quickSaveWriter.Write<int>("numberOfPlayers", gameManager.numberOfPlayers);
        quickSaveWriter.Write<int>("turnNumber", gameManager.turnNumber);
        quickSaveWriter.Write<int>("activePlayerIndex", gameManager.activePlayerIndex);
        quickSaveWriter.Write<Vector3[]>("playerPositions", gameManager.playerPositions);
        quickSaveWriter.Commit();
    }

    private void SavePlayer(PlayerManager player, QuickSaveWriter quickSaveWriter) {
        string playerKey = "Player" + player.index;
        //All player data
        quickSaveWriter.Write<int>(playerKey + "numberOfUnits", player.allyUnits.Count);
        quickSaveWriter.Write<bool>(playerKey + "isComputer", player.isComputer);
        quickSaveWriter.Write<string>(playerKey + "color", ColorUtility.ToHtmlStringRGBA(player.color));
        quickSaveWriter.Write<int>(playerKey + "gold", player.gold);
        quickSaveWriter.Write<int>(playerKey + "goldIncome", player.goldIncome);
       
       int i = 0;
        foreach(UnitController unit in player.allyUnits)
        {
            SaveUnit(unit, quickSaveWriter, playerKey, i);    
            i++;      
        }
        quickSaveWriter.Commit();
    }

    private void SaveUnit(UnitController unit, QuickSaveWriter quickSaveWriter, string playerKey, int index)
    {
        string unitKey = playerKey + "unit" + index;
        quickSaveWriter.Write<string>(unitKey + "unitType", unit.unitType.ToString());
        quickSaveWriter.Write<int>(unitKey + "maxHealth", unit.maxHealth);
        quickSaveWriter.Write<int>(unitKey + "currentHealth", unit.currentHealth);
        quickSaveWriter.Write<int>(unitKey + "attack", unit.attack);
        quickSaveWriter.Write<int>(unitKey + "attackRange", unit.attackRange);
        quickSaveWriter.Write<int>(unitKey + "baseProductionCost", unit.baseProductionCost);
        quickSaveWriter.Write<int>(unitKey + "turnsToProduce", unit.turnsToProduce);
        quickSaveWriter.Write<int>(unitKey + "turnProduced", unit.turnProduced);
        quickSaveWriter.Write<int>(unitKey + "level", unit.level);
        quickSaveWriter.Commit();
    }
}