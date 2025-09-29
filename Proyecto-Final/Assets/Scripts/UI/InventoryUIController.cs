using UnityEngine;

public class InventoryUIController : UIControllerBase
{
    [Header("Inventory Settings")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleInventoryKey = KeyCode.I;
    [SerializeField] private KeyCode alternateToggleKey = KeyCode.Tab;
    [SerializeField] private bool closeInventoryOnEscape = true;
    [SerializeField] private bool disablePlayerMovementWhenOpen = true;
    [SerializeField] private InventoryUI inventoryUI;

    private bool isInventoryOpen = false;
    private PlayerController playerController;

    public bool IsInventoryOpen => isInventoryOpen;

    protected override void CacheReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        if (inventoryPanel == null)
            inventoryPanel = GameObject.FindGameObjectWithTag("InventoryPanel");

        if (inventoryUI == null && inventoryPanel != null)
            inventoryUI = inventoryPanel.GetComponent<InventoryUI>();
    }

    protected override void ConfigureInitialState()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
    }

    protected override void SetupEventListeners()
    {
        UIEvents.OnInventoryToggleRequested += ToggleInventory;
    }

    public override void HandleUpdate()
    {
        HandleInventoryInput();
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
        if (inventoryPanel == null || UIManager.Instance.GameState.IsInstructionsOpen) return;

        if (LevelManager.Instance != null && !CanOpenInventory())
            return;

        inventoryPanel.SetActive(true);
        isInventoryOpen = true;

        if (inventoryUI != null)
        {
            inventoryUI.UpdateAllSlots();
            inventoryUI.ForceRefresh();
        }

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(false);

        LevelManager.Instance?.SetGameState(GameState.OnInventory);
        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.GameInventoryBookOpen);

        UIEvents.TriggerInventoryOpened();
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;

        inventoryPanel.SetActive(false);
        isInventoryOpen = false;

        if (inventoryUI != null)
            inventoryUI.ClearDescriptionPanel();

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(true);

        if (LevelManager.Instance?.GetCurrentGameState() == GameState.OnInventory)
            LevelManager.Instance.SetGameState(GameState.Digging);
        
        if (LevelManager.Instance?.GetCurrentGameState() == GameState.Paused)
            LevelManager.Instance.SetGameState(GameState.Digging);

        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.GameInventoryBookClose);
        GameManager.Instance?.ResumeGame();
        UIManager.Instance.HUD.SetActive(true);
        playerController.SetMovementEnabled(true);
        UIEvents.TriggerInventoryClosed();

    }

    private bool CanOpenInventory()
    {
        var state = LevelManager.Instance.currentGameState;
        return state != GameState.Night &&
               state != GameState.Paused &&
               state != GameState.OnCrafting &&
               state != GameState.GameOver &&
               state != GameState.OnAltarRestoration;
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnInventoryToggleRequested -= ToggleInventory;
    }
}