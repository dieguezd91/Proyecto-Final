using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Module Components")]
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private HealthUIController healthUI;
    [SerializeField] private ManaUIController manaUI;
    [SerializeField] private InventoryUIController inventoryUIController;
    [SerializeField] private SeedSlotsUIController seedSlotsUI;
    [SerializeField] private GameStateUIController gameStateUI;
    [SerializeField] private FeedbackUIController feedbackUI;
    [SerializeField] private TooltipUIController tooltipUI;

    [Header("Sound")]
    [SerializeField] private InterfaceSoundBase interfaceSounds;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject HUD;
    public GameObject pausePanel;
    public InventoryUI inventoryUI;


    public InterfaceSoundBase InterfaceSounds => interfaceSounds;
    public static UIManager Instance { get; private set; }

    public PauseMenuController PauseMenu => pauseMenuController;
    public HealthUIController Health => healthUI;
    public ManaUIController Mana => manaUI;
    public InventoryUIController Inventory => inventoryUIController;
    public SeedSlotsUIController SeedSlots => seedSlotsUI;
    public GameStateUIController GameState => gameStateUI;
    public FeedbackUIController Feedback => feedbackUI;
    public TooltipUIController Tooltip => tooltipUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeModules();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupModules();
        RegisterGlobalEvents();
    }

    private void Update()
    {
        pauseMenuController?.HandleUpdate();
        gameStateUI?.HandleUpdate();
        tooltipUI?.HandleUpdate();
        feedbackUI?.HandleUpdate();
        inventoryUIController?.HandleUpdate();
        seedSlotsUI?.HandleUpdate();
    }


    private void InitializeModules()
    {
        if (pauseMenuController == null) pauseMenuController = GetComponentInChildren<PauseMenuController>();
        if (healthUI == null) healthUI = GetComponentInChildren<HealthUIController>();
        if (manaUI == null) manaUI = GetComponentInChildren<ManaUIController>();
        if (inventoryUIController == null) inventoryUIController = GetComponentInChildren<InventoryUIController>();
        if (inventoryUI == null) inventoryUI = GetComponentInChildren<InventoryUI>();
        if (seedSlotsUI == null) seedSlotsUI = GetComponentInChildren<SeedSlotsUIController>();
        if (gameStateUI == null) gameStateUI = GetComponentInChildren<GameStateUIController>();
        if (feedbackUI == null) feedbackUI = GetComponentInChildren<FeedbackUIController>();
        if (tooltipUI == null) tooltipUI = GetComponentInChildren<TooltipUIController>();

        pauseMenuController?.Initialize();
        healthUI?.Initialize();
        manaUI?.Initialize();
        inventoryUIController?.Initialize();
        seedSlotsUI?.Initialize();
        gameStateUI?.Initialize();
        feedbackUI?.Initialize();
        tooltipUI?.Initialize();
    }

    public void OpenInventoryOptions()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OpenOptionsTab();
        }
        else
        {
            Debug.LogWarning("[UIManager] OpenInventoryOptions: inventoryUI referencia nula.");
        }
    }


    private void SetupModules()
    {
        pauseMenuController?.Setup();
        healthUI?.Setup();
        manaUI?.Setup();
        inventoryUIController?.Setup();
        seedSlotsUI?.Setup();
        gameStateUI?.Setup();
        feedbackUI?.Setup();
        tooltipUI?.Setup();
    }

    private void RegisterGlobalEvents()
    {
        UIEvents.OnPlayerHealthChanged += Health.UpdatePlayerHealth;
        UIEvents.OnHomeHealthChanged += Health.UpdateHomeHealth;

        UIEvents.OnManaChanged += Mana.UpdateMana;

        UIEvents.OnInventoryToggleRequested += Inventory.ToggleInventory;

        UIEvents.OnGameStateChanged += GameState.OnGameStateChanged;

        UIEvents.OnPlayerDamaged += Feedback.ShowDamageEffect;
    }

    private void OnDestroy()
    {
        if (Health != null) UIEvents.OnPlayerHealthChanged -= Health.UpdatePlayerHealth;
        if (Health != null) UIEvents.OnHomeHealthChanged -= Health.UpdateHomeHealth;
        if (Mana != null) UIEvents.OnManaChanged -= Mana.UpdateMana;
        if (Inventory != null) UIEvents.OnInventoryToggleRequested -= Inventory.ToggleInventory;
        if (GameState != null) UIEvents.OnGameStateChanged -= GameState.OnGameStateChanged;
        if (Feedback != null) UIEvents.OnPlayerDamaged -= Feedback.ShowDamageEffect;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (Health != null)
            Health.UpdatePlayerHealth(currentHealth, maxHealth);
    }

    public void UpdateHomeHealthBar(float currentHealth, float maxHealth)
    {
        if (Health != null)
            Health.UpdateHomeHealth(currentHealth, maxHealth);
    }

    public void UpdateManaUI()
    {
        if (Mana != null)
            Mana.UpdateMana();
    }

    public void OpenInventory()
    {
        if (Inventory != null)
            Inventory.OpenInventory();
    }

    public void CloseInventory()
    {
        if (Inventory != null)
            Inventory.CloseInventory();
    }

    public void ToggleInventory()
    {
        if (Inventory != null)
            Inventory.ToggleInventory();
    }

    public bool IsInventoryOpen()
    {
        return Inventory != null ? Inventory.IsInventoryOpen : false;
    }

    public void OpenInstructions()
    {
        if (GameState != null)
            GameState.OpenInstructions();
    }

    public void CloseInstructions()
    {
        if (GameState != null)
            GameState.CloseInstructions();
    }

    public bool IsInstructionsOpen()
    {
        return GameState != null ? GameState.IsInstructionsOpen : false;
    }

    public void InitializeSeedSlotsUI()
    {
        if (SeedSlots != null)
            SeedSlots.InitializeSlots();
    }

    public void UpdateSeedCountsUI()
    {
        if (SeedSlots != null)
            SeedSlots.UpdateSeedCounts();
    }

    public void ShowTooltipForSlot(int slotIndex)
    {
        if (Tooltip != null)
            Tooltip.ShowSlotTooltip(slotIndex);
    }

    public void HideTooltip()
    {
        if (Tooltip != null)
            Tooltip.HideTooltip();
    }

    public void AnimateRespawnRecovery(float duration)
        => StartCoroutine(AnimateRespawnCoroutine(duration));

    public void SetGrayscaleGhostEffect(bool enabled)
    {
        if (Feedback != null)
            Feedback.SetGrayscaleEffect(enabled);
    }

    public void ShowRitualOverlay()
    {
        if (Feedback != null)
            Feedback.ShowRitualOverlay();
    }

    public void HideRitualOverlay()
    {
        if (Feedback != null)
            Feedback.HideRitualOverlay();
    }

    private IEnumerator AnimateRespawnCoroutine(float duration)
    {
        if (Health != null)
            yield return Health.AnimateRespawnRecovery(duration);
        if (Mana != null)
            yield return Mana.AnimateRespawnRecovery(duration);
    }
}