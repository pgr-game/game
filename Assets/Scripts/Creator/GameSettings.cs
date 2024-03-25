using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public string mapName;
    public int numberOfPlayers;
    public Color[] playerColors;
    public Vector3[] playerPositions;
    public string difficulty;

    public void SetMapName(string name)
    {
        mapName = name.Replace(" ", "");
    }

    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }
}
