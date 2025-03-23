using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public enum GameState
{
    None = 0,
    Paused,
    GameOver,
    Day,
    Night
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Day/Night Settings")]
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float dayDuration = 60f;
    public float currentDayTime;
    private bool isCycleRunning = false;

    [Header("Day Counter")]
    [SerializeField] private int dayCount = 0;
    public UnityEvent<int> onNewDay;

    private LifeController playerLife;
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
        DontDestroyOnLoad(gameObject);

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

        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();

            if (waveSpawner != null)
            {
                waveSpawner.onAllWavesCompleted.AddListener(HandleAllWavesCompleted);
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

    void Update()
    {
        if (isCycleRunning && currentGameState != GameState.Paused && currentGameState != GameState.GameOver)
        {
            UpdateDayCycle();
        }
    }

    private void StartDayCycle()
    {
        currentDayTime = dayDuration;
        isCycleRunning = true;

        SetGameState(GameState.Day);

        Debug.Log($"Starting day cycle. Day #{dayCount}");

        onNewDay.Invoke(dayCount);
    }

    private void UpdateDayCycle()
    {
        if (currentGameState == GameState.Day)
        {
            currentDayTime -= Time.deltaTime;

            if (uiManager != null)
            {
                uiManager.UpdateTimeUI();
            }

            if (currentDayTime <= 0)
            {
                TransitionToNight();
            }
        }
    }

    private void TransitionToNight()
    {
        Debug.Log("Transitioning to night");

        SetGameState(GameState.Night);
    }

    private void HandleAllWavesCompleted()
    {
        Debug.Log("All waves completed, transitioning to day");

        dayCount++;

        SetGameState(GameState.Day);
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

        Debug.Log($"Changing game state: {currentGameState} -> {newState}");
        currentGameState = newState;

        switch (newState)
        {
            case GameState.Day:
                Time.timeScale = 1f;
                break;

            case GameState.Night:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                break;
        }
    }

    private void HandlePlayerDeath()
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
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentDay()
    {
        return dayCount;
    }

    public void ResetDayCount()
    {
        dayCount = 1;
    }
}