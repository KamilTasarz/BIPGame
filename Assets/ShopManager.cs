using System.Collections; // Required for the timed popup
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
    public GameObject notEnoughMoneyPanel; // Your new warning popup
    public Button[] itemButtons;
    public int[] itemPrices;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateMoneyUI();
        
        // Ensure the warning box is hidden when the game starts
        if (notEnoughMoneyPanel != null)
        {
            notEnoughMoneyPanel.SetActive(false);
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
        }
        else
        {
            // Trigger the warning popup
            StopAllCoroutines(); // Resets the timer if the player clicks rapidly
            StartCoroutine(ShowNotEnoughMoneyPopup());
        }
    }

    IEnumerator ShowNotEnoughMoneyPopup()
    {
        if (notEnoughMoneyPanel != null)
        {
            notEnoughMoneyPanel.SetActive(true);       // Turn the box ON
            yield return new WaitForSeconds(2f);       // Wait for 2 seconds
            notEnoughMoneyPanel.SetActive(false);      // Turn the box OFF
        }
    }

    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Gold : " + playerGold + "$";
        }
    }
}