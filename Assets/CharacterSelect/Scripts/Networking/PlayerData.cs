using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class PlayerData : NetworkBehaviour
{
	[SerializeField]
	private NetworkVariable<int> networkIndex = new NetworkVariable<int>();
	[SerializeField]
	private NetworkVariable<Color32> networkColor = new NetworkVariable<Color32>();
	[SerializeField]
	private NetworkVariable<FixedString64Bytes> networkName = new NetworkVariable<FixedString64Bytes>();
	[SerializeField]
	private NetworkList<Color32> networkOtherPlayersColors = new NetworkList<Color32>();


	public int index
	{
		get => networkIndex.Value; 
		private set => networkIndex.Value = value;
	}
	public Color32 color
	{
		get => networkColor.Value; 
		private set => networkColor.Value = value;
	}
	public string name 
	{ 
		get => networkName.Value.ToString(); 
		private set => networkName.Value = value;
	}
	public NetworkList<Color32> otherPlayersColors
	{
		get => networkOtherPlayersColors;
	}

	public void Init(int index, Color32 color, string name, Color32[] otherPlayersColors)
	{
		this.index = index;
		this.color = color;
		this.name = name;
		foreach (var otherPlayersColor in otherPlayersColors)
		{
			networkOtherPlayersColors.Add(otherPlayersColor);
		}
	}
}
