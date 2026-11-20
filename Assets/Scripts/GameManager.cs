using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.Canvas.ShaderGraph;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum ResourceType
{
	GRAIN,
	TIMBER,
	TAR,
	IRON,
	FLAX,
	AMBER
}

[Serializable]
public struct ResourceData
{
	public ResourceType type;
	public Sprite icon;
	public int basePrice;
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
	public GameObject canvas;

	[Header("Spawn Rate Settings")]
	public float baseMinSpawnDelay = 10f;
	public float baseMaxSpawnDelay = 15f;
	public float firstBoatDelay = 2f;
	public float initialStaggerDelay = 3f;
	public bool randomizeStartOrder = true;

	[Header("Prefabs")]
	public GameObject boatPrefab;
	public Transform boatParent;

	[Header("Resources Database")]
	public List<ResourceData> resourceDatabase = new List<ResourceData>();

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
		if (TutorialManager.Instance.isTutorial)
		{
			canvas.SetActive(false);
			return;
		}
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

	public Sprite GetResourceIcon(ResourceType type)
	{
		foreach (ResourceData data in resourceDatabase)
		{
			if (data.type == type)
			{
				return data.icon;
			}
		}
		return null;
	}

	public int GetResourceBasePrice(ResourceType type)
	{
		foreach (ResourceData data in resourceDatabase)
		{
			if (data.type == type)
			{
				return data.basePrice;
			}
		}
		return 25;
	}

	public ResourceType GetRandomExtraResource(ResourceType excludeType)
	{
		ResourceType[] allTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
		List<ResourceType> candidates = new List<ResourceType>();

		foreach (ResourceType t in allTypes)
		{
			if (t != excludeType)
			{
				candidates.Add(t);
			}
		}

		return candidates[UnityEngine.Random.Range(0, candidates.Count)];
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
			string temp = waves[index].waveNumber.ToString();
			waveNumberCounter.text = "Wave " + temp;
		}

		canvas.SetActive(true);
		AudioManager.Instance.PlayMusic("ocean", AudioManager.Instance.musicSource);

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
		float mult = UpgradesManager.Instance.ProvideCurrentModifierValue(UpgradesManager.Instance.modifiers[3]);

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
			boatBehaviour.originPort = destination;
			List<ResourceType> cargo = new List<ResourceType>
			{
				destination.producedResource,
				GetRandomExtraResource(destination.producedResource)
			};

			boatBehaviour.SetCargo(cargo);
		}
	}

	public int AddWaveEarnings(int baseAmount)
	{
		if (!waveLasting) return 0;

		float mult = UpgradesManager.Instance.ProvideCurrentModifierValue(UpgradesManager.Instance.modifiers[0]);
		int earnings = Mathf.RoundToInt(baseAmount * mult);

		currentWaveGold += earnings;
		UpdateGoalUI();

		if (ShopManager.instance != null)
		{
			ShopManager.instance.ChangeMoney(earnings);
		}

		return earnings;
	}

	private void UpdateGoalUI()
	{
		if (goldGoalText != null && currentWaveIndex < waves.Count)
		{
			int goal = waves[currentWaveIndex].requiredGold;
			goldGoalText.text = $"Goal: {currentWaveGold} / {goal}$";
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

		if (PirateManager.Instance != null)
		{
			PirateManager.Instance.ClearPirates();
		}
	}

	private void OnWaveComplete()
	{
		StopAllPortsAndClearBoats();

		AudioManager.Instance.StopMusic(AudioManager.Instance.musicSource);
		AudioManager.Instance.PlaySFX("win");

		Wave finishedWave = waves[currentWaveIndex];

		canvas.SetActive(false);

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