using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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
		if (currentTier < tierCount - 1)
		{
			currentTier++;
			currentValue = tierValues[currentTier];
		}
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
			float[] tier2 = { 1.0f, 1.05f, 1.1f, 1.2f, 1.35f, 1.5f, 1.75f, 2.0f, 3.0f, 5.0f };
			float[] tier3 = { 1.0f, 1.05f, 1.1f, 1.2f, 1.35f, 1.5f, 1.75f, 2.0f, 3.0f, 5.0f };
			float[] tier4 = { 1.0f, 1.05f, 1.1f, 1.2f, 1.35f, 1.5f, 1.75f, 2.0f, 3.0f, 5.0f };
			modifiers.Add(new Modifier("money_mult", 10, tier1, 25));
			modifiers.Add(new Modifier("vortex_pull", 10, tier2, 25));
			modifiers.Add(new Modifier("hull_str", 10, tier3, 25));
			modifiers.Add(new Modifier("placeholder", 10, tier4, 25));

		}
		else
		{
			Destroy(gameObject);
			return;
		}

		
	}

	float ProvideCurrentModifierValue(Modifier query_modifier)
	{
		return query_modifier.currentValue;
	}

	Upgrade GenerateUpgradeCard(Modifier query_modifier)
	{
		return new Upgrade(query_modifier);
	}
}