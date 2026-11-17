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
	public ResourceType type;
	public float spawnTime;

	public Resource() { }

	public Resource(ResourceType type, float spawnTime)
	{
		this.type = type;
		this.spawnTime = spawnTime;
	}
}

[Serializable]
public class Wave
{
	public string waveName = "New Wave";

	public int waveNumber;
	public float totalTime;

	public List<Resource> resources;

	public Wave()
	{
		resources = new List<Resource>();
	}

	public Wave(string waveName, int waveNumber, float totalTime)
	{
		this.waveName = waveName;
		this.waveNumber = waveNumber;
		this.totalTime = totalTime;
		resources = new List<Resource>();
	}
}

public class GameManager : MonoBehaviour
{
	private bool waveStarted = false;
	private bool waveLasting = false;
	private bool waveEnded = false;

	private int currentWaveIndex = 0;
	private float currentWaveTime = 0f;

	public TextMeshProUGUI waveNumberCounter;
	public TextMeshProUGUI countdownTimer;

	public List<Wave> waves = new List<Wave>();

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

				string generatedId = $"{currentWave.waveNumber}_{res.type}_{(r + 1):D3}";

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
			if (currentWaveTime > 0f)
			{
				currentWaveTime -= Time.deltaTime;

				int minutes = Mathf.FloorToInt(currentWaveTime / 60f);
				int seconds = Mathf.FloorToInt(currentWaveTime % 60f);

				if (countdownTimer != null)
				{
					countdownTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
				}
			} else
			{
				currentWaveTime = 0f;
				waveLasting = false;
				waveEnded = true;
				OnWaveComplete();
			}
		}
	}

	public void StartWave(int index)
	{
		waveNumberCounter.text = waves[index].waveNumber.ToString();

		if (index >= 0 && index < waves.Count)
		{
			currentWaveIndex = index;
			currentWaveTime = waves[index].totalTime;

			waveStarted = true;
			waveLasting = true;
			waveEnded = false;

			Debug.Log($"Started: {waves[index].waveName} for {waves[index].totalTime} seconds.");
		}
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
