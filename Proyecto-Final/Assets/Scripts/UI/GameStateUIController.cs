using System.Collections;
using UnityEngine;

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
    [SerializeField] private float showTransitionDuration = 0.5f;
    [SerializeField] private float hideTransitionDuration = 0.8f;
    [SerializeField] private float maxFocalLength = 200f;

    private UnityEngine.Rendering.Universal.DepthOfField dof;
    private Coroutine blurTransition;
    private bool isHiding = false;

    public bool IsInstructionsOpen => isInstructionsOpen;
    public GameState LastState => lastState;

    protected override void CacheReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<PlayerController>();
    }

    protected override void SetupEventListeners() { }

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

        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(maxFocalLength, showTransitionDuration));

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

    protected override void CleanupEventListeners() { }

    public void ResumeGame()
    {
        // Cerrar inventario con animación
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

        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLengthAndDeactivate(0f, hideTransitionDuration));
    }

    private IEnumerator FadeFocalLength(float target, float duration)
    {
        if (dof == null) yield break;

        float start = dof.focalLength.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            dof.focalLength.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        dof.focalLength.value = target;
    }

    private IEnumerator FadeFocalLengthAndDeactivate(float target, float duration)
    {
        if (dof == null)
        {
            isHiding = false;
            yield break;
        }

        float start = dof.focalLength.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            dof.focalLength.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        dof.focalLength.value = target;
        isHiding = false;
    }
}