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
	public Port spawnPort; // In the Inspector, you drag/drop or click the circle to select an existing Port

	public Resource() { }
}

[Serializable]
public class Wave
{
	public string waveName = "New Wave";

	public int waveNumber;
	public float totalTime;

	// The wave now directly controls what spawns and when
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

	public TextMeshProUGUI waveNumberCounter;
	public TextMeshProUGUI countdownTimer;

	public List<Wave> waves = new List<Wave>();

	// You can still keep a list of all ports here if needed for other logic
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

				// Dynamically read the resource type from the assigned port, fallback if unassigned
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

			// Simplified queue logic: Loop through the wave's own resource list
			foreach (Resource res in waves[index].resources)
			{
				// Only queue if a port is assigned and the time fits within the wave
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
		// Reads the type dynamically from the Port MonoBehaviour
		Debug.Log($"[{currentWaveElapsedTime:F1}s] Spawning {destination.producedResource} at Port: {destination.gameObject.name}!");
	}

	private void OnWaveComplete()
	{
		Debug.Log($"Wave {waves[currentWaveIndex].waveName} completed!");

		int nextIndex = currentWaveIndex + 1;
		if (nextIndex < waves.Count)
		{
			StartWave(nextIndex);
		} else
		{
			Debug.Log("All waves finished!");
			if (countdownTimer != null) countdownTimer.text = "VICTORY!";
		}
	}
}