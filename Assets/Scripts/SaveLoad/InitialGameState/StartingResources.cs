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
        SerializerUtility.SerializeList(serializer, ref fortLoadData);
        SerializerUtility.SerializeList(serializer, ref cityLoadData);
        SerializerUtility.SerializeList(serializer, ref supplyLoadData);

        // Serialize treeLoadData
        treeLoadData ??= new TreeLoadData(
            new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(),
            (1, "node"));
        treeLoadData.NetworkSerialize(serializer);
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

    public StartingResources DeepCopy()
    {
        StartingResources newStartingResources = new StartingResources();
        newStartingResources.gold = this.gold;


        newStartingResources.fortLoadData = new List<FortLoadData>();
        if (this.fortLoadData != null)
        {
            for (int i = 0; i < this.fortLoadData.Count; i++)
            {
                if (this.fortLoadData[i].id == int.MaxValue)
                    continue;
                newStartingResources.fortLoadData.Add(
                    new FortLoadData(this.fortLoadData[i].position,
                        this.fortLoadData[i].hexPosition,
                        this.fortLoadData[i].id));
            }
        }


        newStartingResources.cityLoadData = new List<CityLoadData>();
        if (this.cityLoadData != null)
        {
            for (int i = 0; i < this.cityLoadData.Count; i++)
            {
                if (this.cityLoadData[i].level == int.MaxValue)
                    continue;
                newStartingResources.cityLoadData.Add(
                    new CityLoadData(this.cityLoadData[i].position,
                        this.cityLoadData[i].name,
                        this.cityLoadData[i].level,
                        this.cityLoadData[i].unitInProduction,
                        this.cityLoadData[i].unitInProductionTurnsLeft));
            }
        }


        newStartingResources.supplyLoadData = new List<SupplyLoadData>();
        if (this.supplyLoadData != null)
        {
            for (int i = 0; i < this.supplyLoadData.Count; i++)
            {
                if (this.supplyLoadData[i].startPosition.x == float.MaxValue)
                    continue;
                newStartingResources.supplyLoadData.Add(
                    new SupplyLoadData(this.supplyLoadData[i].startPosition,
                        this.supplyLoadData[i].endPosition));
            }
        }


        if (this.treeLoadData != null && this.treeLoadData.researchNode.Item1 == int.MaxValue)
        {
            newStartingResources.treeLoadData = null; //new TreeLoadData();
        }
        else if (this.treeLoadData != null && this.treeLoadData.powerEvolution != null)
        {
            newStartingResources.treeLoadData = new TreeLoadData();
            newStartingResources.treeLoadData.researchNode = this.treeLoadData.researchNode;
            newStartingResources.treeLoadData.powerEvolution =
                new Dictionary<int, List<string>>();
            newStartingResources.treeLoadData.strategyEvolution =
                new Dictionary<int, List<string>>();
            foreach (var keyValuePair in this.treeLoadData.powerEvolution)
            {
                var newList = new List<string>();
                foreach (var listItem in keyValuePair.Value)
                {
                    newList.Add(listItem);
                }
                newStartingResources.treeLoadData.powerEvolution.Add(keyValuePair.Key, newList);
            }

            foreach (var keyValuePair in this.treeLoadData.strategyEvolution)
            {
                var newList = new List<string>();
                foreach (var listItem in keyValuePair.Value)
                {
                    newList.Add(listItem);
                }
                newStartingResources.treeLoadData.strategyEvolution.Add(keyValuePair.Key, newList);
            }
        }

        return newStartingResources;
    }

    public static StartingResources NewTransferable()
    {
        StartingResources startingResources = new StartingResources();
        startingResources.fortLoadData = new List<FortLoadData>()
        {
            new FortLoadData(
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue),
                int.MaxValue)
        };
        startingResources.cityLoadData = new List<CityLoadData>()
        {
            new CityLoadData(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                "NULL",
                int.MaxValue,
                "NULL",
                int.MaxValue)
        };
        startingResources.supplyLoadData = new List<SupplyLoadData>()
        {
            new SupplyLoadData(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
        };
        startingResources.treeLoadData = new TreeLoadData(
            new Dictionary<int, List<string>>()
            {
                { int.MaxValue, new List<string>() { "a", "b", "c" } },
                { 0, new List<string>() { "d", "e", "f" } }
            },
            new Dictionary<int, List<string>>()
            {
                { int.MaxValue, new List<string>() { "a", "b", "c" } },
                { 0, new List<string>() { "d", "e", "f" } }
            },
            (int.MaxValue, "NULL"));

        return startingResources;
    }
}