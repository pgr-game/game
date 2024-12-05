using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CityLoadData : INetworkSerializable
{
    public Vector3 position;
    public string name;
    public int level;
    public string unitInProduction;
    public int unitInProductionTurnsLeft;
    public CityLoadData(Vector3 position, string name, int level, 
        string unitInProduction, int unitInProductionTurnsLeft)
    {
        this.position = position;
        this.name = name;
        this.level = level;
        this.unitInProduction = unitInProduction;
        this.unitInProductionTurnsLeft = unitInProductionTurnsLeft;
    }

    public CityLoadData() { }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref level);
        serializer.SerializeValue(ref unitInProduction);
        serializer.SerializeValue(ref unitInProductionTurnsLeft);
    }
}
