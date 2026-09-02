using UnityEngine;
using DG.Tweening;

public class GameStateUIController : UIControllerBase
{

    private void Awake()
    {
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>();
    }
    [SerializeField] private PauseController pauseController;
    [Header("UI Elements")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject dayControlPanel;
    [SerializeField] private GameObject abilityPanel;

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

        if (GameFlowController.Instance != null)
            GameFlowController.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnInventoryClosed -= HandleInventoryClosedEvent;
        UIEvents.OnInventoryOpened -= HandleInventoryOpenedEvent;

        if (worldTransition != null)
        {
            worldTransition.OnStateChanged -= OnWorldStateChanged;
        }

        if (GameFlowController.Instance != null)
            GameFlowController.Instance.OnPhaseChanged -= OnPhaseChanged;
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

        if (GameFlowController.Instance != null)
        {
            OnPhaseChanged(GameFlowController.Instance.CurrentPhase);
        }
        else
        {
            UpdateUIElementsVisibility();
            UpdateAbilityUIVisibility();
        }
    }

    public override void HandleUpdate()
    {
        HandleGameOverState();
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

    public void OnPhaseChanged(GamePhase newPhase)
    {
        UpdateUIElementsVisibility();
        UpdateAbilityUIVisibility();
    }

    private void UpdateAbilityUIVisibility()
    {
        if (abilityPanel == null || GameFlowController.Instance == null)
        {
            return;
        }

        GamePhase phase = GameFlowController.Instance.CurrentPhase;

        bool isInInterior = worldTransition != null && worldTransition.IsInInterior;

        bool showAbilities = phase != GamePhase.Night &&
                             phase != GamePhase.GameOver &&
                             !isInInterior;

        abilityPanel.SetActive(showAbilities);
    }


    private void HandleGameOverState()
    {
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy && HUD != null)
            HUD.SetActive(false);
    }

    private void UpdateUIElementsVisibility()
    {
        if (GameFlowController.Instance == null) return;

        GamePhase currentPhase = GameFlowController.Instance.CurrentPhase;

        bool showHUD = IsGameplayPhase(currentPhase);
        if (HUD != null)
            HUD.SetActive(showHUD);

        bool showDayControls = IsActiveGameplayPhase(currentPhase);
        if (dayControlPanel != null)
            dayControlPanel.SetActive(showDayControls);
    }

    private bool IsGameplayPhase(GamePhase phase)
    {
        return phase == GamePhase.Day ||
               phase == GamePhase.Night;
    }

    private bool IsActiveGameplayPhase(GamePhase phase)
    {
        return phase == GamePhase.Day;
    }

    public void RestoreFromNormalGameplay()
    {
        SetUIElementsVisibility(true);
        UpdateUIElementsVisibility();

        if (pauseController != null) pauseController.Resume();
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
                 // Ensure final value is set and update hiding phase if requested.
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

        // When inventory closes, resume tutorial if it was paused by the menu
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ResumeTutorial();
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

        // Pause the tutorial when the inventory opens so it hides and can be resumed on close
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
        {
            TutorialManager.Instance.PauseTutorial();
        }
    }
}




