using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HEALTH BAR")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;

    [Header("HOME HEALTH BAR")]
    [SerializeField] private Slider homeHealthBar;
    [SerializeField] private Image homeFillImage;
    [SerializeField] private Gradient homeHealthGradient;

    [Header("MANA BAR")]
    [SerializeField] private Slider manaSlider;

    [Header("REFERENCES")]
    [SerializeField] private GameObject player;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject HUD;
    public GameObject pausePanel;
    public GameObject plantSlots;
    [SerializeField] private Button startNightButton;
    [SerializeField] private GameObject dayControlPanel;

    [Header("PLANT SLOTS")]
    [SerializeField] private GameObject[] slotObjects = new GameObject[5];
    [SerializeField] private Image[] slotIcons = new Image[5];
    [SerializeField] private Image[] slotBackgrounds = new Image[5];
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[5];

    [Header("SELECTION")]
    [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;

    [Header("INVENTORY UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleInventoryKey = KeyCode.I;
    [SerializeField] private KeyCode alternateToggleKey = KeyCode.Tab;
    [SerializeField] private bool closeInventoryOnEscape = true;
    [SerializeField] private bool disablePlayerMovementWhenOpen = true;
    [SerializeField] public InventoryUI inventoryUI;

    [Header("INSTRUCTIONS")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button continueButton;

    private bool openedFromPauseMenu = false;
    private LifeController playerLife;
    private ManaSystem manaSystem;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.Day;
    private bool isInventoryOpen = false;
    private bool isInstructionsOpen = false;
    private PlayerController playerController;
    private PauseMenu pauseMenu;

    void Start()
    {
        InitializeReferences();
        SetupListeners();
        InitializeUI();
    }

    void Update()
    {
        CheckGameStateChanges();
        HandleGameOverState();
        HandleInventoryInput();
    }

    private void OnDestroy()
    {
        if (SeedInventory.Instance != null)
        {
            SeedInventory.Instance.onSlotSelected -= UpdateSelectedSlotUI;
        }
    }

    private void InitializeReferences()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (pauseMenu == null)
        {
            pauseMenu = FindObjectOfType<PauseMenu>();
        }

        if (player != null)
        {
            playerLife = player.GetComponent<LifeController>();
            playerController = player.GetComponent<PlayerController>();
            manaSystem = player.GetComponent<ManaSystem>();
        }

        if (GameManager.Instance != null && GameManager.Instance.home != null)
        {
            LifeController homeLife = GameManager.Instance.home.GetComponent<LifeController>();
            if (homeLife != null)
            {
                homeLife.onHealthChanged.AddListener(UpdateHomeHealthBar);
                InitializeHomeHealthBar(homeLife);
            }
        }
    }

    private void SetupListeners()
    {
        if (playerLife != null)
        {
            playerLife.onHealthChanged.AddListener(UpdateHealthBar);
            InitializeHealthBar();
        }

        if (SeedInventory.Instance != null)
        {
            SeedInventory.Instance.onSlotSelected += UpdateSelectedSlotUI;
        }

        if (startNightButton != null)
        {
            startNightButton.onClick.AddListener(OnStartNightButtonClicked);
        }

        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(OpenInstructions);
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.AddListener(CloseInstructions);
        }

        if (continueButton != null && pauseMenu != null)
        {
            continueButton.onClick.AddListener(pauseMenu.Resume);
        }
    }

    private void InitializeUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            isInstructionsOpen = false;
        }

        if (SeedInventory.Instance != null)
        {
            InitializeSlotUI();
            UpdateSelectedSlotUI(SeedInventory.Instance.GetSelectedSlotIndex());
        }

        InitializeInventory();
        UpdateUIElementsVisibility();
    }

    private void InitializeInventory()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = GameObject.FindGameObjectWithTag("InventoryPanel");
        }

        if (inventoryUI == null && inventoryPanel != null)
        {
            inventoryUI = inventoryPanel.GetComponent<InventoryUI>();
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
    }

    public void InitializeSlotUI()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] != null)
            {
                PlantSlot plantSlot = SeedInventory.Instance.GetPlantSlot(i);
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

    private void InitializeHomeHealthBar(LifeController homeLife)
    {
        if (homeHealthBar != null)
        {
            homeHealthBar.minValue = 0;
            homeHealthBar.maxValue = homeLife.maxHealth;
            homeHealthBar.value = homeLife.currentHealth;
            UpdateHomeFillColor(homeLife.GetHealthPercentage());
        }
    }

    public void UpdateHomeHealthBar(float currentHealth, float maxHealth)
    {
        Debug.Log($"Home Health Updated: {currentHealth}/{maxHealth}");

        if (homeHealthBar != null)
        {
            homeHealthBar.value = currentHealth;
            UpdateHomeFillColor(currentHealth / maxHealth);
        }
    }

    private void UpdateHomeFillColor(float healthPercentage)
    {
        if (homeFillImage != null && homeHealthGradient != null)
        {
            homeFillImage.color = homeHealthGradient.Evaluate(healthPercentage);
        }
    }

    private void CheckGameStateChanges()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != lastGameState)
        {
            if (GameManager.Instance.currentGameState == GameState.Paused &&
                lastGameState != GameState.Paused &&
                lastGameState != GameState.None)
            {
                lastState = lastGameState;
            }

            UpdateUIElementsVisibility();
            lastGameState = GameManager.Instance.currentGameState;
        }
    }

    private void HandleGameOverState()
    {
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy && HUD != null)
        {
            HUD.SetActive(false);
        }
    }

    private void UpdateUIElementsVisibility()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentGameState == GameState.Day)
            {
                if (plantSlots != null) plantSlots.gameObject.SetActive(true && !isInstructionsOpen);
                if (dayControlPanel != null) dayControlPanel.SetActive(true && !isInstructionsOpen);
                if (startNightButton != null) startNightButton.gameObject.SetActive(true && !isInstructionsOpen);
            }
            else if (GameManager.Instance.currentGameState == GameState.Night)
            {
                if (plantSlots != null) plantSlots.gameObject.SetActive(false);
                if (dayControlPanel != null) dayControlPanel.SetActive(false);
                if (startNightButton != null) startNightButton.gameObject.SetActive(false);
            }
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

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(toggleInventoryKey) || Input.GetKeyDown(alternateToggleKey))
        {
            ToggleInventory();
        }
        if (closeInventoryOnEscape && Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
        {
            CloseInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null || isInstructionsOpen) return;
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.Day)
        {
            return;
        }
        inventoryPanel.SetActive(true);
        isInventoryOpen = true;
        if (inventoryUI != null)
        {
            inventoryUI.UpdateAllSlots();
            inventoryUI.ForceRefresh();
        }
        if (disablePlayerMovementWhenOpen && playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        Debug.Log("Inventario abierto");
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
        isInventoryOpen = false;
        if (disablePlayerMovementWhenOpen && playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        Debug.Log("Inventario cerrado");
    }

    public bool IsInventoryOpen() => isInventoryOpen;

    public void SetInventoryOpen(bool open)
    {
        if (open != isInventoryOpen)
        {
            if (open)
                OpenInventory();
            else
                CloseInventory();
        }
    }

    public void OpenInstructions()
    {
        if (instructionsPanel == null) return;

        openedFromPauseMenu = pausePanel != null && pausePanel.activeSelf;

        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.Paused)
        {
            lastState = GameManager.Instance.currentGameState;
        }

        if (HUD != null) HUD.SetActive(false);
        if (plantSlots != null) plantSlots.SetActive(false);
        if (dayControlPanel != null) dayControlPanel.SetActive(false);
        if (inventoryPanel != null && isInventoryOpen) inventoryPanel.SetActive(false);
        if (startNightButton != null) startNightButton.gameObject.SetActive(false);

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        instructionsPanel.SetActive(true);
        isInstructionsOpen = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Paused);
        }

        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
    }

    public void CloseInstructions()
    {
        if (instructionsPanel == null) return;

        instructionsPanel.SetActive(false);
        isInstructionsOpen = false;

        if (openedFromPauseMenu && pausePanel != null)
        {
            pausePanel.SetActive(true);
            PauseMenu.isGamePaused = true;
            Time.timeScale = 0f;

            if (HUD != null)
            {
                HUD.SetActive(false);
            }
        }
        else
        {
            if (HUD != null) HUD.SetActive(true);
            UpdateUIElementsVisibility();

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }

            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Paused)
            {
                GameManager.Instance.SetGameState(lastState);
            }
        }

        Debug.Log("Panel de instrucciones cerrado");
    }

    public bool IsInstructionsOpen() => isInstructionsOpen;

    private void OnStartNightButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Day)
        {
            GameManager.Instance.ManualTransitionToNight();
        }
    }

    public void UpdateManaUI()
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = manaSystem.GetMaxMana();
            manaSlider.value = manaSystem.GetCurrentMana();
        }
    }
}