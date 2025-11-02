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

    [Header("Animation")]
    [SerializeField] private InventoryAnimationController animationController;
    [SerializeField] private bool waitForAnimationToComplete = true;

    [Header("Page Management")]
    [SerializeField] private bool animationControllerManagesPages = true;

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

        if (animationController == null && inventoryPanel != null)
        {
            Transform animatorTransform = inventoryPanel.transform.Find("InventoryAnimator");
            if (animatorTransform != null)
            {
                animationController = animatorTransform.GetComponent<InventoryAnimationController>();
            }

            if (animationController == null)
            {
                animationController = inventoryPanel.GetComponentInChildren<InventoryAnimationController>();
            }
        }
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

        if (animationController != null)
        {
            animationController.OnOpenAnimationComplete += OnInventoryOpenAnimationComplete;
            animationController.OnCloseAnimationComplete += OnInventoryCloseAnimationComplete;
            animationController.OnPageReadyToShow += OnPageReadyToShow;
        }
    }

    public override void HandleUpdate()
    {
        HandleInventoryInput();
    }

    private void HandleInventoryInput()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            return;

        if (Input.GetKeyDown(toggleInventoryKey) || Input.GetKeyDown(alternateToggleKey))
            ToggleInventory();

        if (closeInventoryOnEscape && Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
        {
            if (animationController != null && animationController.IsAnimating)
            {
                return;
            }

            CloseInventory();
        }
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
        if (inventoryPanel == null) return;

        if (LevelManager.Instance != null && !CanOpenInventory())
            return;

        if (animationController != null && animationController.IsAnimating)
        {
            return;
        }

        inventoryPanel.SetActive(true);
        isInventoryOpen = true;

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(false);

        LevelManager.Instance?.SetGameState(GameState.OnInventory);

        UIEvents.TriggerInventoryOpened();
    }

    private void OnPageReadyToShow()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateAllSlots();
            inventoryUI.ForceRefresh();
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;

        if (animationController != null && animationController.IsAnimating)
        {
            return;
        }

        if (waitForAnimationToComplete && animationController != null)
        {
            UIEvents.TriggerInventoryClosed();
        }
        else
        {
            PerformCloseInventory();
        }
    }

    private void PerformCloseInventory()
    {
        if (inventoryPanel == null) return;

        if (inventoryUI != null)
            inventoryUI.ClearDescriptionPanel();

        inventoryPanel.SetActive(false);
        isInventoryOpen = false;

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(true);

        if (LevelManager.Instance != null)
        {
            var uiController = UIManager.Instance?.GameState;
            if (uiController != null)
            {
                LevelManager.Instance.SetGameState(uiController.LastState);
            }
        }

        GameManager.Instance?.ResumeGame();
        UIManager.Instance.HUD.SetActive(true);
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

    private void OnInventoryOpenAnimationComplete()
    {

    }

    private void OnInventoryCloseAnimationComplete()
    {
        PerformCloseInventory();
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnInventoryToggleRequested -= ToggleInventory;

        if (animationController != null)
        {
            animationController.OnOpenAnimationComplete -= OnInventoryOpenAnimationComplete;
            animationController.OnCloseAnimationComplete -= OnInventoryCloseAnimationComplete;
            animationController.OnPageReadyToShow -= OnPageReadyToShow;
        }
    }

    public void OpenInventoryWithPage(string pageName)
    {
        if (inventoryPanel == null) return;

        if (animationController != null && animationController.IsAnimating)
        {
            return;
        }

        inventoryPanel.SetActive(true);
        isInventoryOpen = true;

        if (disablePlayerMovementWhenOpen && playerController != null)
            playerController.SetMovementEnabled(false);

        if (animationController != null)
        {
            animationController.OpenWithPage(pageName);
        }

        UIEvents.TriggerInventoryOpened();
    }

    public bool IsAnimating => animationController != null && animationController.IsAnimating;
}