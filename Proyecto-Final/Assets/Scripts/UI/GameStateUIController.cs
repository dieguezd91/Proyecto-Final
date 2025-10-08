using System.Collections;
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

    private bool isInstructionsOpen = false;
    private bool openedFromPauseMenu = false;
    private GameState lastGameState = GameState.None;
    private GameState lastState = GameState.None;
    private PlayerController playerController;
    [SerializeField] private PauseMenuController _pauseMenuController;

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

        if (UIManager.Instance != null && UIManager.Instance.IsInventoryOpen())
        {
            if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            {
                ResumeGame();
                return;
            }

            UIManager.Instance.CloseInventory();
            if (playerController != null) playerController.SetMovementEnabled(true);
            if (HUD != null && !isInstructionsOpen) HUD.SetActive(true);
            return;
        }

        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
        {
            ResumeGame();
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

        UIManager.Instance?.OpenInventoryOptions();
        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.GameInventoryBookOpen);
        GameManager.Instance?.PauseGame();
        LevelManager.Instance?.SetGameState(GameState.Paused);

        if (HUD != null) HUD.SetActive(false);
        if (playerController != null) playerController.SetMovementEnabled(false);
    }

    private void CloseInventoryAndResume()
    {
        UIManager.Instance?.CloseInventory();

        if (playerController != null) playerController.SetMovementEnabled(true);
        if (HUD != null && !isInstructionsOpen) HUD.SetActive(true);

        GameManager.Instance?.ResumeGame();
        if (LevelManager.Instance != null && lastState != GameState.None)
            LevelManager.Instance.SetGameState(lastState);
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

    public void PauseGame()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState != GameState.Paused)
            lastState = LevelManager.Instance.currentGameState;

        GameManager.Instance?.PauseGame();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (_pauseMenuController != null)
        {
            _pauseMenuController.Show();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseAll();
            UIManager.Instance.InterfaceSounds.PlaySound(InterfaceSoundType.GamePauseOpen);
        }

        if (HUD != null)
            HUD.SetActive(false);

        LevelManager.Instance?.SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        UIManager.Instance?.CloseInventory();

        if (_pauseMenuController != null)
            _pauseMenuController.Hide();

        GameManager.Instance?.ResumeGame();

        UIManager.Instance?.inventoryUI?.HideInventory();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeAll();
            UIManager.Instance.InterfaceSounds.PlaySound(InterfaceSoundType.GamePauseClose);
        }

        if (HUD != null && !isInstructionsOpen)
            HUD.SetActive(true);

        if (LevelManager.Instance != null && lastState != GameState.None)
            LevelManager.Instance.SetGameState(lastState);
    }
}
