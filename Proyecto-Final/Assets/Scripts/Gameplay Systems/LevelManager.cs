using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



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
    [SerializeField] private PauseController pauseController;
    [Header("References")]
    [SerializeField] public GameObject home;
    [SerializeField] private EnemiesSpawner waveSpawner;
    [SerializeField] private List<SpawnPointAnimator> spawnpoints = new List<SpawnPointAnimator>();
    [SerializeField] private AmbienceSoundManager ambienceSoundManager;
    
    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;

    [Header("World Transition")]

    private GameResetController gameResetController;
    private HouseLifeController HomeLife;
    public AmbienceSoundManager AmbienceSoundManager => ambienceSoundManager;
    public UIManager uiManager;

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        gameResetController = GetComponent<GameResetController>();
        if (gameResetController == null)
        {
            UnityEngine.Debug.LogError("GameResetController missing on LevelManager!");
        }

        if (pauseController == null) pauseController = FindObjectOfType<PauseController>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (GameFlowController.Instance != null) GameFlowController.Instance.OnPhaseChanged += HandlePhaseChanged;
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
        DayCycleController.Instance.StartDay();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ForceEndNight();

        if (Input.GetKeyDown(KeyCode.L))
        {
            InventoryManager.Instance.AddGold(100);
        }

    }


    private void HandleHordeCompleted()
    {
        RewardsSystem.Instance?.EvaluateAndGrantReward();
        DayCycleController.Instance.StartDay();
    }

    private void OnDestroy() { if (GameFlowController.Instance != null) GameFlowController.Instance.OnPhaseChanged -= HandlePhaseChanged; }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
bool isNight = newPhase == GamePhase.Night;
        foreach (var spawnpoint in spawnpoints)
        {
            if (spawnpoint != null) spawnpoint.SetNightMode(isNight);
        }

        if (isNight)
        {
            RewardsSystem.Instance?.StartNightEvaluation();
        }
    }
    private void HandleHomeDeath()
    {
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        GameFlowController.Instance.SetPhase(GamePhase.GameOver);

        if (uiManager != null && uiManager.gameOverPanel != null)
        {
            uiManager.gameOverPanel.SetActive(true);

            if (pauseController != null) pauseController.Pause();
        }
    }

    public void ShowContinuePanel()
    {
        GameFlowController.Instance.SetPhase(GamePhase.GameOver);

        if (uiManager != null && uiManager.continuePanel != null)
        {
            uiManager.StartCoroutine(uiManager.AnimateContinuePanel());
            if (pauseController != null) pauseController.Pause();
        }
        else
        {
            Debug.LogWarning("No se encontr� el panel de 'Continuar�' en el UIManager.");
        }
    }

    public void GameOverRestart()
    {
        GameFlowController.Instance.SetPhase(GamePhase.Day);
        if (gameResetController != null) gameResetController.ResetGame();
        SceneLoaderManager.Instance.LoadGameScene();
    }
    
    public void GameOverMainMenu()
    {
        GameFlowController.Instance.SetPhase(GamePhase.Day);
        if (gameResetController != null) gameResetController.ResetGame();
        SceneLoaderManager.Instance.LoadMenuScene();
    }



    public void ForceEndNight()
    {
        foreach (var altar in FindObjectsOfType<RitualAltar>())
            altar.ForceStopRitual();

        var spawner = FindObjectOfType<EnemiesSpawner>();
        if (spawner != null) spawner.EndNight();
        else GameFlowController.Instance.SetPhase(GamePhase.Day);
        DayCycleController.Instance.StartDay();
    }
}














