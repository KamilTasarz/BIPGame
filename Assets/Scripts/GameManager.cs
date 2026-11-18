using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public enum ResourceType
{
	FISH,
	COAL,
	AMBER
}

[Serializable]
public class Resource
{
	public string id;
	public float spawnTime;
	public Port spawnPort;

	public Resource() { }
}

[Serializable]
public class Wave
{
	public string waveName = "New Wave";
	public int waveNumber;
	public float totalTime;

	public List<Resource> resources = new List<Resource>();
}

public class GameManager : MonoBehaviour
{
	private struct PendingSpawn
	{
		public Port port;
		public float spawnTime;
	}

	private bool waveStarted = false;
	private bool waveLasting = false;
	private bool waveEnded = false;

	private int currentWaveIndex = 0;
	private float currentWaveTimeRemaining = 0f;
	private float currentWaveElapsedTime = 0f;

	[Header("UI Elements")]
	public TextMeshProUGUI waveNumberCounter;
	public TextMeshProUGUI countdownTimer;

	[Header("Prefabs")]
	public GameObject boatPrefab;

	[Header("Data")]
	public List<Wave> waves = new List<Wave>();
	public List<Port> ports = new List<Port>();

	private List<PendingSpawn> pendingSpawns = new List<PendingSpawn>();

	public static GameManager Instance { get; private set; }

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		} else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void OnValidate()
	{
		if (waves == null) return;

		for (int w = 0; w < waves.Count; w++)
		{
			Wave currentWave = waves[w];

			if (currentWave.waveNumber == 0)
			{
				currentWave.waveNumber = w + 1;
			}

			if (currentWave.resources == null) continue;

			for (int r = 0; r < currentWave.resources.Count; r++)
			{
				Resource res = currentWave.resources[r];

				string typeString = res.spawnPort != null ? res.spawnPort.producedResource.ToString() : "UNASSIGNED";
				string generatedId = $"{currentWave.waveNumber}_{typeString}_{(r + 1):D3}";

				if (res.id != generatedId)
				{
					res.id = generatedId;
				}
			}
		}
	}

	private void Start()
	{
		if (waves.Count > 0)
		{
			StartWave(0);
		}
	}

	private void Update()
	{
		if (waveLasting)
		{
			if (currentWaveTimeRemaining > 0f)
			{
				currentWaveTimeRemaining -= Time.deltaTime;
				currentWaveElapsedTime += Time.deltaTime;

				int minutes = Mathf.FloorToInt(currentWaveTimeRemaining / 60f);
				int seconds = Mathf.FloorToInt(currentWaveTimeRemaining % 60f);

				if (countdownTimer != null)
				{
					countdownTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
				}

				while (pendingSpawns.Count > 0 && currentWaveElapsedTime >= pendingSpawns[0].spawnTime)
				{
					SpawnResource(pendingSpawns[0].port);
					pendingSpawns.RemoveAt(0);
				}
			} else
			{
				currentWaveTimeRemaining = 0f;
				waveLasting = false;
				waveEnded = true;
				OnWaveComplete();
			}
		}
	}

	public void StartWave(int index)
	{
		if (waveNumberCounter != null)
		{
			waveNumberCounter.text = waves[index].waveNumber.ToString();
		}

		if (index >= 0 && index < waves.Count)
		{
			currentWaveIndex = index;
			currentWaveTimeRemaining = waves[index].totalTime;
			currentWaveElapsedTime = 0f;

			pendingSpawns.Clear();

			foreach (Resource res in waves[index].resources)
			{
				if (res.spawnPort != null && res.spawnTime <= waves[index].totalTime && res.spawnTime >= 0)
				{
					pendingSpawns.Add(new PendingSpawn { port = res.spawnPort, spawnTime = res.spawnTime });
				}
			}

			pendingSpawns.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

			waveStarted = true;
			waveLasting = true;
			waveEnded = false;

			Debug.Log($"Started: {waves[index].waveName} for {waves[index].totalTime} seconds.");
		}
	}

	public void SpawnResource(Port destination)
	{
		if (boatPrefab != null)
		{
			GameObject newBoat = Instantiate(boatPrefab, destination.boatSpawnPoint.transform.localPosition, destination.transform.rotation, destination.transform);
			//newBoat.gameObject.GetComponent<BoatBehaviour>().DisableRigidbody();
			float x = destination.boatSpawnPoint.transform.position.x;
			float y = 0.85f;
			float z = destination.boatSpawnPoint.transform.position.z;
			newBoat.transform.position = new Vector3(x, y, z);

			BoatBehaviour boatBehaviour = newBoat.GetComponent<BoatBehaviour>();
			if (boatBehaviour != null)
			{
				boatBehaviour.carriedResource = destination.producedResource;
				boatBehaviour.originPort = destination;
			}

			//newBoat.gameObject.GetComponent<BoatBehaviour>().EnableRigidbody();
			Debug.Log($"[{currentWaveElapsedTime:F1}s] Spawning boat with {destination.producedResource} at Port: {destination.gameObject.name}!");
		} else
		{
			Debug.LogWarning("Boat Prefab is not assigned in GameManager!");
		}
	}

	private void OnWaveComplete()
	{
		Debug.Log($"Wave {waves[currentWaveIndex].waveName} completed!");

		int nextIndex = currentWaveIndex + 1;
		if (nextIndex < waves.Count)
		{
			ShopManager.instance.OpenShop();
		} else
		{
			Debug.Log("All waves finished!");
			if (countdownTimer != null) countdownTimer.text = "VICTORY!";
		}
	}

	public void StartNextWave()
	{
		int nextIndex = currentWaveIndex + 1;
		if (nextIndex < waves.Count)
		{
			StartWave(nextIndex);
		}
	}
}