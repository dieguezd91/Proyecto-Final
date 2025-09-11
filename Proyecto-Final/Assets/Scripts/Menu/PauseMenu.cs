using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject OptionsMenu;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private UIManager uiManager;

    [Header("Blur Settings")]
    [SerializeField] private Volume blurVolume;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float maxFocalLength = 200f;

    private GameState lastState = GameState.Day;

    private Coroutine blurTransition;
    private DepthOfField dof;


    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        // buscamos el componente DepthOfField en el VolumeProfile
        if (blurVolume != null && blurVolume.profile.TryGet(out dof))
        {
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 0f; // sin blur al inicio
        }
    }

    void Update()
    {
        if (uiManager != null && uiManager.IsInventoryOpen())
            return;

        if (Input.GetKeyDown(KeyCode.Escape) &&
            !IsInstructionsPanelActive() &&
            !CraftingUIManager.isCraftingUIOpen && !HouseRestorationUIManager.isUIOpen &&
            LevelManager.Instance != null &&
            LevelManager.Instance.currentGameState != GameState.GameOver && LevelManager.Instance.currentGameState != GameState.OnAltarRestoration && LevelManager.Instance.currentGameState != GameState.OnRitual)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
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
        if (LevelManager.Instance != null && LevelManager.Instance.currentGameState != GameState.Paused)
        {
            lastState = LevelManager.Instance.currentGameState;
        }

        GameManager.Instance?.PauseGame();
        pauseMenu.SetActive(true);
        SoundManager.Instance.PauseAll();
        UIManager.Instance.InterfaceSounds.PlaySound(InterfaceSoundType.GamePauseOpen);

        if (uiManager != null && uiManager.HUD != null)
        {
            uiManager.HUD.SetActive(false);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetGameState(GameState.Paused);
        }

        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(maxFocalLength));
    }

    public void Resume()
    {
        GameManager.Instance?.ResumeGame();
        pauseMenu.SetActive(false);
        OptionsMenu.SetActive(false);
        SoundManager.Instance.ResumeAll();
        UIManager.Instance.InterfaceSounds.PlaySound(InterfaceSoundType.GamePauseClose);

        if (uiManager != null && uiManager.HUD != null && !uiManager.IsInstructionsOpen())
        {
            uiManager.HUD.SetActive(true);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetGameState(lastState);
        }

        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(0f));
    }
    
    public void Continue()
    {
        GameManager.Instance?.ResumeGame();
        pauseMenu.SetActive(false);
        OptionsMenu.SetActive(false);
        SoundManager.Instance.PlayOneShot("ButtonClick");

        if (uiManager != null && uiManager.HUD != null && !uiManager.IsInstructionsOpen())
        {
            uiManager.HUD.SetActive(true);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetGameState(lastState);
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
            SoundManager.Instance.PlayOneShot("ButtonClick");
        }

        GameManager.Instance?.PauseGame();
    }

    public void CloseInstructions()
    {
        if (uiManager != null)
        {
            uiManager.CloseInstructions();
        }
        else if (instructionsPanel != null)
        {
            SoundManager.Instance.PlayOneShot("ButtonClick");
            instructionsPanel.SetActive(false);
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused() && pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            GameManager.Instance.PauseGame();
        }
        else
        {
            GameManager.Instance?.ResumeGame();
            if (LevelManager.Instance != null && LevelManager.Instance.currentGameState == GameState.Paused)
            {
                LevelManager.Instance.SetGameState(lastState);
            }
        }
    }

    public void GoToMainMenu()
    {
        GameManager.Instance?.ResumeGame();
        pauseMenu.SetActive(false);
        SoundManager.Instance.PlayOneShot("ButtonClick");
        SceneManager.LoadScene("MenuScene");
    }

    private IEnumerator FadeFocalLength(float target)
    {
        if (dof == null) yield break;

        float start = dof.focalLength.value;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            dof.focalLength.value = Mathf.Lerp(start, target, elapsed / transitionDuration);
            yield return null;
        }

        dof.focalLength.value = target;
    }

}