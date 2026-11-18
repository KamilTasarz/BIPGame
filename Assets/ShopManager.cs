using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
	public static ShopManager instance;

	[Header("Player Stats")]
	public int playerGold = 100;
	public TextMeshProUGUI moneyText;

	[Header("Shop UI")]
	public GameObject shopPanel;
	public GameObject notEnoughMoneyPanel;
	public Button[] itemButtons;
	public int[] itemPrices;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		UpdateMoneyUI();

		// Ensure UI panels are hidden when the game starts
		if (notEnoughMoneyPanel != null)
		{
			notEnoughMoneyPanel.SetActive(false);
		}
		if (shopPanel != null)
		{
			shopPanel.SetActive(false);
		}
	}

	public void ChangeMoney(int amount)
	{
		playerGold += amount;
		UpdateMoneyUI();
	}

	public void BuyItem(int itemIndex)
	{
		int price = itemPrices[itemIndex];

		if (playerGold >= price)
		{
			ChangeMoney(-price);
			itemButtons[itemIndex].interactable = false;
			Debug.Log("Purchased item slot: " + itemIndex);
		} else
		{
			StopAllCoroutines();
			StartCoroutine(ShowNotEnoughMoneyPopup());
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
	}

	// --- New Shop Flow Methods ---

	public void OpenShop()
	{
		if (shopPanel != null)
		{
			shopPanel.SetActive(true);
		}
	}

	public void CloseShop()
	{
		if (shopPanel != null)
		{
			shopPanel.SetActive(false);
		}

		// Tell the GameManager to proceed to the next wave after closing
		GameManager.Instance.StartNextWave();
	}
}