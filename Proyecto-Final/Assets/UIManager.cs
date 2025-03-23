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
    public GameObject plantSlots;

    [Header("Plant Slots")]
    [SerializeField] private GameObject[] slotObjects = new GameObject[5];
    [SerializeField] private Image[] slotIcons = new Image[5];
    [SerializeField] private Image[] slotBackgrounds = new Image[5];
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[5];

    [Header("Selection")]
    [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;

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

        if (PlantInventory.Instance != null)
        {
            PlantInventory.Instance.onSlotSelected += UpdateSelectedSlotUI;

            InitializeSlotUI();

            UpdateSelectedSlotUI(PlantInventory.Instance.GetSelectedSlotIndex());
        }
        else
        {
            Debug.LogError("PlantInventory instance not found!");
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

    private void OnDestroy()
    {
        if (PlantInventory.Instance != null)
        {
            PlantInventory.Instance.onSlotSelected -= UpdateSelectedSlotUI;
        }
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
                if(plantSlots != null) plantSlots.gameObject.SetActive(true);
            }
            else if (GameManager.Instance.currentGameState == GameState.Night)
            {
                if (timeText != null) timeText.gameObject.SetActive(false);
                if (wavesText != null) wavesText.gameObject.SetActive(true);
                if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(true);
                if(plantSlots != null) plantSlots.gameObject.SetActive(false);
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

    private void InitializeSlotUI()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] != null)
            {
                PlantSlot plantSlot = PlantInventory.Instance.GetPlantSlot(i);

                if (plantSlot != null && plantSlot.plantPrefab != null)
                {
                    if (slotIcons[i] != null)
                    {
                        slotIcons[i].sprite = plantSlot.plantIcon;
                        slotIcons[i].preserveAspect = true;
                        slotIcons[i].gameObject.SetActive(true);
                    }

                    if (slotNumbers[i] != null)
                    {
                        slotNumbers[i].text = (i + 1).ToString();
                    }
                }
                else
                {
                    if (slotIcons[i] != null)
                    {
                        slotIcons[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void UpdateSelectedSlotUI(int selectedIndex)
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == selectedIndex) ? selectedColor : normalColor;
                slotObjects[i].transform.localScale = (i == selectedIndex) ?
                    new Vector3(selectedScale, selectedScale, 1f) :
                    new Vector3(normalScale, normalScale, 1f);
            }
        }
    }
}