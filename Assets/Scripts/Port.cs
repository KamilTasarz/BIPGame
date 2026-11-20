using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Port : MonoBehaviour
{
	[Header("Production & Demand")]
	public ResourceType producedResource;
	public List<ResourceType> desiredResources = new List<ResourceType>();
	public Transform boatSpawnPoint;

	[Header("Demand UI Icons (Przypisz 2 ikonki)")]
	public Image[] desiredIcons;

	public ParticleSystem ps;

	private bool isSpawningActive = false;
	private bool hasWaitingBoat = false;
	private float spawnTimer = 0f;

	private void Start()
	{
		RollNewDemand();
		if (ps != null)
		{
			ps.Stop();	
		}
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

	public void RollNewDemand()
	{
		desiredResources.Clear();

		ResourceType[] allTypes = (ResourceType[])System.Enum.GetValues(typeof(ResourceType));
		List<ResourceType> pool = new List<ResourceType>();

		foreach (ResourceType type in allTypes)
		{
			// Port nie losuje jako zapotrzebowania tego, co sam produkuje
			if (type != producedResource)
			{
				pool.Add(type);
			}
		}

		int count = Mathf.Min(2, pool.Count);
		for (int i = 0; i < count; i++)
		{
			int randomIndex = Random.Range(0, pool.Count);
			desiredResources.Add(pool[randomIndex]);
			pool.RemoveAt(randomIndex);
		}

		UpdateDemandUI();
	}

	public void UpdateDemandUI()
	{
		if (GameManager.Instance == null || desiredIcons == null) return;

		for (int i = 0; i < desiredIcons.Length; i++)
		{
			if (desiredIcons[i] == null) continue;

			if (i < desiredResources.Count)
			{
				desiredIcons[i].sprite = GameManager.Instance.GetResourceIcon(desiredResources[i]);
				desiredIcons[i].gameObject.SetActive(desiredIcons[i].sprite != null);
			} else
			{
				desiredIcons[i].gameObject.SetActive(false);
			}
		}
	}

	public int CalculateCargoValue(List<ResourceType> cargo)
	{
		int totalValue = 0;

		foreach (ResourceType resource in cargo)
		{
			int basePrice = GameManager.Instance.GetResourceBasePrice(resource);
			int multiplier = desiredResources.Contains(resource) ? 2 : 1;

			totalValue += basePrice * multiplier;
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

	public void PlayParts()
	{
		if (ps != null) {
			ps.Play();
		}
	}
}