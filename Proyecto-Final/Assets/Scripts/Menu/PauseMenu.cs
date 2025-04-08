using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private UIManager uiManager;

    private bool isPaused = false;
    public static bool isGamePaused = false;
    private GameState lastState = GameState.Day;

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsInstructionsPanelActive() && GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.GameOver)
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private bool IsInstructionsPanelActive()
    {
        if (uiManager != null)
        {
            return uiManager.IsInstructionsOpen();
        }
        return instructionsPanel != null && instructionsPanel.activeSelf;
    }

    public void Pause()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameState.Paused)
        {
            lastState = GameManager.Instance.currentGameState;
        }

        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        isGamePaused = true;

        if (uiManager != null && uiManager.HUD != null)
        {
            uiManager.HUD.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Paused);
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        isGamePaused = false;

        if (uiManager != null && uiManager.HUD != null && !uiManager.IsInstructionsOpen())
        {
            uiManager.HUD.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(lastState);
        }
    }

    public void ShowInstructions()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (uiManager != null)
        {
            uiManager.OpenInstructions();
        }
        else if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void CloseInstructions()
    {
        if (uiManager != null)
        {
            uiManager.CloseInstructions();
        }
        else if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (isGamePaused && pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;

            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Paused)
            {
                GameManager.Instance.SetGameState(lastState);
            }
        }
    }

    public void Close()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        isGamePaused = false;
        SceneManager.LoadScene("MenuScene");
    }
}