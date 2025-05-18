using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HEALTH BAR")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private TextMeshProUGUI playerHealthText;

    [Header("HOME HEALTH BAR")]
    [SerializeField] private Slider homeHealthBar;
    [SerializeField] private Image homeFillImage;
    [SerializeField] private Gradient homeHealthGradient;
    [SerializeField] private TextMeshProUGUI homeHealthText;

    [Header("FLOATING DAMAGE")]
    [SerializeField] private GameObject floatingDamagePrefab;
    private float lastPlayerHealth;

    [Header("MANA BAR")]
    [SerializeField] private Slider manaBar;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Gradient manaGradient;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("REFERENCES")]
    [SerializeField] private GameObject player;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject HUD;
    public GameObject pausePanel;
    public GameObject seedSlots;
    [SerializeField] private CanvasGroup seedSlotsCanvasGroup;
    [SerializeField] private Button startNightButton;
    [SerializeField] private GameObject dayControlPanel;

    [Header("PLANT SLOTS")]
    [SerializeField] private GameObject[] slotObjects = new GameObject[5];
    [SerializeField] private Image[] slotIcons = new Image[5];
    [SerializeField] private Image[] slotBackgrounds = new Image[5];
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] seedCount = new TextMeshProUGUI[5];

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

    [Header("FEEDBACK")]
    [SerializeField] private GameObject damagedScreen;

    [Header("PLAYER ABILITY UI")]
    [SerializeField] private GameObject abilityPanel;

    private bool openedFromPauseMenu = false;
    private LifeController playerLife;
    private ManaSystem manaSystem;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.Day;
    private bool isInventoryOpen = false;
    private bool isInstructionsOpen = false;
    private PlayerController playerController;
    private PauseMenu pauseMenu;
    private Coroutine fadeCoroutine;

    void Start()
    {
        InitializeReferences();
        SetupListeners();
        InitializeUI();

        if (playerLife != null)
            UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);

        if (GameManager.Instance?.home != null)
        {
            LifeController homeLife = GameManager.Instance.home.GetComponent<LifeController>();
            if (homeLife != null)
                UpdateHomeHealthBar(homeLife.currentHealth, homeLife.maxHealth);
        }

        UpdateManaUI();
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

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem abilitySystem)
        {
            abilitySystem.OnAbilityChanged -= OnAbilityChanged;
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
            lastPlayerHealth = playerLife.currentHealth;
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

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem abilitySystem)
        {
            abilitySystem.OnAbilityChanged += OnAbilityChanged;
        }
    }

    private void InitializeUI()
    {
        if (FindObjectOfType<PlayerAbilitySystem>()?.CurrentAbility != PlayerAbility.Planting)
        {
            seedSlotsCanvasGroup.alpha = 0.5f;
            seedSlotsCanvasGroup.interactable = false;
            seedSlotsCanvasGroup.blocksRaycasts = false;
        }

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
            InitializeSeedSlotsUI();
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

    public void InitializeSeedSlotsUI()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] != null)
            {
                PlantSlot plantSlot = SeedInventory.Instance.GetPlantSlot(i);

                if (plantSlot != null && plantSlot.seedCount > 0 && plantSlot.plantPrefab != null)
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

                    if (seedCount[i] != null)
                    {
                        seedCount[i].text = plantSlot.seedCount.ToString();
                    }
                }
                else
                {
                    if (slotIcons[i] != null) slotIcons[i].gameObject.SetActive(false);
                    if (slotNumbers[i] != null) slotNumbers[i].text = "";
                    if (seedCount[i] != null) seedCount[i].text = "";
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

        if (homeHealthText != null)
        {
            homeHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
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
            UpdateAbilityUIVisibility();
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
            if (GameManager.Instance.currentGameState != GameState.Night)
            {
                if (seedSlots != null) seedSlots.gameObject.SetActive(true && !isInstructionsOpen);
                if (dayControlPanel != null) dayControlPanel.SetActive(true && !isInstructionsOpen);
                if (startNightButton != null) startNightButton.gameObject.SetActive(true && !isInstructionsOpen);
            }
            else if (GameManager.Instance.currentGameState == GameState.Night)
            {
                if (seedSlots != null) seedSlots.gameObject.SetActive(false);
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
       
        float damageTaken = lastPlayerHealth - currentHealth;
        if (damageTaken > 0f && floatingDamagePrefab != null)
        {
            Vector3 spawnPos = player.transform.position + Vector3.up * 0.5f;
            GameObject txt = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
            txt.GetComponent<FloatingDamageText>()?.SetText(damageTaken);
        }
        lastPlayerHealth = currentHealth;

        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
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

    private void UpdateAbilityUIVisibility()
    {
        if (abilityPanel == null || GameManager.Instance == null)
            return;

        if (GameManager.Instance.currentGameState != GameState.Night)
            abilityPanel.SetActive(true);
        else if (GameManager.Instance.currentGameState == GameState.Night)
            abilityPanel.SetActive(false);
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
        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night)
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

        GameManager.Instance?.SetGameState(GameState.OnInventory);

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

        if (GameManager.Instance?.GetCurrentGameState() == GameState.OnInventory)
        {
            GameManager.Instance.SetGameState(GameState.Day);
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
        if (seedSlots != null) seedSlots.SetActive(false);
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
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.Night)
        {
            GameManager.Instance.ManualTransitionToNight();
        }
    }

    public void UpdateManaUI()
    {
        if (manaBar != null && manaSystem != null)
        {
            float current = manaSystem.GetCurrentMana();
            float max = manaSystem.GetMaxMana();
            float percent = current / max;

            manaBar.maxValue = max;
            manaBar.value = current;

            if (manaFillImage != null && manaGradient != null)
            {
                manaFillImage.color = manaGradient.Evaluate(percent);
            }

            if (manaText != null)
            {
                manaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }
    }

    public void UpdateSeedCountsUI()
    {
        for (int i = 0; i < seedCount.Length; i++)
        {
            var slot = SeedInventory.Instance.GetPlantSlot(i);
            if (slot != null)
            {
                seedCount[i].text = slot.seedCount > 0 ? slot.seedCount.ToString() : "-";
            }
        }
    }

    public void ShowDamagedScreen()
    {
        if (damagedScreen == null) return;
        damagedScreen.SetActive(true);
        StopCoroutine(nameof(HideDamagedScreen));
        StartCoroutine(HideDamagedScreen());
    }

    private IEnumerator HideDamagedScreen()
    {
        yield return new WaitForSeconds(0.5f);
        damagedScreen.SetActive(false);
    }

    private void OnAbilityChanged(PlayerAbility newAbility)
    {
        if (seedSlotsCanvasGroup == null) return;

        bool show = newAbility == PlayerAbility.Planting;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePlantSlots(show));
    }

    private IEnumerator FadePlantSlots(bool fadeIn)
    {
        float duration = 0.35f;
        float startAlpha = seedSlotsCanvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            seedSlotsCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        seedSlotsCanvasGroup.alpha = targetAlpha;
        seedSlotsCanvasGroup.interactable = fadeIn;
        seedSlotsCanvasGroup.blocksRaycasts = fadeIn;
    }
}