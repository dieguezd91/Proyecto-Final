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
    [SerializeField] private ImprovedUIButton _BackButton;
    
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
        Debug.Log("[MainMenuController] Setting up event listeners");
        
        if (_playButton != null)
        {
            _playButton.OnClick.AddListener(() => PlayGame("TreeScene"));
            _playButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Play button hovered"));
            Debug.Log("[MainMenuController] Play button listeners added");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Play button is null!");
        }
        
        if (_optionsButton != null)
        {
            _optionsButton.OnClick.AddListener(ShowOptions);
            _optionsButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] ShowOptions button hovered"));
            Debug.Log("[MainMenuController] Options button listeners added");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Options button is null!");
        }
        
        if (_controlsButton != null)
        {
            _controlsButton.OnClick.AddListener(() => Debug.Log("[MainMenuController] Controls button clicked"));
            _controlsButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Controls button hovered"));
            Debug.Log("[MainMenuController] Controls button listeners added");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Controls button is null!");
        }
        
        if (_exitButton != null)
        {
            _exitButton.OnClick.AddListener(GameManager.Instance.QuitGame);
            _exitButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Exit button hovered"));
            Debug.Log("[MainMenuController] Exit button listeners added");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Exit button is null!");
        }
        
        if (_BackButton != null)
        {
            _BackButton.OnClick.AddListener(HideOptions);
            _BackButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Back button hovered"));
            Debug.Log("[MainMenuController] Back button listeners added");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Exit button is null!");
        }
    }

    protected override void ConfigureInitialState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayGame(string name)
    {
        // Example: Unload menu (index 0), load game (index 1)
        SceneLoaderManager.Instance.LoadSceneByName(name);
    }

    public void ShowOptions()
    {
        Debug.Log("[MainMenuController] ShowOptions button clicked, showing options panel");
        if (_optionsPanel != null)
        {
            _optionsPanel.Show();
        }
    }
    
    public void HideOptions()
    {
        Debug.Log("[MainMenuController] ShowOptions button clicked, showing options panel");
        if (_optionsPanel != null)
        {
            _optionsPanel.Hide();
        }
    }
}
