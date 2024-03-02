using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadData
{
    public SceneLoadData(int numberOfPlayers, Vector3[] playerPositions, 
    StartingResources[] startingResources, Color32[] playerColors, string[] startingCityNames, 
    int turnNumber, int activePlayerIndex) {
        this.numberOfPlayers = numberOfPlayers;
        this.playerPositions = playerPositions;
        this.startingResources = startingResources;
        this.playerColors = playerColors;
        this.startingCityNames = startingCityNames;
        this.turnNumber = turnNumber;
        this.activePlayerIndex = activePlayerIndex;
    }

    public SceneLoadData() {}
    public int numberOfPlayers;
    public Vector3[] playerPositions;
    public StartingResources[] startingResources;
    public Color32[] playerColors;
    public string[] startingCityNames;
    public int turnNumber;
    public int activePlayerIndex;
}