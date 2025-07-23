using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossNightManager : MonoBehaviour
{
    [Header("Boss Configuration")]
    [SerializeField] private List<GameObject> bossPrefabs = new List<GameObject>();
    [SerializeField] private int bossNightInterval = 5;
    [SerializeField] private float bossSpawnDelay = 3f;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Boss Night Settings")]
    [SerializeField] private bool useDynamicBossSelection = true;
    [SerializeField] private float bossAnnouncementDuration = 2f;

    [Header("Events")]
    public UnityEvent onBossNightStart;
    public UnityEvent onBossSpawned;
    public UnityEvent<GameObject> onBossDefeated;
    public UnityEvent onBossNightComplete;

    private EnemiesSpawner enemiesSpawner;
    private GameObject currentBoss;
    private bool isBossNight = false;
    private bool bossDefeated = false;

    private bool bossHasSpawnedThisNight = false;
    private int lastBossNightDay = -1;

    public bool IsBossNight => isBossNight;
    public bool IsBossActive => currentBoss != null;
    public GameObject CurrentBoss => currentBoss;

    private void Start()
    {
        enemiesSpawner = GetComponent<EnemiesSpawner>();

        if (enemiesSpawner == null)
        {
            enabled = false;
            return;
        }

        if (bossSpawnPoint == null && enemiesSpawner.spawnPoints.Count > 0)
        {
            bossSpawnPoint = enemiesSpawner.spawnPoints[0];
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onNewDay.AddListener(OnNewDay);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ForceEndBossNight();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            int currentDay = GameManager.Instance.GetCurrentDay();
            if (currentDay != lastBossNightDay)
            {
                isBossNight = true;
                bossHasSpawnedThisNight = false;
                lastBossNightDay = currentDay;
                StartBossNight();
            }
        }
    }

    private void OnNewDay(int dayCount)
    {
        bool shouldBeBossNight = IsBossNightDay(dayCount);

        if (shouldBeBossNight && dayCount != lastBossNightDay)
        {
            isBossNight = true;
            bossHasSpawnedThisNight = false;
            lastBossNightDay = dayCount;
        }
        else if (!shouldBeBossNight && dayCount != lastBossNightDay)
        {
            ResetBossNightState();
        }
    }

    public bool IsBossNightDay(int dayCount)
    {
        return dayCount > 0 && dayCount % bossNightInterval == 0;
    }

    public void StartBossNight()
    {
        if (bossHasSpawnedThisNight)
        {
            return;
        }

        int currentDay = GameManager.Instance.GetCurrentDay();
        bool isActuallyBossNight = IsBossNightDay(currentDay);

        if (!isActuallyBossNight)
        {
            return;
        }

        if (bossPrefabs.Count == 0)
        {
            return;
        }

        isBossNight = true;
        bossDefeated = false;
        onBossNightStart?.Invoke();

        StartCoroutine(BossNightSequence());
    }

    private IEnumerator BossNightSequence()
    {
        yield return new WaitForSeconds(bossAnnouncementDuration);

        if (!bossHasSpawnedThisNight)
        {
            SpawnBoss();
        }

        yield return new WaitUntil(() => bossDefeated);

        CompleteBossNight();
    }

    private void SpawnBoss()
    {
        if (bossHasSpawnedThisNight)
        {
            return;
        }

        if (bossSpawnPoint == null)
        {
            if (enemiesSpawner != null && enemiesSpawner.spawnPoints.Count > 0)
            {
                bossSpawnPoint = enemiesSpawner.spawnPoints[0];
            }
            else
            {
                return;
            }
        }

        GameObject selectedBoss = SelectBoss();

        if (selectedBoss == null)
        {
            return;
        }

        currentBoss = Instantiate(selectedBoss, bossSpawnPoint.position, bossSpawnPoint.rotation);
        currentBoss.transform.SetParent(transform);

        SetupBoss(currentBoss);

        bossHasSpawnedThisNight = true;

        onBossSpawned?.Invoke();
    }

    private GameObject SelectBoss()
    {
        if (bossPrefabs.Count == 0) return null;

        if (useDynamicBossSelection)
        {
            return SelectBossByLunarPhase();
        }
        else
        {
            int bossNightCount = GameManager.Instance.GetCurrentDay() / bossNightInterval;
            int bossIndex = (bossNightCount - 1) % bossPrefabs.Count;
            return bossPrefabs[bossIndex];
        }
    }

    private GameObject SelectBossByLunarPhase()
    {
        if (LunarCycleManager.Instance != null)
        {
            MoonPhase currentPhase = LunarCycleManager.Instance.GetCurrentMoonPhase();

            int phaseIndex = (int)currentPhase;
            if (phaseIndex < bossPrefabs.Count)
            {
                return bossPrefabs[phaseIndex];
            }
        }

        return bossPrefabs[Random.Range(0, bossPrefabs.Count)];
    }

    private void SetupBoss(GameObject boss)
    {
        LifeController bossLife = boss.GetComponent<LifeController>();
        if (bossLife != null)
        {
            bossLife.onDeath.AddListener(() => OnBossDefeated(boss));
        }
    }

    private void OnBossDefeated(GameObject defeatedBoss)
    {
        bossDefeated = true;
        currentBoss = null;

        onBossDefeated?.Invoke(defeatedBoss);

        GrantBossRewards();
    }

    private void GrantBossRewards()
    {
        if (InventoryManager.Instance != null)
        {
            int goldReward = 100 + (GameManager.Instance.GetCurrentDay() / bossNightInterval) * 50;
            InventoryManager.Instance.AddGold(goldReward);
        }
    }

    private void CompleteBossNight()
    {
        ResetBossNightState();
        onBossNightComplete?.Invoke();

        if (enemiesSpawner != null)
        {
            enemiesSpawner.onHordeEnd?.Invoke();
        }
    }

    private void ResetBossNightState()
    {
        isBossNight = false;
        bossHasSpawnedThisNight = false;
        bossDefeated = false;

        if (currentBoss != null)
        {
            currentBoss = null;
        }
    }

    public void ForceEndBossNight()
    {
        if (!isBossNight) return;

        if (currentBoss != null)
        {
            LifeController bossLife = currentBoss.GetComponent<LifeController>();
            if (bossLife != null)
            {
                bossLife.Kill();
            }
            else
            {
                Destroy(currentBoss);
                OnBossDefeated(currentBoss);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onNewDay.RemoveListener(OnNewDay);
        }
    }
}