using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    [Header("Referencias")]
    [SerializeField] private GameObject player;
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Settings")]
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float dayDuration = 60f;
    public float currentDayTime;
    private bool isCycleRunning = false;

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

        Debug.Log("Iniciando ciclo dia");
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
        Debug.Log("Transicion a noche");

        SetGameState(GameState.Night);
    }

    private void HandleAllWavesCompleted()
    {
        Debug.Log("Todas las oleadas completadas, transicion a día");

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

        Debug.Log($"Cambiando estado del juego: {currentGameState} -> {newState}");
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

        if (uiManager.gameOverPanel != null)
        {
            uiManager.gameOverPanel.SetActive(true);
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}