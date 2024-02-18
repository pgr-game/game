using System.Collections;
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