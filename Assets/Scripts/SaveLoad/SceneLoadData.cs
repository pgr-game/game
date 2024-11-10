using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Linq;

public class SceneLoadData : INetworkSerializable, IEquatable<SceneLoadData>
{
	public SceneLoadData(int numberOfPlayers, Vector3[] playerPositions,
	Color32[] playerColors, string[] startingCityNames, int turnNumber,
	int activePlayerIndex, bool[] isComputer, bool isMultiplayer,
	string difficulty)
	{
		this.numberOfPlayers = numberOfPlayers;
		this.playerPositions = playerPositions;
		this.playerColors = playerColors;
		this.startingCityNames = startingCityNames;
		this.turnNumber = turnNumber;
		this.activePlayerIndex = activePlayerIndex;
		this.isComputer = isComputer;
		this.isMultiplayer = isMultiplayer;
		this.difficulty = difficulty;
	}

	public SceneLoadData() { }
	public int numberOfPlayers;
	public Vector3[] playerPositions;
	public Color32[] playerColors;
	public string[] startingCityNames;
	public int turnNumber;
	public int activePlayerIndex;
	public bool[] isComputer;
	public bool isMultiplayer;
	public string difficulty;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref numberOfPlayers);
		serializer.SerializeValue(ref playerPositions);
		serializer.SerializeValue(ref playerColors);
		serializer.SerializeValue(ref turnNumber);
		serializer.SerializeValue(ref activePlayerIndex);
		serializer.SerializeValue(ref isComputer);
		serializer.SerializeValue(ref isMultiplayer);
		serializer.SerializeValue(ref difficulty);

		// Serialize string array manually
		if (serializer.IsReader)
		{
			int length = 0;
			serializer.SerializeValue(ref length);
			startingCityNames = new string[length];
			for (int i = 0; i < length; i++)
			{
				string cityName = string.Empty;
				serializer.SerializeValue(ref cityName);
				startingCityNames[i] = cityName;
			}
		}
		else
		{
			int length = startingCityNames.Length;
			serializer.SerializeValue(ref length);
			for (int i = 0; i < length; i++)
			{
				string cityName = startingCityNames[i];
				serializer.SerializeValue(ref cityName);
			}
		}
	}

	public bool Equals(SceneLoadData other)
	{
		if (other == null) return false;
		return numberOfPlayers == other.numberOfPlayers &&
		       playerPositions.SequenceEqual(other.playerPositions) &&
		       playerColors.SequenceEqual(other.playerColors) &&
		       startingCityNames.SequenceEqual(other.startingCityNames) &&
		       turnNumber == other.turnNumber &&
		       activePlayerIndex == other.activePlayerIndex &&
		       isComputer.SequenceEqual(other.isComputer) &&
		       isMultiplayer == other.isMultiplayer &&
		       difficulty == other.difficulty;
	}

	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		if (obj.GetType() != GetType()) return false;
		return Equals(obj as SceneLoadData);
	}

	public override int GetHashCode()
	{
		int hash = numberOfPlayers.GetHashCode();
		hash = (hash * 397) ^ playerPositions.GetHashCode();
		hash = (hash * 397) ^ playerColors.GetHashCode();
		hash = (hash * 397) ^ startingCityNames.GetHashCode();
		hash = (hash * 397) ^ turnNumber.GetHashCode();
		hash = (hash * 397) ^ activePlayerIndex.GetHashCode();
		hash = (hash * 397) ^ isComputer.GetHashCode();
		hash = (hash * 397) ^ isMultiplayer.GetHashCode();
		hash = (hash * 397) ^ difficulty.GetHashCode();
		return hash;
	}
}
