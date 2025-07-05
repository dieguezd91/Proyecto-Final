using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemiesSpawner : MonoBehaviour
{
    [SerializeField] private bool useContinuousHordeSystem = true;
    [SerializeField] private int baseEnemiesPerNight;
    [SerializeField] private int enemiesPerNightIncrement;
    [SerializeField] private float difficultyScalingFactor = 1.2f;

    [Header("Configuración de Spawn Individual")]
    [SerializeField] private float baseSpawnInterval; // Intervalo base entre spawns
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float spawnIntervalDecreasePerDay = 0.1f; // Reduccion de intervalo por dia

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;
    public bool useRandomSpawnPoint = true;
    public bool dontSpawnWhenPlayerNearby = true;
    public float playerCheckRadius = 5f;

    [Header("Enemy Types")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Header("Events")]
    public UnityEvent onHordeStart;
    public UnityEvent onHordeEnd;
    public UnityEvent onEnemySpawned;
    public UnityEvent<int, int> onHordeProgress;
    
    private int totalEnemiesKilled;
    private int currentEnemiesAlive;
    private int totalEnemiesToKill;
    private int totalEnemiesSpawned;
    private bool isSpawning = false;
    private Transform playerTransform;
    private GameState lastGameState = GameState.None;
    private Coroutine continuousSpawnCoroutine;
    private float currentHordeTime;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool hordeCompleted = false;
    private float currentSpawnInterval;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        ResetHordeCounters();
    }

    void Update()
    {
        if (GameManager.Instance.currentGameState == GameState.Night && lastGameState != GameState.Night)
        {
            StartContinuousHorde();
        }

        lastGameState = GameManager.Instance.currentGameState;

        int trulyAliveEnemies = 0;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                trulyAliveEnemies++;
        }

        if (GameManager.Instance.currentGameState == GameState.Night &&
            totalEnemiesKilled >= totalEnemiesToKill &&
            trulyAliveEnemies <= 0 &&
            !hordeCompleted)
        {
            hordeCompleted = true;
            StopContinuousHorde(true);
        }
    }


    public void StartContinuousHorde()
    {
        if (GameManager.Instance.currentGameState != GameState.Night)
        {
            return;
        }

        ResetHordeCounters();

        int currentDay = GameManager.Instance.GetCurrentDay();
        totalEnemiesToKill = baseEnemiesPerNight + ((currentDay - 1) * enemiesPerNightIncrement);

        currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentDay - 1) * spawnIntervalDecreasePerDay
        );

        onHordeStart?.Invoke();

        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
        }

        continuousSpawnCoroutine = StartCoroutine(SpawnEnemiesIndividually());
    }

    IEnumerator SpawnEnemiesIndividually()
    {
        isSpawning = true;
        currentHordeTime = 0f;
        hordeCompleted = false;

        yield return new WaitForSeconds(1f);

        while (GameManager.Instance.currentGameState == GameState.Night && totalEnemiesSpawned < totalEnemiesToKill)
        {
            currentHordeTime += currentSpawnInterval;

            float adjustedInterval = AdjustSpawnIntervalByTime();

            SpawnEnemy();
            totalEnemiesSpawned++;
            onEnemySpawned?.Invoke();
            onHordeProgress?.Invoke(totalEnemiesKilled, totalEnemiesToKill);

            yield return new WaitForSeconds(adjustedInterval);
        }

        isSpawning = false;
        continuousSpawnCoroutine = null;

        Debug.Log($"Total de enemigos generados: {totalEnemiesSpawned}");

        if (currentEnemiesAlive <= 0 && !hordeCompleted)
        {
            hordeCompleted = true;
            StopContinuousHorde(true);
        }
    }

    private float AdjustSpawnIntervalByTime()
    {
        if (currentHordeTime > 60f)
        {
            return currentSpawnInterval * 0.75f;
        }
        else if (currentHordeTime > 30f)
        {
            return currentSpawnInterval * 0.9f;
        }

        return currentSpawnInterval;
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Count == 0 || enemyPrefabs.Count == 0)
        {
            Debug.Log("No hay puntos de spawn o enemigos asignados");
            return;
        }

        Transform spawnPoint;

        if (useRandomSpawnPoint)
        {
            if (dontSpawnWhenPlayerNearby && playerTransform != null)
            {
                List<Transform> validSpawnPoints = new List<Transform>();

                foreach (Transform point in spawnPoints)
                {
                    if (Vector3.Distance(point.position, playerTransform.position) > playerCheckRadius)
                    {
                        validSpawnPoints.Add(point);
                    }
                }

                spawnPoint = (validSpawnPoints.Count > 0)
                    ? validSpawnPoints[Random.Range(0, validSpawnPoints.Count)]
                    : spawnPoints[Random.Range(0, spawnPoints.Count)];
            }
            else
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            }
        }
        else
        {
            spawnPoint = spawnPoints[totalEnemiesSpawned % spawnPoints.Count];
        }

        GameObject selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        GameObject enemy = Instantiate(selectedEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.transform.SetParent(this.transform);
        currentEnemiesAlive++;
        activeEnemies.Add(enemy);

        LifeController enemyLife = enemy.GetComponent<LifeController>();
        if (enemyLife != null)
        {
            enemyLife.onDeath.AddListener(() => OnEnemyDeath(enemy));
        }
    }

    public void ResetHordeCounters()
    {
        totalEnemiesKilled = 0;
        totalEnemiesSpawned = 0;
        currentEnemiesAlive = 0;
        totalEnemiesToKill = baseEnemiesPerNight;
        hordeCompleted = false;
        currentHordeTime = 0f;

        activeEnemies.Clear();

        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
            continuousSpawnCoroutine = null;
        }

        isSpawning = false;
    }

    void OnEnemyDeath(GameObject enemy)
    {
        totalEnemiesKilled++;
        currentEnemiesAlive--;

        activeEnemies.RemoveAll(e => e == null);

        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        onHordeProgress?.Invoke(totalEnemiesKilled, totalEnemiesToKill);

        int trulyAliveEnemies = 0;
        foreach (var e in activeEnemies)
        {
            if (e != null)
                trulyAliveEnemies++;
        }

        if (totalEnemiesKilled >= totalEnemiesToKill && trulyAliveEnemies <= 0 &&
            GameManager.Instance.currentGameState == GameState.Night && !hordeCompleted)
        {
            hordeCompleted = true;
            StopContinuousHorde(true);
        }
    }



    public void StopContinuousHorde(bool completed)
    {
        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
            continuousSpawnCoroutine = null;
        }

        isSpawning = false;

        if (completed)
        {
            onHordeEnd?.Invoke();

            GameManager.Instance.currentGameState = GameState.Digging;
        }
    }

    public int GetRemainingEnemies()
    {
        return totalEnemiesToKill - totalEnemiesKilled;
    }

    public int GetTotalTargetEnemies()
    {
        return totalEnemiesToKill;
    }

    public int GetEnemiesKilled()
    {
        return totalEnemiesKilled;
    }

    public int GetCurrentEnemiesAlive()
    {
        return currentEnemiesAlive;
    }

    public float GetHordeProgress()
    {
        if (totalEnemiesToKill <= 0) return 0;
        return (float)totalEnemiesKilled / totalEnemiesToKill;
    }

    public string GetCurrentDifficultyInfo()
    {
        float adjustedInterval = AdjustSpawnIntervalByTime();
        return $"Spawn cada {adjustedInterval:F1}s";
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

    public void EndNight()
    {
    
        if (GameManager.Instance.currentGameState != GameState.Night)
        {
            return;
        }

        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
            continuousSpawnCoroutine = null;
        }

        List<GameObject> enemiesCopy = new List<GameObject>(activeEnemies);
        foreach (GameObject enemy in enemiesCopy)
        {
            if (enemy != null)
            {
                LifeController enemyLife = enemy.GetComponent<LifeController>();
                if (enemyLife != null)
                {
                    enemyLife.Kill();
                }
                else
                {
                    OnEnemyDeath(enemy);
                }
            }
        }

        totalEnemiesKilled = totalEnemiesToKill;
        currentEnemiesAlive = 0;
        activeEnemies.Clear();

        hordeCompleted = true;
        StopContinuousHorde(true);
    }
    
}