using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : UIControllerBase
{
    [Header("Buttons")]
    [SerializeField] private ImprovedUIButton _playButton;
    [SerializeField] private ImprovedUIButton _optionsButton;
    [SerializeField] private ImprovedUIButton _controlsButton;
    [SerializeField] private ImprovedUIButton _exitButton;
    [SerializeField] private ImprovedUIButton _backButton;
    
    [Header("Panels")]
    [SerializeField] private UIControllerBase _optionsPanel;

    private void Start()
    {
        // Ensure the controller is properly initialized
        Initialize();
        Setup();
    }

    protected override void CacheReferences()
    {
        // Cache any references needed
    }

    protected override void SetupEventListeners()
    {

        if (_playButton != null)
        {
            _playButton.OnClick.AddListener(PlayGame);
            _playButton.OnHover.AddListener(() => { });
        }

        if (_optionsButton != null)
        {
            _optionsButton.OnClick.AddListener(ShowOptions);
            _optionsButton.OnHover.AddListener(() => { });
        }

        if (_controlsButton != null)
        {
            _controlsButton.OnClick.AddListener(() => { });
            _controlsButton.OnHover.AddListener(() => { });
        }

        if (_exitButton != null)
        {
            _exitButton.OnClick.AddListener(GameManager.Instance.QuitGame);
            _exitButton.OnHover.AddListener(() => { });
        }

        
        if (_backButton != null)
        {
            _backButton.OnClick.AddListener(HideOptions);
            _backButton.OnHover.AddListener(() => { });
        }
    }

    protected override void ConfigureInitialState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void PlayGame() => SceneLoaderManager.Instance.LoadGameScene();

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
}
