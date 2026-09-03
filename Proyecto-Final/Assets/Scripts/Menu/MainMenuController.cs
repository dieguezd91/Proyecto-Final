using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : UIControllerBase
{
    [Header("Buttons")]
    [SerializeField] private ImprovedUIButton _playButton;
    [SerializeField] private ImprovedUIButton _optionsButton;
    [SerializeField] private ImprovedUIButton _controlsButton;
    [SerializeField] private ImprovedUIButton _creditsButton;
    [SerializeField] private ImprovedUIButton _exitButton;
    [SerializeField] private ImprovedUIButton _creditsBackButton;
    [SerializeField] private ImprovedUIButton _optionsBackButton;
    [SerializeField] private ImprovedUIButton _controlsBackButton;
    
    [Header("Panels")]
    [SerializeField] private UIControllerBase _optionsPanel;
    [SerializeField] private UIControllerBase _creditsPanel;
    [SerializeField] private UIControllerBase _controlsPanel;

    private void Start()
    {
        // Ensure the controller is properly initialized
        Initialize();
        Setup();
    }

    protected override void CacheReferences()
    {
        if (_controlsBackButton == null && _controlsPanel != null)
        {
            _controlsBackButton = _controlsPanel.GetComponentInChildren<ImprovedUIButton>(true);
        }
    }

    protected override void SetupEventListeners()
    {

        if (_playButton != null)
        {
            _playButton.OnClick.AddListener(OnPlayButtonClick);
            _playButton.OnHover.AddListener(() => { });
        }

        if (_optionsButton != null)
        {
            _optionsButton.OnClick.AddListener(ShowOptions);
            _optionsButton.OnHover.AddListener(() => { });
        }

        if (_controlsButton != null)
        {
            _controlsButton.OnClick.AddListener(ShowControls);
            _controlsButton.OnHover.AddListener(() => { });
        }

        if (_exitButton != null)
        {
            _exitButton.OnClick.AddListener(GameManager.Instance.QuitGame);
            _exitButton.OnHover.AddListener(() => { });
        }
        
        if (_optionsBackButton != null)
        {
            _optionsBackButton.OnClick.AddListener(HideOptions);
            _optionsBackButton.OnHover.AddListener(() => { });
        }

        if (_creditsBackButton != null)
        {
            _creditsBackButton.OnClick.AddListener(HideCredits);
            _creditsBackButton.OnHover.AddListener(() => { });
        }

        if (_controlsBackButton != null)
        {
            _controlsBackButton.OnClick.AddListener(HideControls);
            _controlsBackButton.OnHover.AddListener(() => { });
        }

        if (_controlsPanel is InstructionsPanel instructionsPanel)
        {
            instructionsPanel.OnGoBackClicked.AddListener(HideControls);
        }

        if (_creditsButton != null)
        {
            _creditsButton.OnClick.AddListener(ShowCredits);
            _creditsButton.OnHover.AddListener(() => { });
        }
    }

    protected override void ConfigureInitialState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowOptions()
    {
        if (_optionsPanel != null)
        {
            _optionsPanel.Show();
        }
    }

    private void HideOptions()
    {
        if (_optionsPanel != null)
        {
            _optionsPanel.Hide();
        }
    }

    private void ShowControls()
    {
        _controlsPanel?.Show();
    }

    private void HideControls()
    {
        _controlsPanel?.Hide();
    }

    private void ShowCredits()
    {
        if (_creditsPanel != null)
        {
            _creditsPanel.Show();
        }
    }

    private void HideCredits()
    {
        if (_creditsPanel != null)
        {
            _creditsPanel.Hide();
        }
    }

    public void OnPlayButtonClick()
    {
        SceneLoaderManager.Instance.LoadGameScene();
    }
}
