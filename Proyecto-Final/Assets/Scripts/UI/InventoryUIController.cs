using UnityEngine;

public class InventoryUIController : UIControllerBase
{
    private void Awake()
    {
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>();
    }
    [SerializeField] private PauseController pauseController;
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

    public bool IsInventoryOpen => UIManager.Instance?.Flow != null && UIManager.Instance.Flow.IsOpen(UIModal.Inventory);

    protected override void CacheReferences()
    {
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
        if (LevelManager.Instance != null && (pauseController != null && pauseController.IsPaused))
            return;

        if (Input.GetKeyDown(toggleInventoryKey) || Input.GetKeyDown(alternateToggleKey))
        {
            UIManager.Instance?.Tooltip?.ForceHide();
            ToggleInventory();
        }

        if (closeInventoryOnEscape && Input.GetKeyDown(KeyCode.Escape) && IsInventoryOpen && !InputConsumptionManager.IsEscapeConsumed)
        {
            if (animationController != null && animationController.IsAnimating)
            {
                return;
            }

            InputConsumptionManager.ConsumeEscape();
            UIManager.Instance?.Tooltip?.ForceHide();
            CloseInventory();
        }
    }

    public void ToggleInventory()
    {
        if (IsInventoryOpen)
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
        TutorialEvents.InvokeInventoryOpened();
        

        UIManager.Instance?.Flow?.Open(UIModal.Inventory);

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
        if (inventoryPanel == null || !inventoryPanel.activeSelf) return;

        if (animationController != null && animationController.IsAnimating)
        {
            return;
        }

        if (waitForAnimationToComplete && animationController != null && animationController.gameObject.activeInHierarchy)
        {
            animationController.CloseBook();
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
        

        if (pauseController != null) pauseController.Resume();

        UIManager.Instance?.Flow?.Close(UIModal.Inventory);

        if (UIManager.Instance != null && UIManager.Instance.HUD != null) UIManager.Instance.HUD.SetActive(true);

        // Always notify listeners that inventory has closed to avoid missed events
        UIEvents.TriggerInventoryClosed();
    }

    private bool CanOpenInventory()
    {
        var state = GameFlowController.Instance.CurrentPhase;
        return state != GamePhase.Night &&
               (pauseController == null || !pauseController.IsPaused) &&
               state != GamePhase.GameOver ;
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

        inventoryPanel.SetActive(true); UIManager.Instance?.Flow?.Open(UIModal.Inventory); if (animationController != null)
        {
            animationController.OpenWithPage(pageName);
        }

        UIEvents.TriggerInventoryOpened();
    }

    public bool IsAnimating => animationController != null && animationController.IsAnimating;
}








