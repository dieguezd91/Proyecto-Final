using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("HEALTH BAR")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image whiteFillImage;
    [SerializeField] private float whiteBarSpeed = 1.5f;

    private float targetFill = 1f;

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
    [SerializeField] private GameObject[] slotObjects = new GameObject[9];
    [SerializeField] private Image[] slotIcons = new Image[9];
    [SerializeField] private Image[] slotBackgrounds = new Image[9];
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[9];
    [SerializeField] private TextMeshProUGUI[] seedCount = new TextMeshProUGUI[9];

    [Header("SELECTION COLORS")]
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
    private CanvasGroup damagedScreenCanvasGroup;
    private float targetDamageAlpha = 0f;
    private float damageFadeSpeed = 2f;
    private float damageFadeOutDelay = 1f;
    private float lastDamageTime = 0f;

    [Header("PLAYER ABILITY UI")]
    [SerializeField] private GameObject abilityPanel;

    [Header("TOOLTIP")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(20f, -20f);

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
    private Coroutine warningCoroutine;

    private int lastPressSlot = -1;            
    private float lastPressTime = 0f;          
    [SerializeField] private float doublePressThreshold = 0.3f;


    // Drag & Drop fields
    private Transform _slotsParent;
    private Transform _draggingSlot;
    private GameObject _placeholder;
    private int _originalSiblingIndex;


    private int pendingSwapSlot = -1;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeReferences();
        SetupListeners();
        InitializeUI();


        foreach (var go in slotObjects)
        {
            if (go != null && go.GetComponent<CanvasGroup>() == null)
                go.AddComponent<CanvasGroup>();
        }

        if (slotObjects.Length > 0 && slotObjects[0] != null)
            _slotsParent = slotObjects[0].transform.parent;

        RegisterSlotDragEvents();


        if (playerLife != null)
            UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);

        if (GameManager.Instance?.home != null)
        {
            HouseLifeController homeLife = GameManager.Instance.home.GetComponent<HouseLifeController>();
            if (homeLife != null)
                UpdateHomeHealthBar(homeLife.CurrentHealth, homeLife.MaxHealth);
        }


        if (damagedScreen != null)
        {
            damagedScreenCanvasGroup = damagedScreen.GetComponent<CanvasGroup>()
                                   ?? damagedScreen.AddComponent<CanvasGroup>();
            damagedScreenCanvasGroup.alpha = 0f;
            damagedScreen.SetActive(true);
        }

        lastDamageTime = Time.time;

        UpdateManaUI();

        if (whiteFillImage != null)
            whiteFillImage.fillAmount = playerLife.GetHealthPercentage();
    }

    void Update()
    {
        CheckGameStateChanges();
        HandleGameOverState();
        HandleInventoryInput();
        HandleSeedSlotInput();
        UpdateTooltipPosition();

        if (damagedScreenCanvasGroup != null)
        {
            if (Time.time - lastDamageTime > damageFadeOutDelay)
                targetDamageAlpha = 0f;

            damagedScreenCanvasGroup.alpha = Mathf.Lerp(
                damagedScreenCanvasGroup.alpha,
                targetDamageAlpha,
                damageFadeSpeed * Time.deltaTime
            );
        }
        UpdateWhiteBar();

    }

    private void OnDestroy()
    {
        if (SeedInventory.Instance != null)
            SeedInventory.Instance.onSlotSelected -= UpdateSelectedSlotUI;

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem abilitySystem)
            abilitySystem.OnAbilityChanged -= OnAbilityChanged;
    }

    private void InitializeReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();

        if (player != null)
        {
            playerLife = player.GetComponent<LifeController>();
            playerController = player.GetComponent<PlayerController>();
            manaSystem = player.GetComponent<ManaSystem>();
            lastPlayerHealth = playerLife.currentHealth;
        }

        if (GameManager.Instance != null && GameManager.Instance.home != null)
        {
            HouseLifeController homeLife = GameManager.Instance.home.GetComponent<HouseLifeController>();
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
            SeedInventory.Instance.onSlotSelected += UpdateSelectedSlotUI;

        if (startNightButton != null)
            startNightButton.onClick.AddListener(OnStartNightButtonClicked);

        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OpenInstructions);

        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(CloseInstructions);

        if (continueButton != null && pauseMenu != null)
            continueButton.onClick.AddListener(pauseMenu.Resume);

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem abilitySystem)
            abilitySystem.OnAbilityChanged += OnAbilityChanged;
    }

    private void InitializeUI()
    {
        
        if (FindObjectOfType<PlayerAbilitySystem>()?.CurrentAbility != PlayerAbility.Planting)
        {
            seedSlotsCanvasGroup.alpha = 0.5f;
            //seedSlotsCanvasGroup.interactable = false;
            //seedSlotsCanvasGroup.blocksRaycasts = false;

        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

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
            inventoryPanel = GameObject.FindGameObjectWithTag("InventoryPanel");

        if (inventoryUI == null && inventoryPanel != null)
            inventoryUI = inventoryPanel.GetComponent<InventoryUI>();

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

            if (slotObjects[i] == null) continue;

            var tooltip = slotObjects[i].GetComponent<PlantSlotTooltipHandler>();
            if (tooltip == null)
                tooltip = slotObjects[i].AddComponent<PlantSlotTooltipHandler>();

            tooltip.slotIndex = i;



            if (slotNumbers[i] != null)
                slotNumbers[i].text = (i + 1).ToString();

            PlantSlot plantSlot = SeedInventory.Instance.GetPlantSlot(i);

            if (plantSlot != null && plantSlot.seedCount > 0 && plantSlot.plantPrefab != null)
            {
                if (slotIcons[i] != null)
                {
                    slotIcons[i].sprite = plantSlot.plantIcon;
                    slotIcons[i].preserveAspect = true;
                    slotIcons[i].gameObject.SetActive(true);
                }
                if (seedCount[i] != null)
                {
                    seedCount[i].text = plantSlot.seedCount.ToString();
                    seedCount[i].enabled = true;
                }
            }
            else
            {
                if (slotIcons[i] != null) slotIcons[i].gameObject.SetActive(false);
                if (seedCount[i] != null) seedCount[i].enabled = false;
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

    private void InitializeHomeHealthBar(HouseLifeController homeLife)
    {
        if (homeHealthBar != null)
        {
            homeHealthBar.minValue = 0;
            homeHealthBar.maxValue = homeLife.maxHealth;
            homeHealthBar.value = homeLife.currentHealth;
            UpdateHomeFillColor(homeLife.GetHealthPercent());
        }
    }

    public void UpdateHomeHealthBar(float currentHealth, float maxHealth)
    {
        if (homeHealthBar != null)
        {
            homeHealthBar.value = currentHealth;
            UpdateHomeFillColor(currentHealth / maxHealth);
        }

        if (homeHealthText != null)
            homeHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private void UpdateHomeFillColor(float healthPercentage)
    {
        if (homeFillImage != null && homeHealthGradient != null)
            homeFillImage.color = homeHealthGradient.Evaluate(healthPercentage);
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
            HUD.SetActive(false);
    }

    private void UpdateUIElementsVisibility()
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.currentGameState;

        bool showHUD = state == GameState.Day ||
                       state == GameState.Digging ||
                       state == GameState.Planting ||
                       state == GameState.Harvesting ||
                       state == GameState.Removing ||
                       state == GameState.Night;

        if (HUD != null) HUD.SetActive(showHUD);

        bool showGameplayUI = state == GameState.Day ||
                              state == GameState.Digging ||
                              state == GameState.Planting ||
                              state == GameState.Harvesting ||
                              state == GameState.Removing;

        if (seedSlots != null) seedSlots.SetActive(showGameplayUI && !isInstructionsOpen);
        if (dayControlPanel != null) dayControlPanel.SetActive(showGameplayUI && !isInstructionsOpen);
        if (startNightButton != null) startNightButton.gameObject.SetActive(showGameplayUI && !isInstructionsOpen);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            float percent = currentHealth / maxHealth;
            healthBar.value = currentHealth;
            targetFill = percent;
            UpdateFillColor(percent);
        }


        float damageTaken = lastPlayerHealth - currentHealth;
        if (damageTaken > 0f && floatingDamagePrefab != null)
        {
            lastDamageTime = Time.time;
            var existing = player.GetComponentInChildren<FloatingDamageText>();
            if (existing != null)
            {
                existing.AddDamage(damageTaken);
            }
            else
            {
                Vector3 spawnPos = player.transform.position + Vector3.up * 1f;
                var go = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
                var fdt = go.GetComponent<FloatingDamageText>();
                fdt.Initialize(player.transform);
                fdt.AddDamage(damageTaken);
            }
        }

        float healthPerc = currentHealth / maxHealth;
        targetDamageAlpha = 1f - healthPerc;

        lastPlayerHealth = currentHealth;

        if (currentHealth < lastPlayerHealth)
            TributeSystem.Instance?.NotifyPlayerDamaged();

        lastPlayerHealth = currentHealth;

        if (playerHealthText != null)
            playerHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private void UpdateFillColor(float healthPercentage)
    {
        if (fillImage != null && healthGradient != null)
            fillImage.color = healthGradient.Evaluate(healthPercentage);
    }

    private void UpdateSelectedSlotUI(int selectedIndex)
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;

            bool isSelected = (i == selectedIndex);
            slotBackgrounds[i].color = isSelected ? selectedColor : normalColor;
            slotObjects[i].transform.localScale = isSelected
                ? new Vector3(selectedScale, selectedScale, 1f)
                : new Vector3(normalScale, normalScale, 1f);
        }
    }

    private void UpdateAbilityUIVisibility()
    {
        if (abilityPanel == null || GameManager.Instance == null) return;

        GameState state = GameManager.Instance.currentGameState;
        bool showAbilities = state != GameState.Night &&
                             state != GameState.OnInventory &&
                             state != GameState.OnCrafting &&
                             state != GameState.OnAltarRestoration &&
                             state != GameState.Paused &&
                             state != GameState.GameOver;

        abilityPanel.SetActive(showAbilities);
    }

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(toggleInventoryKey) || Input.GetKeyDown(alternateToggleKey))
            ToggleInventory();

        if (closeInventoryOnEscape && Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
            CloseInventory();
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null || isInstructionsOpen) return;
        if (GameManager.Instance != null &&
            (GameManager.Instance.currentGameState == GameState.Night ||
             GameManager.Instance.currentGameState == GameState.Paused ||
             GameManager.Instance.currentGameState == GameState.OnCrafting ||
             GameManager.Instance.currentGameState == GameState.GameOver ||
             GameManager.Instance.currentGameState == GameState.OnAltarRestoration))
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
            playerController.SetMovementEnabled(false);

        GameManager.Instance?.SetGameState(GameState.OnInventory);
        Debug.Log("Inventario abierto");
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
        isInventoryOpen = false;

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(true);

        if (GameManager.Instance?.GetCurrentGameState() == GameState.OnInventory)
            GameManager.Instance.SetGameState(GameState.Digging);

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
            lastState = GameManager.Instance.currentGameState;

        if (HUD != null) HUD.SetActive(false);
        if (seedSlots != null) seedSlots.SetActive(false);
        if (dayControlPanel != null) dayControlPanel.SetActive(false);
        if (inventoryPanel != null && isInventoryOpen) inventoryPanel.SetActive(false);
        if (startNightButton != null) startNightButton.gameObject.SetActive(false);

        if (pausePanel != null) pausePanel.SetActive(false);

        instructionsPanel.SetActive(true);
        isInstructionsOpen = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Paused);

        if (playerController != null)
            playerController.SetMovementEnabled(false);
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
                HUD.SetActive(false);
        }
        else
        {
            if (HUD != null) HUD.SetActive(true);
            UpdateUIElementsVisibility();

            if (playerController != null)
                playerController.SetMovementEnabled(true);

            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Paused)
                GameManager.Instance.SetGameState(lastState);
        }

        Debug.Log("Panel de instrucciones cerrado");
    }

    public bool IsInstructionsOpen() => isInstructionsOpen;

    private void OnStartNightButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.Night)
            GameManager.Instance.ManualTransitionToNight();
    }

    public void UpdateManaUI()
    {
        if (manaBar == null || manaSystem == null) return;

        float current = manaSystem.GetCurrentMana();
        float max = manaSystem.GetMaxMana();
        float percent = current / max;

        manaBar.maxValue = max;
        manaBar.value = current;

        if (manaFillImage != null && manaGradient != null)
            manaFillImage.color = manaGradient.Evaluate(percent);

        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public void UpdateSeedCountsUI()
    {
        for (int i = 0; i < seedCount.Length; i++)
        {
            var slot = SeedInventory.Instance.GetPlantSlot(i);
            if (slot != null)
                seedCount[i].text = slot.seedCount > 0 ? slot.seedCount.ToString() : "";
        }
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
        //seedSlotsCanvasGroup.interactable = fadeIn;
        //seedSlotsCanvasGroup.blocksRaycasts = fadeIn;
    }


    private void HandleSeedSlotInput()
    {
        if (GameManager.Instance == null) return;
        var state = GameManager.Instance.currentGameState;

        bool validState =
            state == GameState.Day ||
            state == GameState.Digging ||
            state == GameState.Planting ||
            state == GameState.Harvesting ||
            state == GameState.Removing;
        if (!validState) return;

        for (int i = 0; i < slotObjects.Length; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;  
            if (Input.GetKeyDown(key))
            {
                OnSlotKeyPressed(i);
                break;
            }
        }
    }



    private void RegisterSlotDragEvents()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            var go = slotObjects[i];
            if (go == null) continue;
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            int copy = i;

            var clickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            clickEntry.callback.AddListener(evt => OnSlotClicked(copy));
            trigger.triggers.Add(clickEntry);

            var bd = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            bd.callback.AddListener(_ => BeginDragIcon(copy));
            trigger.triggers.Add(bd);

            var dd = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dd.callback.AddListener(evt => OnDragIcon((PointerEventData)evt));
            trigger.triggers.Add(dd);

            var ed = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            ed.callback.AddListener(evt => EndDragIcon((PointerEventData)evt));
            trigger.triggers.Add(ed);


            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(evt => ShowTooltipForSlot(copy));
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(evt => HideTooltip());
            trigger.triggers.Add(exit);
        }
    }


    public void ShowTooltipForSlot(int slotIndex)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        PlantSlot slot = SeedInventory.Instance?.GetPlantSlot(slotIndex);
        if (slot == null || slot.seedCount <= 0)
        {
            HideTooltip();
            return;
        }

        PlantDataSO data = slot.data;
        if (data == null)
        {
            HideTooltip();
            return;
        }


        tooltipPanel.SetActive(true);
        tooltipPanel.transform.position = Input.mousePosition + new Vector3(100f, -50f);

        tooltipText.text = $"<b>{data.plantName}</b>\n<size=80%>{data.description}\nDías para crecer: {data.daysToGrow}</size>";
    }





    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void UpdateTooltipPosition()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
            tooltipPanel.transform.position = Input.mousePosition + (Vector3)tooltipOffset;
    }

    private GameObject _dragIcon;
    private int _dragSourceIndex;

    public void BeginDragIcon(int slotIndex)
    {
        _dragSourceIndex = slotIndex;

        _dragIcon = new GameObject("DragIcon");
        var rt = _dragIcon.AddComponent<RectTransform>();
        rt.SetParent(transform.root, false);
        rt.sizeDelta = slotObjects[slotIndex].GetComponent<RectTransform>().sizeDelta;

        var img = _dragIcon.AddComponent<UnityEngine.UI.Image>();
        img.raycastTarget = false;
        img.sprite = slotIcons[slotIndex].sprite;

        slotIcons[slotIndex].gameObject.SetActive(false);

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem ability)
        {
            ability.SetAbility(PlayerAbility.Planting);
        }
    }

    public void OnDragIcon(PointerEventData data)
    {
        if (_dragIcon == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            data.position,
            data.pressEventCamera,
            out Vector2 localPos);
        (_dragIcon.transform as RectTransform).anchoredPosition = localPos;
    }


    public void EndDragIcon(PointerEventData data)
    {
        if (_dragIcon != null)
            Destroy(_dragIcon);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        int hitIndex = -1;
        foreach (var r in results)
        {
            for (int i = 0; i < slotObjects.Length; i++)
            {
                if (r.gameObject == slotObjects[i] ||
                    r.gameObject.transform.IsChildOf(slotObjects[i].transform))
                {
                    hitIndex = i;
                    break;
                }
            }
            if (hitIndex != -1) break;
        }

        if (hitIndex == _dragSourceIndex)
        {
            if (slotIcons[_dragSourceIndex] != null)
                slotIcons[_dragSourceIndex].gameObject.SetActive(true);

            _dragSourceIndex = -1;
            return;
        }

        if (hitIndex >= 0 && hitIndex != _dragSourceIndex)
        {
            SwapSeedSlots(_dragSourceIndex, hitIndex);

            var rt = seedSlots.GetComponent<RectTransform>();
            if (rt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            InitializeSeedSlotsUI();

            SeedInventory.Instance.SelectSlot(hitIndex);
            UpdateSelectedSlotUI(hitIndex);
        }
        else
        {
            if (slotIcons[_dragSourceIndex] != null)
                slotIcons[_dragSourceIndex].gameObject.SetActive(true);
        }

        UpdateTooltipHandlers();
        _dragSourceIndex = -1;
    }






    private void OnSlotKeyPressed(int i)
    {
        float now = Time.time;

        if (pendingSwapSlot >= 0)
        {
            int first = pendingSwapSlot;
            int second = i;

            if (first == second)
            {
                HighlightSlot(first, false);
                pendingSwapSlot = -1;
                Debug.Log("Intercambio cancelado.");
                return;
            }

            SwapSeedSlots(first, second);
            HighlightSlot(first, false);
            pendingSwapSlot = -1;

            InitializeSeedSlotsUI();
            UpdateSelectedSlotUI(SeedInventory.Instance.GetSelectedSlotIndex());

            Debug.Log($"Slots intercambiados: {first + 1} ↔ {second + 1}");
            return;
        }

        if (i == lastPressSlot && (now - lastPressTime) < doublePressThreshold)
        {
            pendingSwapSlot = i;
            HighlightSlot(i, true);
            Debug.Log($"Modo intercambio ACTIVADO para slot {i + 1}. Elige otro (1–5).");
        }
        else
        {
            SeedInventory.Instance.SelectSlot(i);
            UpdateSelectedSlotUI(i);
            //Debug.Log($"Slot {i + 1} seleccionado (semilla activa).");
        }

        lastPressSlot = i;
        lastPressTime = now;
    }
    
    private void HighlightSlot(int idx, bool highlight)
    {
        if (idx < 0 || idx >= slotObjects.Length) return;

        if (highlight)
        {
            slotBackgrounds[idx].color = selectedColor;
            slotObjects[idx].transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
        }
        else
        {
            slotBackgrounds[idx].color = normalColor;
            slotObjects[idx].transform.localScale = new Vector3(normalScale, normalScale, 1f);
        }
    }

    private void SwapSeedSlots(int a, int b)
    {
        PlantSlot slotA = SeedInventory.Instance.GetPlantSlot(a);
        PlantSlot slotB = SeedInventory.Instance.GetPlantSlot(b);

        if (slotA == null || slotB == null) return;

        SeedsEnum tmpSeedType = slotA.seedType;
        GameObject tmpPrefab = slotA.plantPrefab;
        Sprite tmpIcon = slotA.plantIcon;
        int tmpCount = slotA.seedCount;
        int tmpDaysToGrow = slotA.daysToGrow;
        string tmpDescription = slotA.description;
        PlantDataSO tmpData = slotA.data;

        slotA.seedType = slotB.seedType;
        slotA.plantPrefab = slotB.plantPrefab;
        slotA.plantIcon = slotB.plantIcon;
        slotA.seedCount = slotB.seedCount;
        slotA.daysToGrow = slotB.daysToGrow;
        slotA.description = slotB.description;
        slotA.data = slotB.data;

        slotB.seedType = tmpSeedType;
        slotB.plantPrefab = tmpPrefab;
        slotB.plantIcon = tmpIcon;
        slotB.seedCount = tmpCount;
        slotB.daysToGrow = tmpDaysToGrow;
        slotB.description = tmpDescription;
        slotB.data = tmpData;
    }


    public void AnimateRespawnRecovery(float duration)
    {
        if (playerLife == null || manaSystem == null) return;
        StartCoroutine(AnimateBarsCoroutine(duration));
    }

    private IEnumerator AnimateBarsCoroutine(float duration)
    {
        float time = 0f;

        float startHealth = 0f;
        float endHealth = playerLife.maxHealth;

        float startMana = 0f;
        float endMana = manaSystem.GetMaxMana();

        playerLife.currentHealth = 0f;
        manaSystem.SetMana(0f);
        while (time < duration)
        {
            float t = time / duration;

            playerLife.currentHealth = Mathf.Lerp(startHealth, endHealth, t);
            manaSystem.SetMana(Mathf.Lerp(startMana, endMana, t));

            UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
            UpdateManaUI();

            time += Time.deltaTime;
            yield return null;
        }

        playerLife.currentHealth = endHealth;
        manaSystem.SetMana(endMana);

        UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
        UpdateManaUI();
    }

    private void OnSlotClicked(int slotIndex)
    {
        SeedInventory.Instance.SelectSlot(slotIndex);
        UpdateSelectedSlotUI(slotIndex);

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem ability)
        {
            ability.SetAbility(PlayerAbility.Planting);
        }

        //Debug.Log($"Slot {slotIndex + 1} clicked → switched to Planting");
    }


    private void UpdateTooltipHandlers()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            var tooltip = slotObjects[i]?.GetComponent<PlantSlotTooltipHandler>();
            if (tooltip != null)
            {
                tooltip.slotIndex = i;
            }
        }
    }

    private void UpdateWhiteBar()
    {
        if (whiteFillImage == null || fillImage == null) return;

        float current = whiteFillImage.fillAmount;
        float target = fillImage.fillAmount;

        if (current > target)
        {
            whiteFillImage.fillAmount = Mathf.MoveTowards(current, target, whiteBarSpeed * Time.deltaTime);
        }
        else
        {
            whiteFillImage.fillAmount = target;
        }
    }

}
