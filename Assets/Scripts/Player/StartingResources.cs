using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartingResources
{
    public List<UnitController> units;
    public List<FortLoadData> fortLoadData;
    public List<UnitLoadData> unitLoadData;
    public List<CityLoadData> cityLoadData;
    public List<SupplyLoadData> supplyLoadData;
    public TreeLoadData treeLoadData;
    public int gold;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize gold
        serializer.SerializeValue(ref gold);


        // Serialize TreeLoadData
        if (serializer.IsReader)
        {
            treeLoadData = new TreeLoadData(new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>(), (0, ""));
        }
        treeLoadData ??= new TreeLoadData(new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>(), (0, ""));
        treeLoadData.NetworkSerialize(serializer);

        fortLoadData ??= new List<FortLoadData>();
        SerializeList(serializer, ref fortLoadData);

        unitLoadData ??= new List<UnitLoadData>();
        SerializeList(serializer, ref unitLoadData);

        cityLoadData ??= new List<CityLoadData>();
        SerializeList(serializer, ref cityLoadData);

        supplyLoadData ??= new List<SupplyLoadData>();
        SerializeList(serializer, ref supplyLoadData);

        // Serialize UnitController references
        if (serializer.IsReader)
        {
            units = new List<UnitController>();
        }
        else
        {
            // Serialize UnitController references (ID-based or as necessary for your system)
            // Custom serialization logic may be required for non-serializable components like UnitController.
        }
    }
    private static void SerializeList<T, TValue>(BufferSerializer<T> serializer, ref List<TValue> list) where T : IReaderWriter where TValue : INetworkSerializable
    {
        int count = serializer.IsReader ? 0 : list?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new List<TValue>(count);
        }

        for (int i = 0; i < count; i++)
        {
            TValue value = default;
            value.NetworkSerialize(serializer);

            if (serializer.IsReader)
            {
                list.Add(value);
            }
        }
    }
}