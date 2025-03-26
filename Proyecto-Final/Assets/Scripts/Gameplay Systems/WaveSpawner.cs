using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public bool autoStartNextWave = true;
    [SerializeField] float timeBetweenWaves;

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
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] int totalEnemies;
    private int enemiesPerWave = 5;
    public int totalWaves;
    [SerializeField] static int wavesRemaining;
    public int currentWave;
    private bool waveEnded = false;
    private int nextWaveToStart = 1;
    private bool needsWaveReset = false;
    private GameState lastGameState = GameState.None;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        ResetWaveCounters();

        if (GameManager.Instance.currentGameState == GameState.Night)
        {
            StartWave(1);
        }
    }

    void Update()
    {
        if (GameManager.Instance.currentGameState == GameState.Night && lastGameState != GameState.Night)
        {
            Debug.Log("Cambio de dia a noche. Iniciando oleadas.");
            needsWaveReset = true;
        }

        lastGameState = GameManager.Instance.currentGameState;

        if (GameManager.Instance.currentGameState == GameState.Night && needsWaveReset)
        {
            ResetWaveCounters();
            StartWave(1);
            needsWaveReset = false;
        }

        if (GameManager.Instance.currentGameState == GameState.Night && !isSpawning && enemiesRemaining <= 0 && !waveEnded && currentWave > 0)
        {
            HandleWaveEnd();
        }
    }

    void StartWave(int waveIndex)
    {
        if (GameManager.Instance.currentGameState != GameState.Night)
        {
            Debug.Log($"No se puede iniciar oleada {waveIndex}: no es de noche");
            return;
        }

        if (waveIndex > totalWaves)
        {
            Debug.Log($"No hay mas oleadas disponibles");
            if (!isSpawning && enemiesRemaining <= 0)
            {
                GameManager.Instance.currentGameState = GameState.Day;
                onAllWavesCompleted?.Invoke();
            }
            return;
        }

        waveEnded = false;

        currentWave = waveIndex;
        enemiesRemaining = enemiesPerWave;
        enemiesSpawned = 0;

        nextWaveToStart = currentWave + 1;

        Debug.Log($"Iniciando oleada {currentWave} de {totalWaves}");
        onWaveStart?.Invoke();
        StartCoroutine(SpawnWave());
    }

    void HandleWaveEnd()
    {
        waveEnded = true;
        Debug.Log($"Finalizando oleada {currentWave}");
        onWaveEnd?.Invoke();

        int nextWave = currentWave + 1;

        if (nextWave <= totalWaves)
        {
            if (autoStartNextWave)
            {
                StartCoroutine(StartNextWaveAfterDelay(nextWave));
            }
        }
        else
        {
            Debug.Log($"Era la última oleada ({currentWave} de {totalWaves}). Cambiando a estado Día");
            GameManager.Instance.currentGameState = GameState.Day;
            onAllWavesCompleted?.Invoke();
        }
    }

    IEnumerator StartNextWaveAfterDelay(int nextWaveIndex)
    {
        Debug.Log($"Esperando {timeBetweenWaves} segundos para iniciar oleada {nextWaveIndex}");
        yield return new WaitForSeconds(timeBetweenWaves);

        if (GameManager.Instance.currentGameState == GameState.Night)
        {
            Debug.Log($"Iniciando siguiente oleada: {nextWaveIndex}");
            StartWave(nextWaveIndex);
        }
        else
        {
            Debug.Log("No se inicia la siguiente oleada porque ya no es de noche");
        }
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
        Debug.Log($"Generación de enemigos completada para oleada {currentWave}. Enemigos restantes: {enemiesRemaining}");
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.Log("No hay puntos de spawn asignados");
            return;
        }

        Transform spawnPoint = useRandomSpawnPoint
            ? spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)]
            : spawnPoints[enemiesSpawned % spawnPoints.Count];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        LifeController enemyLife = enemy.GetComponent<LifeController>();
        if (enemyLife != null)
        {
            enemyLife.onDeath.AddListener(OnEnemyDeath);
        }
    }

    public void ResetWaveCounters()
    {
        currentWave = 0;
        nextWaveToStart = 1;
        waveEnded = true;
        isSpawning = false;
        enemiesRemaining = 0;
    }

    void OnEnemyDeath()
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0 && !isSpawning && !waveEnded && GameManager.Instance.currentGameState == GameState.Night)
        {
            HandleWaveEnd();
        }
    }

    public void StartNextWave()
    {
        if (GameManager.Instance.currentGameState != GameState.Night || isSpawning || enemiesRemaining > 0)
        {
            Debug.Log("No se puede iniciar la siguiente oleada ahora");
            return;
        }

        int nextWave = currentWave + 1;

        if (nextWave <= totalWaves)
        {
            StartWave(nextWave);
        }
        else
        {
            if (!isSpawning && enemiesRemaining <= 0)
            {
                GameManager.Instance.currentGameState = GameState.Day;
                onAllWavesCompleted?.Invoke();
            }
        }
    }

    public int GetCurrentWaveIndex()
    {
        return currentWave;
    }

    public int GetRemainingEnemies()
    {
        return enemiesRemaining;
    }

    public int GetEnemiesPerWave()
    {
        return enemiesPerWave;
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