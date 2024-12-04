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
        List<SaveGameDescription> saveGameDescriptions = new List<SaveGameDescription>();
        QuickSaveReader quickSaveReader = QuickSaveReader.Create("SavesList");
        int numberOfSavedGames = quickSaveReader.Read<int>("numberOfSavedGames");

        for(int i = 0; i < numberOfSavedGames; i++) {
            string saveString = quickSaveReader.Read<string>("saveString"+i);
            string mapName = "";
            if (quickSaveReader.Exists("mapName" + i))
            {
                mapName = quickSaveReader.Read<string>("mapName" + i);
            }
            string saveDate = quickSaveReader.Read<string>("saveDate"+i);

            saveGameDescriptions.Add(new SaveGameDescription(saveString, mapName, saveDate));
        }

        return saveGameDescriptions;
    }

    public void CreateSaveFilesFile() {
        try {
            QuickSaveReader quickSaveReader = QuickSaveReader.Create("SavesList");
            return;
        } catch (QuickSaveException) {
            QuickSaveWriter quickSaveWriter = QuickSaveWriter.Create("SavesList");
            quickSaveWriter.Write<int>("numberOfSavedGames", 0);
            quickSaveWriter.Commit();
        }
    }

    public void PrepareExampleLoads() {
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
        
        sceneLoadData.playerColors = new Color32[sceneLoadData.numberOfPlayers];
        sceneLoadData.startingCityNames = new string[sceneLoadData.numberOfPlayers];

        for(int i = 0; i < sceneLoadData.numberOfPlayers; i++) {
            string colorString = "#" + quickSaveReader.Read<string>("Player" + i + "color");
            Color convertedColor = ColorUtility.TryParseHtmlString(colorString, out convertedColor) ? convertedColor : new Color();
            Color32 convertedColor32 = (Color32)convertedColor;
            sceneLoadData.playerColors[i] = convertedColor32;
            sceneLoadData.startingCityNames[i] = "NULL";
        }

        return sceneLoadData;
    }

    public StartingResources[] LoadStartingResources(int numberOfPlayers)
    {
        QuickSaveReader quickSaveReader = QuickSaveReader.Create(saveRoot);
        StartingResources[] startingResources = new StartingResources[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        {
            startingResources[i] = LoadPlayer(quickSaveReader, i);
        }

        return startingResources;
    }

    public StartingUnits[] LoadStartingUnits(int numberOfPlayers)
    {
        QuickSaveReader quickSaveReader = QuickSaveReader.Create(saveRoot);
        StartingUnits[] startingUnits = new StartingUnits[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        {
            startingUnits[i] = LoadPlayerUnits(quickSaveReader, i);
        }

        return startingUnits;
    }

    private SceneLoadData LoadGameManager(SceneLoadData sceneLoadData, QuickSaveReader quickSaveReader)
    {
        sceneLoadData.numberOfPlayers = quickSaveReader.Read<int>("numberOfPlayers");
        sceneLoadData.turnNumber = quickSaveReader.Read<int>("turnNumber");
        sceneLoadData.activePlayerIndex = quickSaveReader.Read<int>("activePlayerIndex");
        sceneLoadData.playerPositions = quickSaveReader.Read<Vector3[]>("playerPositions");
        sceneLoadData.difficulty = quickSaveReader.Read<string>("difficulty");
        sceneLoadData.isComputer = quickSaveReader.Read<bool[]>("isComputer");
        sceneLoadData.isMultiplayer = quickSaveReader.Read<bool>("isMultiplayer");

		return sceneLoadData;
    }

    private StartingResources LoadPlayer(QuickSaveReader quickSaveReader, int index) 
    {
        string playerKey = "Player" + index;
        StartingResources startingResources = gameManager.NewStartingResources();
        
        int numberOfForts = quickSaveReader.Read<int>(playerKey + "numberOfForts");
        int numberOfCities = quickSaveReader.Read<int>(playerKey + "numberOfCities");
        int numberOfSupplyLines = quickSaveReader.Read<int>(playerKey + "numberOfSupplyLines");
        startingResources.gold = quickSaveReader.Read<int>(playerKey + "gold");

        for(int i = 0; i < numberOfForts; i++)
        {
            startingResources.fortLoadData.Add(LoadFortData(quickSaveReader, playerKey, i));          
        }

        for(int i = 0; i < numberOfCities; i++)
        {
            startingResources.cityLoadData.Add(LoadCityData(quickSaveReader, playerKey, i));          
        }

        for (int i = 0; i < numberOfSupplyLines; i++)
        {
            startingResources.supplyLoadData.Add(LoadSupplyLineData(quickSaveReader, playerKey, i));
        }

        startingResources.treeLoadData = LoadTreeData(quickSaveReader, playerKey);

        return startingResources;
    }

    private StartingUnits LoadPlayerUnits(QuickSaveReader quickSaveReader, int index)
    {
        string playerKey = "Player" + index;
        StartingUnits startingUnits = new StartingUnits();

        int numberOfUnits = quickSaveReader.Read<int>(playerKey + "numberOfUnits");
        startingUnits.units = new List<UnitController>();
        startingUnits.unitLoadData = new List<UnitLoadData>();


        for (int i = 0; i < numberOfUnits; i++)
        {
            string unitType = quickSaveReader.Read<string>(playerKey + "unit" + i + "unitType");
            startingUnits.units.Add(LoadUnitPrefab(unitType));
            startingUnits.unitLoadData.Add(LoadUnitData(quickSaveReader, playerKey, i));
        }

        return startingUnits;
    }

    private UnitController LoadUnitPrefab(string unitType)
    {
        GameObject prefabObject = gameManager.getUnitPrefabByName(unitType);
        if (prefabObject == null) {
            return null;
        } else
        {
            return gameManager.getUnitPrefabByName(unitType).GetComponent<UnitController>();
        }
    }

    private UnitLoadData LoadUnitData(QuickSaveReader quickSaveReader, string playerKey, int unitIndex)
    {
        string unitKey = playerKey + "unit" + unitIndex;

        UnitLoadData unitLoadData = new UnitLoadData(
            quickSaveReader.Read<Vector3>(unitKey + "position"),
            quickSaveReader.Read<string>(unitKey + "unitType"),
            quickSaveReader.Read<int>(unitKey + "maxHealth"),
            quickSaveReader.Read<int>(unitKey + "currentHealth"),
            quickSaveReader.Read<int>(unitKey + "attack"),
            quickSaveReader.Read<int>(unitKey + "attackRange"),
            quickSaveReader.Read<int>(unitKey + "baseProductionCost"),
            quickSaveReader.Read<int>(unitKey + "turnsToProduce"),
            quickSaveReader.Read<int>(unitKey + "turnProduced"),
            quickSaveReader.Read<int>(unitKey + "level"),
            quickSaveReader.Read<int>(unitKey + "experience"),
            quickSaveReader.Read<float>(unitKey + "rangeLeft"),
            quickSaveReader.Read<bool>(unitKey + "attacked"),
            quickSaveReader.Read<Vector3>(unitKey + "longPathClickPosition")
        );

        return unitLoadData;
    }

    private FortLoadData LoadFortData(QuickSaveReader quickSaveReader, string playerKey, int fortIndex)
    {
        string fortKey = playerKey + "fort" + fortIndex;

        FortLoadData fortLoadData = new FortLoadData(
            quickSaveReader.Read<Vector3>(fortKey + "position"),
            quickSaveReader.Read<Vector3Int>(fortKey + "hexPosition"),
            quickSaveReader.Read<int>(fortKey + "id")
        );

        return fortLoadData;
    }

    private CityLoadData LoadCityData(QuickSaveReader quickSaveReader, string playerKey, int index)
    {
        string cityKey = playerKey + "city" + index;

        CityLoadData cityLoadData = new CityLoadData(
            quickSaveReader.Read<Vector3>(cityKey + "position"),
            quickSaveReader.Read<string>(cityKey + "name"),
            quickSaveReader.Read<int>(cityKey + "level"),
            quickSaveReader.Read<string>(cityKey + "unitInProduction"),
            quickSaveReader.Read<int>(cityKey + "unitInProductionTurnsLeft")
        );

        return cityLoadData;
    }

    private SupplyLoadData LoadSupplyLineData(QuickSaveReader quickSaveReader, string playerKey, int index)
    {
        string supplyLineKey = playerKey + "supplyLine" + index;

        SupplyLoadData supplyLoadData = new SupplyLoadData(
            quickSaveReader.Read<Vector3>(supplyLineKey + "startPosition"),
            quickSaveReader.Read<Vector3>(supplyLineKey + "endPosition")
        );

        return supplyLoadData;
    }

    private TreeLoadData LoadTreeData(QuickSaveReader quickSaveReader, string playerKey)
    {
        string treeKey = playerKey + "tree";

        var powerEvolution = new Dictionary<int, List<string>>();
        int powerEvolutionCount = quickSaveReader.Read<int>(treeKey + "powerEvolutionCount");
        for(int i = 0; i < powerEvolutionCount; i++)
        {
            powerEvolution.Add(
                quickSaveReader.Read<int>(treeKey + "powerEvolutionKey" + i),
                quickSaveReader.Read<List<string>>(treeKey + "powerEvolutionList" + i)
            );
        }

        var strategyEvolution = new Dictionary<int, List<string>>();
        int strategyEvolutionCount = quickSaveReader.Read<int>(treeKey + "strategyEvolutionCount");
        for (int i = 0; i < strategyEvolutionCount; i++)
        {
            strategyEvolution.Add(
                quickSaveReader.Read<int>(treeKey + "strategyEvolutionKey" + i),
                quickSaveReader.Read<List<string>>(treeKey + "strategyEvolutionList" + i)
            );
        }

        (int, string) researchNode = quickSaveReader.Read<(int, string)>(treeKey + "researchNode");

        TreeLoadData treeLoadData = new TreeLoadData(powerEvolution, strategyEvolution, researchNode);

        return treeLoadData;
    }
}