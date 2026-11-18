using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
	public ResourceType producedResource;
	public List<ResourceType> acceptedResources = new List<ResourceType>();
	public Transform boatSpawnPoint;

	private bool isSpawningActive = false;
	private bool hasWaitingBoat = false;
	private float spawnTimer = 0f;

	private void Update()
	{
		if (!isSpawningActive || hasWaitingBoat) return;

		spawnTimer -= Time.deltaTime;
		if (spawnTimer <= 0f)
		{
			SpawnBoat();
		}
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