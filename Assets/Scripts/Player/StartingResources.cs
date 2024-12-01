using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Test : INetworkSerializable, IEquatable<Test>
{
    //public List<UnitController> units;
    //public List<UnitLoadData> unitLoadData;

    public List<FortLoadData> fortLoadData;
    public List<CityLoadData> cityLoadData;
    public List<SupplyLoadData> supplyLoadData;
    //public TreeLoadData treeLoadData;
    public int gold;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gold);

        fortLoadData ??= new List<FortLoadData>();
        SerializeList(serializer, ref fortLoadData);

        cityLoadData ??= new List<CityLoadData>();
        SerializeList(serializer, ref cityLoadData);

        supplyLoadData ??= new List<SupplyLoadData>();
        SerializeList(serializer, ref supplyLoadData);
    }

    private static void SerializeList<T, TValue>(BufferSerializer<T> serializer, ref List<TValue> list)
        where T : IReaderWriter where TValue : INetworkSerializable, new()
    {
        int count = serializer.IsReader ? 0 : list?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new List<TValue>(count);
        }

        for (int i = 0; i < count; i++)
        {
            TValue value = serializer.IsReader ? new TValue() : list[i];
            value.NetworkSerialize(serializer);

            if (serializer.IsReader)
            {
                list.Add(value);
            }
        }
    }

    public bool Equals(Test other)
    {
        if (other == null) return false;
        return gold == other.gold;
    }

    public override bool Equals(object obj)
    {
        return obj is Test other && Equals(other);
    }

    public override int GetHashCode()
    {
        return gold.GetHashCode();
    }
}
public class StartingResources : INetworkSerializable, IEquatable<StartingResources>
{
    // units and unitLoadData are excluded from serialization as units are spawned by the host
    public List<UnitController> units;
    public List<UnitLoadData> unitLoadData;

    public List<FortLoadData> fortLoadData;
    public List<CityLoadData> cityLoadData;
    public List<SupplyLoadData> supplyLoadData;
    public TreeLoadData treeLoadData;
    public int gold;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gold);

        /*
        treeLoadData ??= new TreeLoadData(new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>(), (0, ""));
        treeLoadData.NetworkSerialize(serializer);

        fortLoadData ??= new List<FortLoadData>();
        SerializeList(serializer, ref fortLoadData);

        cityLoadData ??= new List<CityLoadData>();
        SerializeList(serializer, ref cityLoadData);

        supplyLoadData ??= new List<SupplyLoadData>();
        SerializeList(serializer, ref supplyLoadData);
        */
    }

    private static void SerializeList<T, TValue>(BufferSerializer<T> serializer, ref List<TValue> list)
        where T : IReaderWriter where TValue : INetworkSerializable, new()
    {
        int count = serializer.IsReader ? 0 : list?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new List<TValue>(count);
        }

        for (int i = 0; i < count; i++)
        {
            TValue value = serializer.IsReader ? new TValue() : list[i];
            value.NetworkSerialize(serializer);

            if (serializer.IsReader)
            {
                list.Add(value);
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