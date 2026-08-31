using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : UIControllerBase
{
    [SerializeField] private PauseController pauseController; [SerializeField] private UIManager uiManager;

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

    private void Start() { if (pauseController == null) pauseController = FindObjectOfType<PauseController>(); // Ensure the controller is properly initialized
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

    private void OnEnable()
    {
        ShowPauseMenu();
    }

    public override void Show()
    {
        _currentState = PanelState.Shown;
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        // Visibility is managed entirely by the parent OptionsPage.
        // We only update state, but do NOT disable the gameObject.
        _currentState = PanelState.Hidden;
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
            _pauseMenuPanel.OnSkipButtonClicked.AddListener(HandleSkipTutorialClicked);
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
            _pauseMenuPanel.OnSkipButtonClicked.RemoveListener(HandleSkipTutorialClicked);
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
        if (UIManager.Instance != null && UIManager.Instance.GamePhase != null)
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                TutorialManager.Instance.ResumeTutorial();
            }

            UIManager.Instance.GamePhase.ResumeGame();
        }
    }

    // Methods to control just the main pause menu visibility
    public void ShowPauseMenu()
    {
        if (_pauseMenuPanel != null)
        {
            if (TutorialManager.Instance != null)
            {
                _pauseMenuPanel.SetSkipButtonActive(TutorialManager.Instance.IsTutorialActive());
            }
            else
            {
                _pauseMenuPanel.SetSkipButtonActive(false);
            }

            _pauseMenuPanel.Show();
        }
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
        if (pauseController != null) pauseController.Resume();

        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Hide();

        SoundManager.Instance.PlayOneShot("ButtonClick");
        SceneLoaderManager.Instance.LoadSceneByName("Menu");
    }

    private void HandleSkipTutorialClicked()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.SkipTutorial();
        }

        Continue();
    }
}



