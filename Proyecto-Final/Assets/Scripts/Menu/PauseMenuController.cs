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
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float maxFocalLength = 200f;

    private Coroutine blurTransition;
    private DepthOfField dof;

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
        Debug.Log($"[PauseMenuController] Show called on {gameObject.name}");
        gameObject.SetActive(true);
        _currentState = PanelState.Shown;
        
        // Show the main pause menu panel by default when controller is shown
        ShowPauseMenu();
            
        OnShowAnimation();
    }

    public override void Hide()
    {
        if (_currentState == PanelState.Hidden) return;
        Debug.Log($"[PauseMenuController] Hide called on {gameObject.name}");
        OnHideAnimation();
            
        gameObject.SetActive(false);
        _currentState = PanelState.Hidden;
    }

    protected override void OnShowAnimation()
    {
        Debug.Log("[PauseMenuController] OnShowAnimation - applying blur effect");
        // Apply blur effect when showing
        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(maxFocalLength));
    }

    protected override void OnHideAnimation()
    {
        Debug.Log("[PauseMenuController] OnHideAnimation - removing blur effect");
        // Remove blur effect when hiding
        if (blurTransition != null) StopCoroutine(blurTransition);
        blurTransition = StartCoroutine(FadeFocalLength(0f));
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

    private bool IsInstructionsPanelActive()
    {
        bool result = false;
        if (uiManager != null)
        {
            result = uiManager.IsInstructionsOpen();
        }
        return result;
    }

    protected override void SetupEventListeners()
    {
        Debug.Log("[PauseMenuController] Setting up event listeners");
        
        // Subscribe to PauseMenuPanel events
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.OnContinueClicked.AddListener(Continue);
            _pauseMenuPanel.OnOptionsClicked.AddListener(ShowOptions);
            _pauseMenuPanel.OnInstructionsClicked.AddListener(ShowInstructions);
            _pauseMenuPanel.OnMainMenuClicked.AddListener(GoToMainMenu);
            _pauseMenuPanel.OnExitClicked.AddListener(() => GameManager.Instance?.QuitGame());
            Debug.Log("[PauseMenuController] Subscribed to PauseMenuPanel events");
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] PauseMenuPanel is null!");
        }

        // Subscribe to OptionsMenuPanel events
        if (_optionsMenuPanel != null)
        {
            _optionsMenuPanel.OnGoBackClicked.AddListener(HideOptions);
            Debug.Log("[PauseMenuController] Subscribed to OptionsMenuPanel events");
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] OptionsMenuPanel is null!");
        }
    }

    protected override void CleanupEventListeners()
    {
        // Unsubscribe from PauseMenuPanel events
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.OnContinueClicked.RemoveListener(Continue);
            _pauseMenuPanel.OnOptionsClicked.RemoveListener(ShowOptions);
            _pauseMenuPanel.OnInstructionsClicked.RemoveListener(ShowInstructions);
            _pauseMenuPanel.OnMainMenuClicked.RemoveListener(GoToMainMenu);
            _pauseMenuPanel.OnExitClicked.RemoveListener(() => GameManager.Instance?.QuitGame());
            Debug.Log("[PauseMenuController] Unsubscribed from PauseMenuPanel events");
        }

        // Unsubscribe from OptionsMenuPanel events
        if (_optionsMenuPanel != null)
        {
            _optionsMenuPanel.OnGoBackClicked.RemoveListener(HideOptions);
            Debug.Log("[PauseMenuController] Unsubscribed from OptionsMenuPanel events");
        }
    }

    // UI-only methods that handle button interactions
    public void Continue()
    {
        Debug.Log("[PauseMenuController] Continue button clicked");
        if (UIManager.Instance != null && UIManager.Instance.GameState != null)
        {
            UIManager.Instance.GameState.ResumeGame();
        }
        else
        {
            Debug.LogError("[PauseMenuController] Cannot resume - UIManager or GameState is null!");
        }
    }

    // Methods to control just the main pause menu visibility
    public void ShowPauseMenu()
    {
        Debug.Log("[PauseMenuController] ShowPauseMenu called");
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Show();
        if (_optionsMenuPanel != null)
            _optionsMenuPanel.Hide();
    }

    public void HidePauseMenu()
    {
        Debug.Log("[PauseMenuController] HidePauseMenu called");
        if (_pauseMenuPanel != null)
            _pauseMenuPanel.Hide();
    }

    // Options menu methods
    public void ShowOptions()
    {
        Debug.Log("[PauseMenuController] ShowOptions called");
        Debug.Log($"[PauseMenuController] _pauseMenuPanel null: {_pauseMenuPanel == null}");
        Debug.Log($"[PauseMenuController] _optionsMenuPanel null: {_optionsMenuPanel == null}");
        
        if (_pauseMenuPanel != null)
        {
            Debug.Log($"[PauseMenuController] Hiding _pauseMenuPanel, current state: {_pauseMenuPanel.CurrentState}");
            _pauseMenuPanel.Hide();
        }
        
        if (_optionsMenuPanel != null)
        {
            Debug.Log($"[PauseMenuController] Showing _optionsMenuPanel, current state: {_optionsMenuPanel.CurrentState}");
            _optionsMenuPanel.Show();
            Debug.Log($"[PauseMenuController] _optionsMenuPanel state after Show(): {_optionsMenuPanel.CurrentState}");
        }
        else
        {
            Debug.LogError("[PauseMenuController] _optionsMenuPanel is null! Cannot show options panel.");
        }
        
        SoundManager.Instance.PlayOneShot("ButtonClick");
    }

    public void HideOptions()
    {
        Debug.Log("[PauseMenuController] HideOptions called");
        if (_optionsMenuPanel != null)
            _optionsMenuPanel.Hide();
        ShowPauseMenu();
        SoundManager.Instance.PlayOneShot("ButtonClick");
    }

    public void ShowInstructions()
    { 
        if (uiManager != null)
        {
            uiManager.OpenInstructions();
        }
        else if (_instructionsPanel != null)
        {
            _instructionsPanel.Show();
            SoundManager.Instance.PlayOneShot("ButtonClick");
        }
    }

    public void CloseInstructions()
    { 
        if (uiManager != null)
        {
            uiManager.CloseInstructions();
        }
        else if (_instructionsPanel != null)
        {
            SoundManager.Instance.PlayOneShot("ButtonClick");
            _instructionsPanel.Hide();
        }
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
