using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //these should only be used for instantiation and will be imported from game launcher later on
    public int InNumberOfPlayers = 3;
    public Vector3[] InPlayerPositions;
    public StartingResources[] startingResources;
    //end of game launcher variables

    public MapManager mapManager;
    public GameObject playerPrefab;

    public int turnNumber = 1;
    
    public int activePlayerIndex = 0;
    public PlayerManager activePlayer;
    private int numberOfPlayers;
    private PlayerManager[] players;

    void Start()
    {
        //this should later be called directly from game creator and not the Start function
        StartGame(InNumberOfPlayers, InPlayerPositions);
    }

    public void StartGame(int numberOfPlayers, Vector3[] playerPositions) {
        if(!IsInitialDataCorrect(numberOfPlayers, playerPositions)) {
            Debug.Log("Wrong initial data. Stopping game now!");
            return;
        }
        mapManager.Init();
        InstantiatePlayers(numberOfPlayers, playerPositions);
    }

    private void InstantiatePlayers(int numberOfPlayers, Vector3[] playerPositions)
    {
        Debug.Log("Instantiating players");
        this.numberOfPlayers = numberOfPlayers;
        players = new PlayerManager[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            players[i] = Instantiate(playerPrefab, playerPositions[i], Quaternion.identity).GetComponent<PlayerManager>();
            players[i].mapManager = mapManager;
            players[i].StartingResources = startingResources[i];
            if(i == activePlayerIndex) {
                players[i].gameObject.SetActive(true);
            }
            else {
                players[i].gameObject.SetActive(false);
            }
            players[i].Init();
        }
        activePlayer = players[activePlayerIndex];
    }

    public void NextPlayer()
    {
        players[activePlayerIndex].gameObject.SetActive(false);
        if(activePlayerIndex + 1 == numberOfPlayers) {
            activePlayerIndex = 0;
        }
        else {
            activePlayerIndex++;
        }
        activePlayer = players[activePlayerIndex];
        players[activePlayerIndex].gameObject.SetActive(true);
        Debug.Log("Player " + activePlayerIndex + " turn");
    }

    private bool IsInitialDataCorrect(int numberOfPlayers, Vector3[] playerPositions) 
    {
        if(startingResources.Length != numberOfPlayers) {
            Debug.Log("Wrong number of player starting resources!");
            return false;
        }
        if(playerPositions.Length != numberOfPlayers) {
            Debug.Log("Wrong number of player starting positions!");
            return false;
        }
        //should also check if positions in bounds or sth
        return true;
    }
}
