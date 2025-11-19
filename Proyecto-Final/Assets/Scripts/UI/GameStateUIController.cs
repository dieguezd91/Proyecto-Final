using UnityEngine;
using DG.Tweening;

public class GameStateUIController : UIControllerBase
{
    [Header("UI Elements")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject dayControlPanel;
    [SerializeField] private GameObject abilityPanel;

    private bool openedFromPauseMenu = false;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.None;
    [SerializeField] private PauseMenuController _pauseMenuController;

    [Header("Blur Settings")]
    [SerializeField] private UnityEngine.Rendering.Volume blurVolume;
    [SerializeField] private float showTransitionDuration = 0.08f;
    [SerializeField] private float hideTransitionDuration = 0.8f;
    [SerializeField] private float maxFocalLength = 200f;

    private UnityEngine.Rendering.Universal.DepthOfField dof;
    private Tween blurTween;
    private bool isHiding = false;
    private bool wasInventoryOpen = false;

    private WorldTransitionAnimator worldTransition;

    public GameState LastState => lastState;

    protected override void CacheReferences()
    {
        if (worldTransition == null)
        {
            worldTransition = FindObjectOfType<WorldTransitionAnimator>();
        }
    }

    protected override void SetupEventListeners()
    {
        UIEvents.OnInventoryClosed += HandleInventoryClosedEvent;
        UIEvents.OnInventoryOpened += HandleInventoryOpenedEvent;

        if (worldTransition != null)
        {
            worldTransition.OnStateChanged += OnWorldStateChanged;
        }
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnInventoryClosed -= HandleInventoryClosedEvent;
        UIEvents.OnInventoryOpened -= HandleInventoryOpenedEvent;

        if (worldTransition != null)
        {
            worldTransition.OnStateChanged -= OnWorldStateChanged;
        }
    }

    private void OnWorldStateChanged(WorldState newWorldState)
    {
        UpdateAbilityUIVisibility();
    }

    protected override void ConfigureInitialState()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

        if (blurVolume != null && blurVolume.profile.TryGet(out dof))
        {
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 0f;
        }

        if (LevelManager.Instance != null)
        {
            lastGameState = LevelManager.Instance.currentGameState;
            lastState = lastGameState;
        }
        UpdateUIElementsVisibility();

        UpdateAbilityUIVisibility();
    }

    public override void HandleUpdate()
    {
        CheckGameStateChanges();
        HandleGameOverState();
        HandlePauseInput();
        CheckInventoryOpenState();
    }

    private void CheckInventoryOpenState()
    {
        if (UIManager.Instance == null) return;

        bool isOpen = UIManager.Instance.IsInventoryOpen();

        if (isOpen && !wasInventoryOpen)
        {
            if (blurTween != null) { blurTween.Kill(); blurTween = null; }
            if (dof != null)
            {
                isHiding = false;
                blurTween = CreateFocalLengthTween(maxFocalLength, showTransitionDuration, false);
            }
        }
        else if (!isOpen && wasInventoryOpen)
        {
            if (dof != null && dof.focalLength.value > 0)
            {
                if (blurTween != null) { blurTween.Kill(); blurTween = null; }
                isHiding = true;
                blurTween = CreateFocalLengthTween(0f, hideTransitionDuration, true);
            }
        }

        wasInventoryOpen = isOpen;
    }

    private void CheckGameStateChanges()
    {
        if (LevelManager.Instance == null) return;

        if (LevelManager.Instance.currentGameState != lastGameState)
        {
            if (LevelManager.Instance.currentGameState == GameState.Paused &&
                lastGameState != GameState.Paused &&
                lastGameState != GameState.None)
            {
                lastState = lastGameState;
            }

            OnGameStateChanged(LevelManager.Instance.currentGameState);
        }
    }

    public void OnGameStateChanged(GameState newState)
    {
        UpdateUIElementsVisibility();
        UpdateAbilityUIVisibility();
        lastGameState = newState;
    }

    private void UpdateAbilityUIVisibility()
    {
        if (abilityPanel == null || LevelManager.Instance == null)
        {
            return;
        }

        GameState state = LevelManager.Instance.currentGameState;

        bool isInInterior = worldTransition != null && worldTransition.IsInInterior;

        bool showAbilities = state != GameState.Night &&
                             state != GameState.OnCrafting &&
                             state != GameState.OnAltarRestoration &&
                             state != GameState.GameOver &&
                             !isInInterior;

        abilityPanel.SetActive(showAbilities);
    }

    private void HandlePauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        UIManager.Instance?.Tooltip?.ForceHide();


        if (UIManager.Instance != null && UIManager.Instance.Inventory != null)
        {
            if (UIManager.Instance.Inventory.IsAnimating)
            {
                return;
            }
        }

        if (IsAnyGameplayUIOpen())
        {
            return;
        }

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
        {
            if (UIManager.Instance != null && UIManager.Instance.IsInventoryOpen())
            {
                ResumeGame();
            }
            return;
        }

        if (UIManager.Instance != null && UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.CloseInventory();
            if (HUD != null) HUD.SetActive(true);
            return;
        }

        OpenInventoryOptions();
    }

    private bool IsAnyGameplayUIOpen()
    {
        if (LevelManager.Instance == null) return false;

        GameState currentState = LevelManager.Instance.currentGameState;

        return currentState == GameState.OnCrafting ||
               currentState == GameState.OnAltarRestoration ||
               currentState == GameState.OnRitual;
    }

    private void OpenInventoryOptions()
    {
        bool canOpen = !CraftingUIManager.isCraftingUIOpen &&
                       !RestorationAltarUIManager.isUIOpen &&
                       LevelManager.Instance != null &&
                       LevelManager.Instance.currentGameState != GameState.GameOver &&
                       LevelManager.Instance.currentGameState != GameState.OnAltarRestoration &&
                       LevelManager.Instance.currentGameState != GameState.OnRitual;

        if (!canOpen) return;

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState != GameState.Paused)
            lastState = LevelManager.Instance.currentGameState;

        UIManager.Instance?.OpenInventoryWithPage("Options");

        GameManager.Instance?.PauseGame();
        LevelManager.Instance?.SetGameState(GameState.Paused);

        if (blurTween != null) { blurTween.Kill(); blurTween = null; }
        if (dof != null)
            blurTween = CreateFocalLengthTween(maxFocalLength, showTransitionDuration, false);

        if (HUD != null) HUD.SetActive(false);
    }

    private void HandleGameOverState()
    {
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy && HUD != null)
            HUD.SetActive(false);
    }

    private void UpdateUIElementsVisibility()
    {
        if (LevelManager.Instance == null) return;

        GameState currentState = LevelManager.Instance.currentGameState;

        bool showHUD = IsGameplayState(currentState);
        if (HUD != null)
            HUD.SetActive(showHUD);

        bool showDayControls = IsActiveGameplayState(currentState);
        if (dayControlPanel != null)
            dayControlPanel.SetActive(showDayControls);
    }

    private bool IsGameplayState(GameState state)
    {
        return state == GameState.Day ||
               state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing ||
               state == GameState.Night;
    }

    private bool IsActiveGameplayState(GameState state)
    {
        return state == GameState.Day ||
               state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing;
    }

    public void RestoreFromNormalGameplay()
    {
        SetUIElementsVisibility(true);
        UpdateUIElementsVisibility();

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            LevelManager.Instance.SetGameState(lastState);

        GameManager.Instance?.ResumeGame();
    }

    public void SetUIElementsVisibility(bool visible)
    {
        if (HUD != null) HUD.SetActive(visible);

        if (dayControlPanel != null) dayControlPanel.SetActive(visible);

        if (!visible && UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.CloseInventory();
        }
    }

    public void ResumeGame()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.CloseInventory();
        }

        if (_pauseMenuController != null)
            _pauseMenuController.Hide();

        GameManager.Instance?.ResumeGame();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeAll();
        }

        if (HUD != null)
            HUD.SetActive(true);

        if (LevelManager.Instance != null && lastState != GameState.None)
            LevelManager.Instance.SetGameState(lastState);

        if (blurTween != null) { blurTween.Kill(); blurTween = null; }
        if (dof != null)
        {
            isHiding = true;
            blurTween = CreateFocalLengthTween(0f, hideTransitionDuration, true);
        }
    }

    // Creates and returns a DOTween Tween that animates the DOF focal length using unscaled time.
    // If setIsHidingFalseOnComplete is true, isHiding will be set to false when the tween completes.
    private Tween CreateFocalLengthTween(float target, float duration, bool setIsHidingFalseOnComplete)
    {
        if (dof == null) return null;

        // Animate the focalLength.value property directly using DOTween and unscaled time.
        Tween t = DOTween.To(() => dof.focalLength.value, x => dof.focalLength.value = x, target, duration)
            .SetUpdate(true)
            // use an ease that makes the blur ramp up quickly for a snappy response
            .SetEase(Ease.OutCubic)
             .OnComplete(() =>
             {
                 // Ensure final value is set and update hiding state if requested.
                 if (dof != null) dof.focalLength.value = target;
                 if (setIsHidingFalseOnComplete) isHiding = false;
             });

        return t;
    }

    private void HandleInventoryClosedEvent()
    {
        if (dof != null && dof.focalLength.value > 0)
        {
            if (blurTween != null) { blurTween.Kill(); blurTween = null; }
            isHiding = true;
            blurTween = CreateFocalLengthTween(0f, hideTransitionDuration, true);
        }
    }

    private void HandleInventoryOpenedEvent()
    {
        // Called when inventory opens (including alternate keys like Tab). Start the blur open tween.
        if (blurTween != null) { blurTween.Kill(); blurTween = null; }
        if (dof != null)
        {
            isHiding = false;
            blurTween = CreateFocalLengthTween(maxFocalLength, showTransitionDuration, false);
        }
    }
}