using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class SerializerUtility
{
    public static void SerializeList<T, TValue>(BufferSerializer<T> serializer, ref List<TValue> list)
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

    public static void SerializeList<T>(BufferSerializer<T> serializer, ref List<string> list) where T : IReaderWriter
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

}
