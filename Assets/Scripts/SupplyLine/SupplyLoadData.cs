using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public record SupplyLoadData : INetworkSerializable
{
    public Vector3 startPosition;
    public Vector3 endPosition;

    public SupplyLoadData(Vector3 startPosition, Vector3 endPosition)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
    }

    public SupplyLoadData() { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref startPosition);
        serializer.SerializeValue(ref endPosition);
    }
}
