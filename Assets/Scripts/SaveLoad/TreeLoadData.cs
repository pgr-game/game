using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TreeLoadData : INetworkSerializable
{
    public Dictionary<int, List<string>> powerEvolution;
    public Dictionary<int, List<string>> strategyEvolution;
    public (int, string) researchNode;

    public TreeLoadData(Dictionary<int, List<string>> powerEvolution, Dictionary<int, List<string>> strategyEvolution, (int, string) researchNode)
    {
        this.powerEvolution = powerEvolution;
        this.strategyEvolution = strategyEvolution;
        this.researchNode = researchNode;
    }

    public TreeLoadData() { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        SerializeDictionary(serializer, ref powerEvolution);
        SerializeDictionary(serializer, ref strategyEvolution);
        SerializeTuple(serializer, ref researchNode);
    }

    private void SerializeDictionary<T>(BufferSerializer<T> serializer, ref Dictionary<int, List<string>> dictionary) where T : IReaderWriter
    {
        int count = dictionary?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            dictionary = new Dictionary<int, List<string>>(count);
            for (int i = 0; i < count; i++)
            {
                int key = 0;
                List<string> value = new List<string>();
                serializer.SerializeValue(ref key);
                SerializeList(serializer, ref value);
                dictionary[key] = value;
            }
        }
        else
        {
            foreach (var kvp in dictionary)
            {
                int key = kvp.Key;
                List<string> value = kvp.Value;
                serializer.SerializeValue(ref key);
                SerializeList(serializer, ref value);
            }
        }
    }

    private void SerializeList<T>(BufferSerializer<T> serializer, ref List<string> list) where T : IReaderWriter
    {
        int count = list?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                string item = string.Empty;
                serializer.SerializeValue(ref item);
                list.Add(item);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                string item = list[i];
                serializer.SerializeValue(ref item);
            }
        }
    }

    private void SerializeTuple<T>(BufferSerializer<T> serializer, ref (int, string) tuple) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tuple.Item1);
        serializer.SerializeValue(ref tuple.Item2);
    }
}
