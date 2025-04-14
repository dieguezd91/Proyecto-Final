using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject home;
    [SerializeField] private EnemiesSpawner waveSpawner;

    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;

    [Header("Day Counter")]
    [SerializeField] private int dayCount = 0;
    public UnityEvent<int> onNewDay;

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
        //DontDestroyOnLoad(gameObject); (lo que ocasionaba el bug de que se quedara en noche)

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
        if (currentGameState == GameState.Day)
        {
            SetGameState(GameState.Night);
        }
    }

    private void HandleHordeCompleted()
    {
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

    public void Restart()
    {
        SetGameState(GameState.Day);
        SceneManager.LoadScene(sceneBuildIndex:0);
    }
}