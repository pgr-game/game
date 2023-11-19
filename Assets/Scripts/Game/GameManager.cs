using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int turnNumber = 1;

    public int activePlayerIndex = 0;
    public Player[] players;


    public Player activePlayer;
    private int numberOfPlayers;

    public GameObject PlayerControlPrefab;
    private PlayerControl[] playerControl;
    // Start is called before the first frame update
    void Start()
    {
        numberOfPlayers = players.Length;
        activePlayer = players[activePlayerIndex];
        InstantiatePlayerControls();
    }

    private void InstantiatePlayerControls()
    {
        Debug.Log("Instantiating player controls");
        playerControl = new PlayerControl[numberOfPlayers];
        for(int i = 0; i < numberOfPlayers; i++) {
            playerControl[i] = Instantiate(PlayerControlPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PlayerControl>();
            playerControl[i].player = players[i];
            if(i == activePlayerIndex) {
                playerControl[i].gameObject.SetActive(true);
            }
            else {
                playerControl[i].gameObject.SetActive(false);
            }
        }
    }

    public void NextPlayer()
    {
        playerControl[activePlayerIndex].gameObject.SetActive(false);
        if(activePlayerIndex + 1 == numberOfPlayers) {
            activePlayerIndex = 0;
        }
        else {
            activePlayerIndex++;
        }
        activePlayer = players[activePlayerIndex];
        playerControl[activePlayerIndex].gameObject.SetActive(true);
        Debug.Log("Player " + activePlayerIndex + " turn");
    }
}
