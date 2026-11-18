using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
	public static ShopManager instance;

	[Header("Player Stats")]
	public int playerGold = 100;
	public TextMeshProUGUI moneyText;

	[Header("Shop UI Panels")]
	public GameObject shopPanel;
	public GameObject notEnoughMoneyPanel;

	[Header("Shop Slot Arrays (Assign 4 of each in Inspector)")]
	public Button[] itemButtons;
	public TextMeshProUGUI[] nameTexts;
	public TextMeshProUGUI[] valueTexts;
	public TextMeshProUGUI[] tierTexts;
	public TextMeshProUGUI[] costTexts;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		if (notEnoughMoneyPanel != null)
		{
			notEnoughMoneyPanel.SetActive(false);
		}
		if (shopPanel != null)
		{
			shopPanel.SetActive(false);
		}

		UpdateMoneyUI();
	}

	public void ChangeMoney(int amount)
	{
		playerGold += amount;
		UpdateMoneyUI();
	}

	public void BuyItem(int itemIndex)
	{
		if (UpgradesManager.Instance == null)
		{
			Debug.LogError("Brak aktywnego obiektu UpgradesManager na scenie!");
			return;
		}

		List<Modifier> mods = UpgradesManager.Instance.modifiers;

		// Failsafe if we click a button but there are no modifiers for that slot
		if (itemIndex >= mods.Count) return;

		Modifier mod = mods[itemIndex];

		// Prevent buying if maxed out
		if (mod.currentTier >= mod.tierCount - 1) return;

		int price = mod.GetNextTierCost();

		if (playerGold >= price)
		{
			ChangeMoney(-price);
			mod.UpgradeToNextTier();

			Debug.Log($"Purchased upgrade: {mod.name} to Tier {mod.currentTier + 1}");
			RefreshShopUI(); // Update UI immediately after purchase
		} else
		{
			StopAllCoroutines();
			StartCoroutine(ShowNotEnoughMoneyPopup());
		}
	}

	// Maps the modifier data directly to your UI text arrays
	private void RefreshShopUI()
	{
		if (UpgradesManager.Instance == null)
		{
			Debug.LogError("Brak aktywnego obiektu UpgradesManager na scenie!");
			return;
		}

		List<Modifier> mods = UpgradesManager.Instance.modifiers;

		for (int i = 0; i < itemButtons.Length; i++)
		{
			if (i < mods.Count)
			{
				Modifier mod = mods[i];
				nameTexts[i].text = mod.name;

				if (mod.currentTier < mod.tierCount - 1)
				{
					// Upgrade is available
					int cost = mod.GetNextTierCost();
					costTexts[i].text = "$" + cost;
					tierTexts[i].text = $"Tier {mod.currentTier + 1}/{mod.tierCount}";
					valueTexts[i].text = $"{mod.currentValue} -> {mod.tierValues[mod.currentTier + 1]}";

					// Button is clickable only if the player has enough gold
					itemButtons[i].interactable = (playerGold >= cost);
				} else
				{
					// Max Tier Reached
					costTexts[i].text = "MAX";
					tierTexts[i].text = $"Tier {mod.tierCount}/{mod.tierCount}";
					valueTexts[i].text = $"{mod.currentValue}";
					itemButtons[i].interactable = false;
				}
			} else
			{
				// Hide unused slots if there are fewer than 4 modifiers
				itemButtons[i].gameObject.SetActive(false);
			}
		}
	}

	IEnumerator ShowNotEnoughMoneyPopup()
	{
		if (notEnoughMoneyPanel != null)
		{
			notEnoughMoneyPanel.SetActive(true);
			yield return new WaitForSeconds(2f);
			notEnoughMoneyPanel.SetActive(false);
		}
	}

	public void UpdateMoneyUI()
	{
		if (moneyText != null)
		{
			moneyText.text = "Gold : " + playerGold + "$";
		}

		// If shop is currently open, receiving money dynamically updates button interactability
		if (shopPanel != null && shopPanel.activeInHierarchy)
		{
			RefreshShopUI();
		}
	}

	public void OpenShop()
	{
		if (shopPanel != null)
		{
			shopPanel.SetActive(true);
			RefreshShopUI(); // Populate values right as it opens
		}
	}

	public void CloseShop()
	{
		if (shopPanel != null)
		{
			shopPanel.SetActive(false);
		}
		GameManager.Instance.StartNextWave();
	}
}