using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : UIControllerBase
{
    [SerializeField] private UIManager uiManager;

    [Header("UI Panels")]
    [SerializeField] private PauseMenuPanel _pauseMenuPanel;
    [SerializeField] private OptionsMenuPanel _optionsMenuPanel;
    [SerializeField] private InstructionsPanel _instructionsPanel;

    [Header("Blur Settings")]
    [SerializeField] private Volume blurVolume;
    [SerializeField] private float showTransitionDuration = 0.5f;
    [SerializeField] private float hideTransitionDuration = 0.8f;
    [SerializeField] private float maxFocalLength = 200f;

    private Coroutine blurTransition;
    private DepthOfField dof;
    private bool isHiding = false;

    private void Start()
    {
        // Ensure the controller is properly initialized
        Initialize();
        Setup();
    }

    protected override void CacheReferences()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    protected override void OnInitialize()
    {
        // Initialize blur effect
        if (blurVolume != null && blurVolume.profile.TryGet(out dof))
        {
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 0f;
        }
    }

    protected override void ConfigureInitialState()
    {

    }

    public override void Show()
    {
        if (_currentState == PanelState.Shown) return;
        gameObject.SetActive(true);
        _currentState = PanelState.Shown;
        ShowPauseMenu();
    }

    public override void Hide()
    {
        if (_currentState == PanelState.Hidden || isHiding || !gameObject.activeInHierarchy) return;
        isHiding = true;
        _currentState = PanelState.Hidden;
        gameObject.SetActive(false);
        isHiding = false;
    }

    protected override void OnShowAnimation()
    {
        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(maxFocalLength, showTransitionDuration));
    }

    protected override void OnHideAnimation()
    {
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
            gameObject.SetActive(false);
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
        gameObject.SetActive(false); // Deactivate after animation
        isHiding = false;
    }

    protected override void SetupEventListeners()
    {
        // Subscribe to PauseMenuPanel events
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.OnContinueClicked.AddListener(Continue);
            _pauseMenuPanel.OnOptionsClicked.AddListener(ShowOptions);
            _pauseMenuPanel.OnMainMenuClicked.AddListener(GoToMainMenu);
            _pauseMenuPanel.OnExitClicked.AddListener(() => GameManager.Instance?.QuitGame());
        }

        // Subscribe to OptionsMenuPanel events
        if (_optionsMenuPanel != null)
        {
            _optionsMenuPanel.OnGoBackClicked.AddListener(HideOptions);
        }
    }

    protected override void CleanupEventListeners()
    {
        // Unsubscribe from PauseMenuPanel events
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.OnContinueClicked.RemoveListener(Continue);
            _pauseMenuPanel.OnOptionsClicked.RemoveListener(ShowOptions);
            _pauseMenuPanel.OnMainMenuClicked.RemoveListener(GoToMainMenu);
            _pauseMenuPanel.OnExitClicked.RemoveListener(() => GameManager.Instance?.QuitGame());
        }

        // Unsubscribe from OptionsMenuPanel events
        if (_optionsMenuPanel != null)
        {
            _optionsMenuPanel.OnGoBackClicked.RemoveListener(HideOptions);
        }
    }

    // UI-only methods that handle button interactions
    public void Continue()
    {
        if (UIManager.Instance != null && UIManager.Instance.GameState != null)
        {
            UIManager.Instance.GameState.ResumeGame();
        }
    }

    // Methods to control just the main pause menu visibility
    public void ShowPauseMenu()
    {
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Show();
        if (_optionsMenuPanel != null)
            _optionsMenuPanel.Hide();
    }

    public void HidePauseMenu()
    {
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Hide();
    }

    // Options menu methods
    public void ShowOptions()
    {
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.Hide();
        }

        if (_optionsMenuPanel != null)
        {
            _optionsMenuPanel.Show();
        }

        SoundManager.Instance.PlayOneShot("ButtonClick");
    }

    public void HideOptions()
    {
        if (_optionsMenuPanel != null)
            _optionsMenuPanel.Hide();
        ShowPauseMenu();
        SoundManager.Instance.PlayOneShot("ButtonClick");
    }

    public void GoToMainMenu()
    {
        GameManager.Instance?.ResumeGame();

        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Hide();

        SoundManager.Instance.PlayOneShot("ButtonClick");
        SceneLoaderManager.Instance.LoadSceneByName("RefactorMenu");
    }
}
