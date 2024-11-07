using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class PlayerData : NetworkBehaviour
{
	public int index;
	public Color32 color;
	public string name;

	public void Init(int index, Color32 color, string name)
	{
		this.index = index;
		this.color = color;
		this.name = name;
	}
}
