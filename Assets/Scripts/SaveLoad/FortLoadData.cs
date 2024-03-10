using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortLoadData
{
    public FortLoadData(Vector3 position, Vector3Int hexPosition, int id)
    {
        this.position = position;
        this.hexPosition = hexPosition;
        this.id = id;
    }
    public Vector3 position;
    public Vector3Int hexPosition;
    public int id;
}
