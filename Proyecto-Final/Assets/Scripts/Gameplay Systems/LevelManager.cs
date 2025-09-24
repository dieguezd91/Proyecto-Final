using System;
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
    OnAltarRestoration,
    OnRitual
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

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] public GameObject home;
    [SerializeField] private EnemiesSpawner waveSpawner;
    [SerializeField] private List<HouseController> houseControllers = new List<HouseController>();
    [SerializeField] private List<SpawnPointAnimator> spawnpoints = new List<SpawnPointAnimator>();
    [SerializeField] private AmbienceSoundManager ambienceSoundManager;
    
    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;

    [Header("Day Counter")]
    [SerializeField] private int dayCount = 0;
    public UnityEvent<int> onNewDay;

    [Header("Respawn")]
    [SerializeField] public float playerRespawnTime;
    [SerializeField] private Transform playerRespawnPoint;

    [Header("World Transition")]
    [SerializeField] private WorldTransitionAnimator worldAnimator;

    public LifeController playerLife;
    private HouseLifeController HomeLife;
    public UIManager uiManager;
    public GameState currentGameState;

    public static LevelManager Instance { get; private set; }

    public event Action<GameState> OnGameStateChanged;

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

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerLife = player.GetComponent<LifeController>();
        }
    }

    private void Start()
    {
        if (home == null)
            home = GameObject.FindGameObjectWithTag("Home");

        if (home != null)
        {
            HomeLife = home.GetComponent<HouseLifeController>();
            if (HomeLife != null)
                HomeLife.onHouseDestroyed.AddListener(HandleHomeDeath);
        }

        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<EnemiesSpawner>();
            if (waveSpawner != null)
                waveSpawner.onHordeEnd.AddListener(HandleHordeCompleted);
        }

        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (ambienceSoundManager == null)
            ambienceSoundManager = FindObjectOfType<AmbienceSoundManager>();

        dayCount = 0;
        SetGameState(GameState.Digging);
        StartDayCycle();
    }

    private void StartDayCycle()
    {
        SetGameState(GameState.Digging);

        if (ambienceSoundManager != null)
            ambienceSoundManager.TransitionAmbience(AmbienceType.Forest, 1f);

        onNewDay?.Invoke(dayCount);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ForceEndNight();

        if (Input.GetKeyDown(KeyCode.L))
        {
            InventoryManager.Instance.AddGold(100);
            Debug.Log("+100 oro agregado");
        }
    }

    public void TransitionToNight()
    {
        dayCount++;
        SetGameState(GameState.Night);

        if (ambienceSoundManager == null)
            ambienceSoundManager = FindObjectOfType<AmbienceSoundManager>();

        if (ambienceSoundManager != null)
            ambienceSoundManager.TransitionAmbience(AmbienceType.Infernum, 1f);

        TributeSystem.Instance?.StartNightEvaluation();
    }


    private void HandleHordeCompleted()
    {
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

        HandleWorldTransition(newState);

        currentGameState = newState;

        OnGameStateChanged?.Invoke(currentGameState);

        if (newState == GameState.Digging)
        {
            var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
            if (abilitySystem != null && abilitySystem.CurrentAbility != PlayerAbility.Digging)
            {
                abilitySystem.SetAbility(PlayerAbility.Digging);
            }
        }

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

        foreach (var spawnpoint in spawnpoints)
        {
            spawnpoint.SetNightMode(isNight);
        }

        foreach (var house in houseControllers)
        {
            if (house != null)
                house.SetNightMode(isNight);
        }
    }

    private void HandleWorldTransition(GameState newState)
    {
        if (worldAnimator == null)
        {
            worldAnimator = FindObjectOfType<WorldTransitionAnimator>();
            if (worldAnimator == null) return;
        }

        HashSet<GameState> nonTransitionStates = new HashSet<GameState>
    {
        GameState.GameOver,
        GameState.OnInventory,
        GameState.OnCrafting,
        GameState.OnAltarRestoration,
        GameState.OnRitual,
        GameState.Paused
    };

        if (nonTransitionStates.Contains(newState))
        {
            return;
        }

        switch (newState)
        {
            case GameState.Night:
                worldAnimator.TransitionToNight();
                break;

            case GameState.Day:
            case GameState.Digging:
            case GameState.Planting:
            case GameState.Harvesting:
            case GameState.Removing:
                worldAnimator.TransitionToDay();
                break;
        }
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

    public IEnumerator RespawnPlayer()
    {
        uiManager?.AnimateRespawnRecovery(playerRespawnTime);
        
        yield return new WaitForSeconds(playerRespawnTime);

        var controller = player.GetComponent<PlayerController>();
        controller.SetMovementEnabled(true);
        controller.SetCanAct(false);

        var life = player.GetComponent<LifeController>();

        yield return StartCoroutine(life.StartInvulnerability(playerRespawnTime));

        life.ResetLife();
    }

    public void OnPlayerDeathAnimationComplete()
    {
        StartCoroutine(RespawnPlayer());
    }

    public Transform GetPlayerRespawnPoint()
    {
        return playerRespawnPoint;
    }

    public void GameOverRestart()
    {
        SetGameState(GameState.Digging);
        ResetGameData();
        SceneLoaderManager.Instance.LoadGameScene();
    }
    
    public void GameOverMainMenu()
    {
        SetGameState(GameState.Digging);
        ResetGameData();
        SceneLoaderManager.Instance.LoadMenuScene();
    }

    public void ResetGameData()
    {
        Debug.Log("Reseteando");
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.CloseInventory();
            if (uiManager.gameOverPanel != null)
                uiManager.gameOverPanel.SetActive(false);
        }

        uiManager?.InitializeSeedSlotsUI();

        if (SeedInventory.Instance != null)
        {
            for (int i = 0; i < 9; i++)
                SeedInventory.Instance.RemoveSeedFromSlot(i);

            SeedInventory.Instance.SelectSlot(0);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAllMaterials();
            InventoryManager.Instance.SetGold(0);
        }

        ResetDayCount();

        if (playerLife != null)
        {
            playerLife.currentHealth = playerLife.maxHealth;
            playerLife.onHealthChanged?.Invoke(playerLife.currentHealth, playerLife.maxHealth);
        }

        if (HomeLife != null)
        {
            HomeLife.ResetLife();
        }

        if (player != null)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
                playerController.SetCanAct(true);
            }

            var lifeController = player.GetComponent<LifeController>();
            if (lifeController != null)
            {
                lifeController.ResetLife();
            }
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) Destroy(enemy);
        }

        Spell[] activeSpells = FindObjectsOfType<Spell>();
        foreach (Spell spell in activeSpells)
        {
            if (spell != null) Destroy(spell.gameObject);
        }

        ManaSystem manaSystem = FindObjectOfType<ManaSystem>();
        if (manaSystem != null)
        {
            manaSystem.SetMana(manaSystem.GetBaseMaxMana());
        }

        uiManager?.UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
        uiManager?.UpdateHomeHealthBar(HomeLife.CurrentHealth, HomeLife.MaxHealth);
        uiManager?.UpdateManaUI();

        Debug.Log("Reset completado.");
    }
    public void ForceEndNight()
    {
        foreach (var altar in FindObjectsOfType<RitualAltar>())
            altar.ForceStopRitual();

        var spawner = FindObjectOfType<EnemiesSpawner>();
        if (spawner != null) spawner.EndNight();
        else SetGameState(GameState.Digging);

        StartDayCycle();
    }
}
