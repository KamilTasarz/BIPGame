using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
//using System.Diagnostics;
using Unity.VisualScripting.FullSerializer;

public class Modifier
{
	public string name;
	public float currentValue;
	public int currentTier;
	public int tierCount;
	public float[] tierValues;
	public int baseCost;

	public Modifier(string name, int tierCount, float[] tierValues, int baseCost)
	{
		this.name = name;
		this.tierCount = tierCount;
		this.currentTier = 0;
		this.tierValues = new float[tierCount];
		for (int i = 0; i < tierCount; i++)
		{
			this.tierValues[i] = tierValues[i];
		}
		this.currentValue = tierValues[0];
		this.baseCost = baseCost;
	}

	public int GetNextTierCost()
	{
		return baseCost * (currentTier + 1);
	}

	public void UpgradeToNextTier()
	{
		Debug.Log("was" + currentTier + " " + currentValue);
		if (currentTier < tierCount - 1)
		{
			currentTier++;
			currentValue = tierValues[currentTier];
		}
		Debug.Log("is" + currentTier + " " + currentValue);
	}
}

public class Upgrade
{
	public Modifier modifier;
	public int currentTier;

	public Upgrade(Modifier appliedModifier)
	{
		modifier = appliedModifier;
		currentTier = modifier.currentTier;
	}
}

public class UpgradesManager : MonoBehaviour
{
	public List<Modifier> modifiers;

	public static UpgradesManager Instance { get; private set; }

	void Awake()
	{
		if (Instance == null)
		{

			Instance = this;
			modifiers = new List<Modifier>();
			float[] tier1 = { 1.0f, 1.05f, 1.1f, 1.2f, 1.35f, 1.5f, 1.75f, 2.0f, 3.0f, 5.0f };
			float[] tier2 = { 1.0f, 1.2f, 1.5f, 1.6f, 1.8f, 2.0f };
			float[] tier3 = { 1.0f, 2.0f, 3.0f};
			float[] tier4 = { 1.0f, 0.95f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.3f, 0.1f, 0.01f };

			modifiers.Add(new Modifier("Money multiplier", 10, tier1, 25));
			modifiers.Add(new Modifier("Vortex strength", 6, tier2, 25));
			modifiers.Add(new Modifier("Ship durability", 3, tier3, 25));
			modifiers.Add(new Modifier("Ship spawn rate", 10, tier4, 25));

		}
		else
		{
			Destroy(gameObject);
			return;
		}

		
	}

	public float ProvideCurrentModifierValue(Modifier query_modifier)
	{
		return query_modifier.currentValue;
	}

	public Upgrade GenerateUpgradeCard(Modifier query_modifier)
	{
		return new Upgrade(query_modifier);
	}
}