using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Port : MonoBehaviour
{
	[Header("Production & Demand")]
	public ResourceType producedResource;
	public ResourceType mainDesiredResource;
	public List<ResourceType> secondaryDesiredResources = new List<ResourceType>();
	public Transform boatSpawnPoint;

	[Header("Demand UI Icons")]
	public Image mainDesiredIcon;
	public Image[] secondaryDesiredIcons;

	private bool isSpawningActive = false;
	private bool hasWaitingBoat = false;
	private float spawnTimer = 0f;

	private void Start()
	{
		UpdateDemandUI();
	}

	private void Update()
	{
		if (!isSpawningActive || hasWaitingBoat) return;

		spawnTimer -= Time.deltaTime;
		if (spawnTimer <= 0f)
		{
			SpawnBoat();
		}
	}

	public void UpdateDemandUI()
	{
		if (GameManager.Instance == null) return;

		if (mainDesiredIcon != null)
		{
			mainDesiredIcon.sprite = GameManager.Instance.GetResourceIcon(mainDesiredResource);
			mainDesiredIcon.enabled = mainDesiredIcon.sprite != null;
		}

		if (secondaryDesiredIcons == null) return;

		for (int i = 0; i < secondaryDesiredIcons.Length; i++)
		{
			if (secondaryDesiredIcons[i] == null) continue;

			if (i < secondaryDesiredResources.Count)
			{
				secondaryDesiredIcons[i].sprite = GameManager.Instance.GetResourceIcon(secondaryDesiredResources[i]);
				secondaryDesiredIcons[i].gameObject.SetActive(secondaryDesiredIcons[i].sprite != null);
			} else
			{
				secondaryDesiredIcons[i].gameObject.SetActive(false);
			}
		}
	}

	public int CalculateCargoValue(List<ResourceType> cargo)
	{
		int totalValue = 0;

		foreach (ResourceType resource in cargo)
		{
			int basePrice = GameManager.Instance.GetResourceBasePrice(resource);
			float rate;

			if (resource == mainDesiredResource)
			{
				rate = 1.0f;
			} else if (secondaryDesiredResources.Contains(resource))
			{
				rate = 0.5f;
			} else
			{
				rate = 0.1f;
			}

			totalValue += Mathf.Max(1, Mathf.RoundToInt(basePrice * rate));
		}

		return totalValue;
	}

	public void StartWaveSpawning(float initialDelay)
	{
		isSpawningActive = true;
		hasWaitingBoat = false;
		spawnTimer = initialDelay;
	}

	public void StopWaveSpawning()
	{
		isSpawningActive = false;
		hasWaitingBoat = false;
	}

	public void OnBoatDeparted()
	{
		if (!isSpawningActive) return;

		hasWaitingBoat = false;
		spawnTimer = GameManager.Instance.GetNextSpawnDelay();
	}

	private void SpawnBoat()
	{
		GameManager.Instance.SpawnBoatAtPort(this);
		hasWaitingBoat = true;
	}
}