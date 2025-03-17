using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public bool autoStartNextWave = true;
    public float timeBetweenWaves;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;
    public bool useRandomSpawnPoint = true;
    public bool dontSpawnWhenPlayerNearby = true;
    public float playerCheckRadius = 5f;

    [Header("Events")]
    public UnityEvent onWaveStart;
    public UnityEvent onWaveEnd;
    public UnityEvent onAllWavesCompleted;
    public UnityEvent<int, int> onWaveProgress;

    public GameObject enemyPrefab;
    private int enemiesRemaining;
    private int enemiesSpawned;
    private bool isSpawning = false;
    private Transform playerTransform;
    public float timeBetweenSpawns;
    public int totalEnemies;
    private int enemiesPerWave = 5;
    public int totalWaves;
    public static int wavesRemaining;
    public static int currentWave;
    private bool waveEnded = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
            
        currentWave = 1;

        StartWave(1);
    }

    void Update()
    {
        if (!isSpawning && enemiesRemaining <= 0 && currentWave < totalWaves && !waveEnded)
        {
            EndWave();
            waveEnded = true;
        }
    }

    void StartWave(int waveIndex)
    {
        if (waveIndex >= totalWaves)
        {
            onAllWavesCompleted?.Invoke();
            return;
        }

        waveEnded = false;

        currentWave = waveIndex;
        enemiesRemaining = enemiesPerWave;
        enemiesSpawned = 0;

        onWaveStart?.Invoke();
        StartCoroutine(SpawnWave());
    }

    void EndWave()
    {
        onWaveEnd?.Invoke();

        if (currentWave + 1 < totalWaves && autoStartNextWave)
        {
            StartCoroutine(StartNextWaveAfterDelay());
        }
        else if (currentWave + 1 >= totalWaves)
        {
            onAllWavesCompleted?.Invoke();
        }
    }

    IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartWave(currentWave + 1);
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;

        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy(enemyPrefab);
            enemiesSpawned++;
            onWaveProgress?.Invoke(enemiesSpawned, enemiesPerWave);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        isSpawning = false;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned");
            return;
        }

        Transform spawnPoint = useRandomSpawnPoint ? spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)] : spawnPoints[enemiesSpawned % spawnPoints.Count];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        LifeController enemyLife = enemy.GetComponent<LifeController>();
        if (enemyLife != null)
        {
            enemyLife.onDeath.AddListener(OnEnemyDeath);
        }
    }

    void OnEnemyDeath()
    {
        enemiesRemaining--;
        Debug.Log($"Enemy defeated! Remaining: {enemiesRemaining}");
    }

    public void StartNextWave()
    {
        if (isSpawning || enemiesRemaining > 0)
        {
            return;
        }

        if (currentWave + 1 < totalWaves)
        {
            StartWave(currentWave + 1);
        }
    }

    public void RestartCurrentWave()
    {
        StartWave(currentWave);
    }

    public int GetCurrentWaveIndex()
    {
        return currentWave;
    }

    public int GetRemainingEnemies()
    {
        return enemiesRemaining;
    }

    public float GetWaveProgress()
    {
        if (wavesRemaining == 0) return 0;
        if (totalEnemies == 0) return 1;
        return (float)enemiesSpawned / totalEnemies;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }

        if (dontSpawnWhenPlayerNearby && playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, playerCheckRadius);
                }
            }
        }
    }
}