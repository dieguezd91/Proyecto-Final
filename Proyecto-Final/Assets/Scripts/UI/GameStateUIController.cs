using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Instructions")]
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button continueButton;

    private bool isInstructionsOpen = false;
    private bool openedFromPauseMenu = false;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.Day;
    private PlayerController playerController;
    private PauseMenu pauseMenu;

    public bool IsInstructionsOpen => isInstructionsOpen;

    protected override void CacheReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    protected override void SetupEventListeners()
    {
        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OpenInstructions);

        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(CloseInstructions);

        if (continueButton != null && pauseMenu != null)
            continueButton.onClick.AddListener(pauseMenu.Continue);

        UIEvents.OnInstructionsRequested += OpenInstructions;
    }

    protected override void ConfigureInitialState()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            isInstructionsOpen = false;
        }

        if (LevelManager.Instance != null)
            lastGameState = LevelManager.Instance.currentGameState;

        UpdateUIElementsVisibility();
    }

    public override void HandleUpdate()
    {
        CheckGameStateChanges();
        HandleGameOverState();
    }

    public void OnGameStateChanged(GameState newState)
    {
        UpdateUIElementsVisibility();
        UpdateAbilityUIVisibility();
        lastGameState = newState;
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

    private void UpdateAbilityUIVisibility()
    {
        if (abilityPanel == null || LevelManager.Instance == null) return;

        GameState state = LevelManager.Instance.currentGameState;
        bool showAbilities = state != GameState.Night &&
                             state != GameState.OnInventory &&
                             state != GameState.OnCrafting &&
                             state != GameState.OnAltarRestoration &&
                             state != GameState.Paused &&
                             state != GameState.GameOver;

        abilityPanel.SetActive(showAbilities && !isInstructionsOpen);
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

    private void RestoreFromNormalGameplay()
    {
        SetUIElementsVisibility(true);
        UpdateUIElementsVisibility();

        if (playerController != null)
            playerController.SetMovementEnabled(true);

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            LevelManager.Instance.SetGameState(lastState);

        GameManager.Instance?.ResumeGame();
    }

    private void SetUIElementsVisibility(bool visible)
    {
        if (HUD != null) HUD.SetActive(visible);
        if (seedSlots != null) seedSlots.SetActive(visible);
        if (dayControlPanel != null) dayControlPanel.SetActive(visible);

        if (!visible && UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.CloseInventory();
        }
    }

    protected override void CleanupEventListeners()
    {
        if (instructionsButton != null)
            instructionsButton.onClick.RemoveListener(OpenInstructions);

        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.RemoveListener(CloseInstructions);

        if (continueButton != null && pauseMenu != null)
            continueButton.onClick.RemoveListener(pauseMenu.Continue);

        UIEvents.OnInstructionsRequested -= OpenInstructions;
    }
}