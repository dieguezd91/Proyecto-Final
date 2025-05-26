using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState
{
    None = 0,
    Paused,
    GameOver,
    Day,
    Night,
    Digging,
    Planting,
    Harvesting,
    OnInventory,
    OnCrafting,
    MainMenu,
    Removing,
    OnAltarRestoration
}

public enum ElementEnum
{
    Ice,
    Wind,
    Electric,
    Fire,
    Stellar,
    Lunar
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] public GameObject home;
    [SerializeField] private EnemiesSpawner waveSpawner;
    [SerializeField] private List<HouseController> houseControllers = new List<HouseController>();

    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;

    [Header("Day Counter")]
    [SerializeField] private int dayCount = 0;
    public UnityEvent<int> onNewDay;

    [Header("Respawn")]
    [SerializeField] private float playerRespawnTime;
    [SerializeField] private Transform playerRespawnPoint;

    private LifeController playerLife;
    private LifeController HomeLife;
    public UIManager uiManager;
    public GameState currentGameState;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (onNewDay == null)
            onNewDay = new UnityEvent<int>();
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerLife = player.GetComponent<LifeController>();

            if (playerLife != null)
            {
                playerLife.onDeath.AddListener(HandlePlayerDeath);
            }
        }

        if (home == null)
        {
            home = GameObject.FindGameObjectWithTag("Home");
        }

        if (home != null)
        {
            HomeLife = home.GetComponent<LifeController>();

            if (HomeLife != null)
            {
                HomeLife.onDeath.AddListener(HandleHomeDeath);
            }
        }

        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<EnemiesSpawner>();

            if (waveSpawner != null)
            {
                waveSpawner.onHordeEnd.AddListener(HandleHordeCompleted);
            }
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        dayCount = 1;
        SetGameState(GameState.Day);
        StartDayCycle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            EnemiesSpawner spawner = FindObjectOfType<EnemiesSpawner>();
            if (spawner != null)
            {
                spawner.EndNight();
            }
        }
    }

    private void StartDayCycle()
    {
        SetGameState(GameState.Day);
        onNewDay.Invoke(dayCount);
    }

    public void ManualTransitionToNight()
    {
        SetGameState(GameState.Night);
        TributeSystem.Instance?.StartNightEvaluation();
    }

    private void HandleHordeCompleted()
    {
        dayCount++;

        SetGameState(GameState.Day);
        TributeSystem.Instance?.EvaluateAndGrantReward();
        StartDayCycle();
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    public void SetGameState(GameState newState)
    {
        if (currentGameState == newState)
            return;

        currentGameState = newState;

        UICursor cursorController = FindObjectOfType<UICursor>();
        if (cursorController != null)
        {
            cursorController.SetCursorForGameState(newState);
        }

        switch (newState)
        {
            case GameState.Day:
            case GameState.Night:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                break;
        }

        bool isNight = newState == GameState.Night;

        foreach (var house in houseControllers)
        {
            if (house != null)
                house.SetNightMode(isNight);
        }
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(RespawnPlayer());
    }

    private void HandleHomeDeath()
    {
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        SetGameState(GameState.GameOver);

        if (uiManager != null && uiManager.gameOverPanel != null)
        {
            uiManager.gameOverPanel.SetActive(true);
            
            Time.timeScale = 0f;
        }
    }

    public int GetCurrentDay()
    {
        return dayCount;
    }

    public void ResetDayCount()
    {
        dayCount = 1;
    }

    private IEnumerator RespawnPlayer()
    {
        player.SetActive(false);

        yield return new WaitForSeconds(playerRespawnTime);

        player.transform.position = playerRespawnPoint.position;
        player.SetActive(true);

        var controller = player.GetComponent<PlayerController>();
        controller.SetMovementEnabled(true);
        controller.SetCanAct(false);

        var life = player.GetComponent<LifeController>();
        life.ResetLife();
        yield return StartCoroutine(life.StartInvulnerability(playerRespawnTime));
        controller.SetCanAct(true);
    }

    public void Restart()
    {
        SetGameState(GameState.Day);
        ResetGameData();
        SceneManager.LoadScene(sceneBuildIndex:0);
    }

    public void ResetGameData()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAllMaterials();
        }

        if (SeedInventory.Instance != null)
        {
            for (int i = 0; i < 5; i++)
            {
                SeedInventory.Instance.RemoveSeedFromSlot(i);
            }
        }

        ResetDayCount();

        if (playerLife != null)
        {
            playerLife.currentHealth = playerLife.maxHealth;
            playerLife.onHealthChanged?.Invoke(playerLife.currentHealth, playerLife.maxHealth);
        }

        if (HomeLife != null)
        {
            HomeLife.currentHealth = HomeLife.maxHealth;
            HomeLife.onHealthChanged?.Invoke(HomeLife.currentHealth, HomeLife.maxHealth);
        }

        uiManager?.UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
        uiManager?.UpdateHomeHealthBar(HomeLife.currentHealth, HomeLife.maxHealth);
        uiManager?.UpdateManaUI();
        uiManager?.InitializeSeedSlotsUI();
    }
}