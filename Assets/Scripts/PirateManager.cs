using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public class PirateManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject piratePrefab;
    public List<GameObject> pirates;

    private float timer = 0.0f;
    public static PirateManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (TutorialManager.Instance.isTutorial) return;
        if (!GameManager.Instance.waveLasting) return;
        if (timer <= 0.0f && pirates.Count < GameManager.Instance.waves[GameManager.Instance.currentWaveIndex].maxPiratesNumber)
        {
            timer = GameManager.Instance.waves[GameManager.Instance.currentWaveIndex].piratesSpawnTime;
            int index = Random.Range(0, spawnPoints.Length);
            GameObject pirate = Instantiate(piratePrefab, spawnPoints[index].position, spawnPoints[index].rotation, transform);
            pirates.Add(pirate);
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    public void ClearPirates()
    {
        timer = 0.0f;
        foreach (var pirate in pirates)
        {
            Destroy(pirate);
        }
        pirates.Clear();
    }
}
