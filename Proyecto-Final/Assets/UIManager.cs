using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] TextMeshProUGUI wavesText;
    [SerializeField] TextMeshProUGUI enemiesRemainingText;
    [SerializeField] TextMeshProUGUI timeText;

    [Header("Barra de Vida")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;

    [Header("Referencias")]
    [SerializeField] private GameObject player;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject HUD;
    public Button mainMenuButton;

    private WaveSpawner waveSpawner;
    private LifeController playerLife;
    private GameState lastGameState = GameState.None;

    void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerLife = player.GetComponent<LifeController>();

            if (playerLife != null)
            {
                playerLife.onHealthChanged.AddListener(UpdateHealthBar);
                InitializeHealthBar();
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(GameManager.Instance.ReturnToMainMenu);
            }
        }

        UpdateUIElementsVisibility();
    }

    void Update()
    {
        if (waveSpawner != null)
        {
            wavesText.text = "OLEADA: " + waveSpawner.GetCurrentWaveIndex().ToString() + " / " + waveSpawner.totalWaves.ToString();
            enemiesRemainingText.text = "ENEMIGOS RESTANTES: " + waveSpawner.GetRemainingEnemies().ToString() + " / " + waveSpawner.GetEnemiesPerWave().ToString();
        }

        UpdateTimeUI();

        if (GameManager.Instance != null && GameManager.Instance.currentGameState != lastGameState)
        {
            UpdateUIElementsVisibility();
            lastGameState = GameManager.Instance.currentGameState;
        }

        if(gameOverPanel.activeInHierarchy) 
            HUD.SetActive(false);
    }

    private void UpdateUIElementsVisibility()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentGameState == GameState.Day)
            {
                if (timeText != null) timeText.gameObject.SetActive(true);
                if (wavesText != null) wavesText.gameObject.SetActive(false);
                if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(false);
            }
            else if (GameManager.Instance.currentGameState == GameState.Night)
            {
                if (timeText != null) timeText.gameObject.SetActive(false);
                if (wavesText != null) wavesText.gameObject.SetActive(true);
                if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(true);
            }
        }
    }

    void InitializeHealthBar()
    {
        if (healthBar != null && playerLife != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = playerLife.maxHealth;
            healthBar.value = playerLife.currentHealth;
            UpdateFillColor(playerLife.GetHealthPercentage());
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            UpdateFillColor(currentHealth / maxHealth);
        }
    }

    private void UpdateFillColor(float healthPercentage)
    {
        if (fillImage != null && healthGradient != null)
        {
            fillImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    public void UpdateTimeUI()
    {
        if (timeText != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.currentGameState == GameState.Day)
            {
                int minutes = Mathf.FloorToInt(GameManager.Instance.currentDayTime / 60);
                int seconds = Mathf.FloorToInt(GameManager.Instance.currentDayTime % 60);
                timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }
}