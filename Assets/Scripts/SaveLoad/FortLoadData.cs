using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FortLoadData : INetworkSerializable
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref hexPosition);
        serializer.SerializeValue(ref id);
    }
}