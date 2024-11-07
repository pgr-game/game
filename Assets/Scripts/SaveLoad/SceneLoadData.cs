using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneLoadData : INetworkSerializable
{
	public SceneLoadData(int numberOfPlayers, Vector3[] playerPositions,
	Color32[] playerColors, string[] startingCityNames, int turnNumber,
	int activePlayerIndex, bool[] isComputer, bool isMultiplayer)
	{
		this.numberOfPlayers = numberOfPlayers;
		this.playerPositions = playerPositions;
		this.playerColors = playerColors;
		this.startingCityNames = startingCityNames;
		this.turnNumber = turnNumber;
		this.activePlayerIndex = activePlayerIndex;
		this.isComputer = isComputer;
		this.isMultiplayer = isMultiplayer;
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

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref numberOfPlayers);
		serializer.SerializeValue(ref playerPositions);
		serializer.SerializeValue(ref playerColors);
		serializer.SerializeValue(ref turnNumber);
		serializer.SerializeValue(ref activePlayerIndex);
		serializer.SerializeValue(ref isComputer);
		serializer.SerializeValue(ref isMultiplayer);

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
}
