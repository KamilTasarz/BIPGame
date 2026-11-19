using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum ResourceType
{
	FISH,
	COAL,
	AMBER
}

[Serializable]
public class Wave
{
	public string waveName = "New Wave";
	public int waveNumber;
	public float totalTime = 60f;
	public int requiredGold = 100;
	public int maxPiratesNumber = 3;
	public float piratesSpawnTime = 10.0f;
}

public class GameManager : MonoBehaviour
{
	public bool waveLasting = false;

	public int currentWaveIndex = 0;
	private float currentWaveTimeRemaining = 0f;
	private int currentWaveGold = 0;

	[Header("UI Elements")]
	public TextMeshProUGUI waveNumberCounter;
	public TextMeshProUGUI countdownTimer;
	public TextMeshProUGUI goldGoalText;
	public GameObject gameOverPanel;

	[Header("Spawn Rate Settings")]
	public float baseMinSpawnDelay = 10f;
	public float baseMaxSpawnDelay = 15f;
	public float firstBoatDelay = 2f;
	public float initialStaggerDelay = 3f;
	public bool randomizeStartOrder = true;

	[Header("Prefabs")]
	public GameObject boatPrefab;
	public Transform boatParent;

	[Header("Data")]
	public List<Wave> waves = new List<Wave>();
	public List<Port> ports = new List<Port>();

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
			if (waves[w].waveNumber == 0)
			{
				waves[w].waveNumber = w + 1;
			}
		}
	}

	private void Start()
	{
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}

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

				int minutes = Mathf.FloorToInt(currentWaveTimeRemaining / 60f);
				int seconds = Mathf.FloorToInt(currentWaveTimeRemaining % 60f);

				if (countdownTimer != null)
				{
					countdownTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
				}
			} else
			{
				currentWaveTimeRemaining = 0f;
				waveLasting = false;
				OnWaveComplete();
			}
		}
	}

	public void StartWave(int index)
	{
		if (index < 0 || index >= waves.Count) return;

		currentWaveIndex = index;
		currentWaveTimeRemaining = waves[index].totalTime;
		currentWaveGold = 0;
		waveLasting = true;

		if (waveNumberCounter != null)
		{
			waveNumberCounter.text = waves[index].waveNumber.ToString();
		}

		UpdateGoalUI();

		List<Port> activePorts = new List<Port>();
		foreach (Port port in ports)
		{
			if (port != null)
			{
				activePorts.Add(port);
			}
		}

		if (randomizeStartOrder)
		{
			for (int i = activePorts.Count - 1; i > 0; i--)
			{
				int randomIndex = UnityEngine.Random.Range(0, i + 1);
				(activePorts[i], activePorts[randomIndex]) = (activePorts[randomIndex], activePorts[i]);
			}
		}

		for (int i = 0; i < activePorts.Count; i++)
		{
			float delay = firstBoatDelay + (i * initialStaggerDelay);
			activePorts[i].StartWaveSpawning(delay);
		}

		Debug.Log($"Started: {waves[index].waveName} | Cel: {waves[index].requiredGold}$");
	}

	public float GetNextSpawnDelay()
	{
		// TO ADD POWERUP MULTIPLIER
		// float mult = UpgradesManager.Instance.GetSpawnDelayMultiplier();
		float mult = 1f;

		float minDelay = Mathf.Max(0f, baseMinSpawnDelay * mult);
		float maxDelay = Mathf.Max(minDelay, baseMaxSpawnDelay * mult);

		return UnityEngine.Random.Range(minDelay, maxDelay);
	}

	public void SpawnBoatAtPort(Port destination)
	{
		if (boatPrefab == null || destination.boatSpawnPoint == null)
		{
			Debug.LogWarning("Brak boatPrefab w GameManager lub boatSpawnPoint w porcie!");
			return;
		}

		GameObject newBoat = Instantiate(boatPrefab, Vector3.zero, destination.transform.rotation, boatParent);
		float x = destination.boatSpawnPoint.position.x;
		float y = 0f;
		float z = destination.boatSpawnPoint.position.z;
		newBoat.transform.position = new Vector3(x, y, z);

		BoatBehaviour boatBehaviour = newBoat.GetComponent<BoatBehaviour>();
		if (boatBehaviour != null)
		{
			boatBehaviour.carriedResource = destination.producedResource;
			boatBehaviour.originPort = destination;
		}
	}

	public void AddWaveEarnings(int amount)
	{
		if (!waveLasting) return;

		currentWaveGold += amount;
		UpdateGoalUI();
	}

	private void UpdateGoalUI()
	{
		if (goldGoalText != null && currentWaveIndex < waves.Count)
		{
			int goal = waves[currentWaveIndex].requiredGold;
			goldGoalText.text = $"Cel: {currentWaveGold} / {goal}$";
		}
	}

	private void StopAllPortsAndClearBoats()
	{
		foreach (Port port in ports)
		{
			if (port != null)
			{
				port.StopWaveSpawning();
			}
		}

		if (boatParent != null)
		{
			for (int i = boatParent.childCount - 1; i >= 0; i--)
			{
				Destroy(boatParent.GetChild(i).gameObject);
			}
		}
		PirateManager.Instance.ClearPirates();
	}

	private void OnWaveComplete()
	{
		StopAllPortsAndClearBoats();

		Wave finishedWave = waves[currentWaveIndex];

		if (currentWaveGold < finishedWave.requiredGold)
		{
			Debug.Log($"PRZEGRANA! Zebrano {currentWaveGold}$ z wymaganych {finishedWave.requiredGold}$.");
			if (countdownTimer != null) countdownTimer.text = "GAME OVER!";
			if (gameOverPanel != null) gameOverPanel.SetActive(true);
			return;
		}

		Debug.Log($"Wave {finishedWave.waveName} completed!");

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

	public void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

}