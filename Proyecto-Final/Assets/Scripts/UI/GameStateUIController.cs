using UnityEngine;
using DG.Tweening;

public class GameStateUIController : UIControllerBase
{
    [Header("UI Elements")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject seedSlots;
    [SerializeField] private GameObject dayControlPanel;
    [SerializeField] private GameObject abilityPanel;

    private bool isInstructionsOpen = false;
    private bool openedFromPauseMenu = false;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.None;
    private PlayerController playerController;
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

    public bool IsInstructionsOpen => isInstructionsOpen;
    public GameState LastState => lastState;

    protected override void CacheReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<PlayerController>();
    }

    protected override void SetupEventListeners()
    {
        UIEvents.OnInventoryClosed += HandleInventoryClosedEvent;
        // Ensure blur also activates when inventory is opened via keys (Tab / I)
        UIEvents.OnInventoryOpened += HandleInventoryOpenedEvent;
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnInventoryClosed -= HandleInventoryClosedEvent;
        UIEvents.OnInventoryOpened -= HandleInventoryOpenedEvent;
    }

    protected override void ConfigureInitialState()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (instructionsPanel != null) { instructionsPanel.SetActive(false); isInstructionsOpen = false; }
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
    }

    public override void HandleUpdate()
    {
        CheckGameStateChanges();
        HandleGameOverState();
        HandlePauseInput();
        CheckInventoryOpenState();
    }

    // Polls the UIManager inventory open state so we reliably trigger blur for all input paths (Tab, I, UI calls).
    private void CheckInventoryOpenState()
    {
        if (UIManager.Instance == null) return;

        bool isOpen = UIManager.Instance.IsInventoryOpen();

        if (isOpen && !wasInventoryOpen)
        {
            // Inventory opened — start blur open tween.
            if (blurTween != null) { blurTween.Kill(); blurTween = null; }
            if (dof != null)
            {
                isHiding = false;
                blurTween = CreateFocalLengthTween(maxFocalLength, showTransitionDuration, false);
            }
        }
        else if (!isOpen && wasInventoryOpen)
        {
            // Inventory closed — start blur hide tween.
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
        if (abilityPanel == null || LevelManager.Instance == null) return;

        GameState state = LevelManager.Instance.currentGameState;
        bool showAbilities = state != GameState.Night &&
                             state != GameState.OnCrafting &&
                             state != GameState.OnAltarRestoration &&
                             state != GameState.GameOver;

        abilityPanel.SetActive(showAbilities && !isInstructionsOpen);
    }

    private void HandlePauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (isInstructionsOpen)
        {
            CloseInstructions();
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
            if (playerController != null) playerController.SetMovementEnabled(true);
            if (HUD != null && !isInstructionsOpen) HUD.SetActive(true);
            return;
        }

        OpenInventoryOptions();
    }

    private void OpenInventoryOptions()
    {
        bool canOpen = !isInstructionsOpen &&
                       !CraftingUIManager.isCraftingUIOpen &&
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
        if (playerController != null) playerController.SetMovementEnabled(false);
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
        if (HUD != null && !isInstructionsOpen)
            HUD.SetActive(showHUD);

        bool showGameplayUI = IsActiveGameplayState(currentState);

        if (seedSlots != null)
            seedSlots.SetActive(showGameplayUI && !isInstructionsOpen);
        if (dayControlPanel != null)
            dayControlPanel.SetActive(showGameplayUI && !isInstructionsOpen);
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

    public void OpenInstructions()
    {
        if (instructionsPanel == null) return;

        openedFromPauseMenu = pausePanel != null && pausePanel.activeSelf;

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState != GameState.Paused)
            lastState = LevelManager.Instance.currentGameState;

        SetUIElementsVisibility(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        instructionsPanel.SetActive(true);
        isInstructionsOpen = true;

        SoundManager.Instance?.PlayOneShot("ButtonClick");

        if (LevelManager.Instance != null)
            LevelManager.Instance.SetGameState(GameState.Paused);

        if (playerController != null)
            playerController.SetMovementEnabled(false);
    }

    public void CloseInstructions()
    {
        if (instructionsPanel == null) return;

        instructionsPanel.SetActive(false);
        isInstructionsOpen = false;

        SoundManager.Instance?.PlayOneShot("ButtonClick");

        if (openedFromPauseMenu && pausePanel != null)
        {
            RestoreFromPauseMenu();
        }
        else
        {
            RestoreFromNormalGameplay();
        }
    }

    private void RestoreFromPauseMenu()
    {
        pausePanel.SetActive(true);
        GameManager.Instance?.PauseGame();
        if (HUD != null)
            HUD.SetActive(false);
    }

    public void RestoreFromNormalGameplay()
    {
        SetUIElementsVisibility(true);
        UpdateUIElementsVisibility();

        if (playerController != null)
            playerController.SetMovementEnabled(true);

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            LevelManager.Instance.SetGameState(lastState);

        GameManager.Instance?.ResumeGame();
    }

    public void SetUIElementsVisibility(bool visible)
    {
        if (HUD != null) HUD.SetActive(visible);
        if (seedSlots != null) seedSlots.SetActive(visible);
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

        if (HUD != null && !isInstructionsOpen)
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