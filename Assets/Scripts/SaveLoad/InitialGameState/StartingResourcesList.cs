using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartingResourcesList : INetworkSerializable, IEquatable<StartingResourcesList>
{
    public List<StartingResources> list;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        SerializerUtility.SerializeList(serializer, ref list);
    }

    public bool Equals(StartingResourcesList other)
    {
        if (other == null) return false;
        if (other.list == null) return false;
        if (other.list.Count != list.Count) return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].Equals(other.list[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is StartingResourcesList other && Equals(other);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var startingResources in list)
        {
            hash += startingResources.GetHashCode();
        }

        return hash;
    }
}
