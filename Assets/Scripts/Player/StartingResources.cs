using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using System;
using System.Collections.Generic;
using Unity.Netcode;

public class StartingResources : INetworkSerializable, IEquatable<StartingResources>
{
    public List<FortLoadData> fortLoadData;
    public List<CityLoadData> cityLoadData;
    public List<SupplyLoadData> supplyLoadData;
    public TreeLoadData treeLoadData;
    public int gold;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gold);

        // Serialize each list
        SerializeList(serializer, ref fortLoadData);
        SerializeList(serializer, ref cityLoadData);
        SerializeList(serializer, ref supplyLoadData);

        // Serialize treeLoadData
        treeLoadData ??= new TreeLoadData(
            new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(),
            (1, "node"));
        treeLoadData.NetworkSerialize(serializer);
    }

    private static void SerializeList<T, TValue>(BufferSerializer<T> serializer, ref List<TValue> list)
        where T : IReaderWriter where TValue : INetworkSerializable, new()
    {
        int count = serializer.IsReader ? 0 : list.Count;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new List<TValue>(count);
            for (int i = 0; i < count; i++)
            {
                TValue value = new TValue();
                value.NetworkSerialize(serializer);
                list.Add(value);
            }
        }
        else
        {
            foreach (var value in list)
            {
                value.NetworkSerialize(serializer);
            }
        }
    }

    public bool Equals(StartingResources other)
    {
        if (other == null) return false;

        return gold == other.gold;
    }

    public override bool Equals(object obj)
    {
        return obj is StartingResources other && Equals(other);
    }

    public override int GetHashCode()
    {
        return gold.GetHashCode();
    }
}