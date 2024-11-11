using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitLoadData : INetworkSerializable
{

	public UnitLoadData(Vector3 position, string unitType, int maxHealth, int currentHealth,
	int attack, int attackRange, int baseProductionCost, int turnsToProduce,
	int turnProduced, int level, int experience, float rangeLeft, bool attacked,
	Vector3 longPathClickPosition)
	{
		this.position = position;
		this.unitType = unitType;
		this.maxHealth = maxHealth;
		this.currentHealth = currentHealth;
		this.attack = attack;
		this.attackRange = attackRange;
		this.baseProductionCost = baseProductionCost;
		this.turnsToProduce = turnsToProduce;
		this.turnProduced = turnProduced;
		this.level = level;
		this.experience = experience;
		this.rangeLeft = rangeLeft;
		this.attacked = attacked;
		this.longPathClickPosition = longPathClickPosition;
	}
	public string unitType;
	public int maxHealth;
	public int currentHealth;
	public int attack;
	public int attackRange;
	public int baseProductionCost;
	public int turnsToProduce;
	public int turnProduced;
	public int level;
	public int experience;
	public float rangeLeft;
	public bool attacked;
	public Vector3 longPathClickPosition;

	private Vector3 _position;
	public Vector3 position
	{
		get => _position;
		private set => _position = value;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref _position);
		serializer.SerializeValue(ref unitType);
		serializer.SerializeValue(ref maxHealth);
		serializer.SerializeValue(ref currentHealth);
		serializer.SerializeValue(ref attack);
		serializer.SerializeValue(ref attackRange);
		serializer.SerializeValue(ref baseProductionCost);
		serializer.SerializeValue(ref turnsToProduce);
		serializer.SerializeValue(ref turnProduced);
		serializer.SerializeValue(ref level);
		serializer.SerializeValue(ref experience);
		serializer.SerializeValue(ref rangeLeft);
		serializer.SerializeValue(ref attacked);
		serializer.SerializeValue(ref longPathClickPosition);
	}
}
