using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;

public class LoadManager : MonoBehaviour
{
    private GameManager gameManager;
    private string saveRoot;

    public void Init(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void SetSaveRoot(string saveRoot) { 
        this.saveRoot = saveRoot;
    }

    public List<SaveGameDescription> LoadSaveGameDecriptions() {
        Debug.Log("Loading save game options");

        List<SaveGameDescription> saveGameDescriptions = new List<SaveGameDescription>();
        QuickSaveReader quickSaveReader = QuickSaveReader.Create("SavesList");
        int numberOfSavedGames = quickSaveReader.Read<int>("numberOfSavedGames");
        Debug.Log(numberOfSavedGames);

        for(int i = 0; i < numberOfSavedGames; i++) {
            string saveString = quickSaveReader.Read<string>("saveString"+i);
            string saveDate = quickSaveReader.Read<string>("saveDate"+i);

            saveGameDescriptions.Add(new SaveGameDescription(saveString, saveDate));
        }

        return saveGameDescriptions;
    }

    public void PrepareExampleLoads() {
        Debug.Log("Preparing example loads...");

        QuickSaveWriter quickSaveWriter1 = QuickSaveWriter.Create("Empty save 1");
        QuickSaveWriter quickSaveWriter2 = QuickSaveWriter.Create("Empty save 2");
        QuickSaveWriter quickSaveWriter3 = QuickSaveWriter.Create("SavesList");
        quickSaveWriter3.Write<int>("numberOfSavedGames", 2);
        quickSaveWriter3.Write<string>("saveString0", "Empty save 1");
        quickSaveWriter3.Write<string>("saveString1", "Empty save 2");
        quickSaveWriter3.Write<string>("saveDate0", DateTime.Now.ToString());
        quickSaveWriter3.Write<string>("saveDate1", DateTime.Now.ToString());
        quickSaveWriter1.Commit();
        quickSaveWriter2.Commit();
        quickSaveWriter3.Commit();
    }

    // We want to load information into a SceneLoadData object 
    // and then put it into the universal scene starting flow
    // in place of default values when starting a game

    // Load order:
    // - Load() entry point, calls game manager save and then player state loads
    // - LoadGameManager(...) loads GameManager data
    // - LoadPlayer(...) loads player data into StartingResources and calls unit load
    // - LoadUnit(...) loads single unit data into player's StartingResources

    public SceneLoadData Load() 
    {
        SceneLoadData sceneLoadData = new SceneLoadData();
        QuickSaveReader quickSaveReader = QuickSaveReader.Create(saveRoot);
        sceneLoadData = LoadGameManager(sceneLoadData, quickSaveReader);
        
        StartingResources[] startingResources = new StartingResources[sceneLoadData.numberOfPlayers];

        for(int i = 0; i < gameManager.numberOfPlayers; i++) {
            //startingResources[i] = LoadPlayer(player, quickSaveReader, i);
        }
        sceneLoadData.startingResources = startingResources;
        return sceneLoadData;
    }

    private SceneLoadData LoadGameManager(SceneLoadData sceneLoadData, QuickSaveReader quickSaveReader)
    {
        sceneLoadData.numberOfPlayers = quickSaveReader.Read<int>("numberOfPlayers");
        sceneLoadData.turnNumber = quickSaveReader.Read<int>("turnNumber");
        sceneLoadData.activePlayerIndex = quickSaveReader.Read<int>("activePlayerIndex");
        sceneLoadData.playerPositions = quickSaveReader.Read<Vector3[]>("playerPositions");

        return sceneLoadData;
    }

    // private StartingResources LoadPlayer(PlayerManager player, QuickSaveReader quickSaveReader, int index) 
    // {
    //     string playerKey = "Player" + index;
    //     StartingResources startingResources = new StartingResources();
    //     //All player data
        
    //     string numberOfUnits = quickSaveReader.Read<int>(playerKey + "numberOfUnits");
    //     startingResources.units = new UnitController[numberOfUnits];
    //     //player.isComputer = quickSaveReader.Read<bool>(playerKey + "isComputer");
    //     //player.color = ColorUtility.TryParseHtmlString(quickSaveReader.Read<string>(playerKey + "color"));
    //     startingResources.gold = quickSaveReader.Read<int>(playerKey + "gold");
    //     //player.goldIncome = quickSaveReader.Read<int>(playerKey + "goldIncome");
       
    //     for(int i = 0; i < numberOfUnits; i++) {
    //     {
    //         player.allyUnits.Add(this.LoadUnit(quickSaveReader, i));          
    //     }
    // }

    // private UnitController LoadUnit(QuickSaveReader quickSaveReader, int index)
    // {
    //     var path = "Prefabs/Units/"
    //     blocks[0] = Resources.Load<GameObject>(path);
    //     string unitKey = playerKey + "unit" + unitIndex;
    //     unit.unitType = quickSaveReader.Read<string>(unitKey + "unitType");
    //     unit.maxHealth = quickSaveReader.Read<int>(unitKey + "maxHealth");
    //     unit.currentHealth = quickSaveReader.Read<int>(unitKey + "currentHealth");
    //     unit.attack = quickSaveReader.Read<int>(unitKey + "attack");
    //     unit.attackRange = quickSaveReader.Read<int>(unitKey + "attackRange");
    //     unit.baseProductionCost = quickSaveReader.Read<int>(unitKey + "baseProductionCost");
    //     unit.turnsToProduce = quickSaveReader.Read<int>(unitKey + "turnsToProduce");
    //     unit.turnProduced = quickSaveReader.Read<int>(unitKey + "turnProduced");
    //     unit.level = quickSaveReader.Read<int>(unitKey + "level");
    // }

}