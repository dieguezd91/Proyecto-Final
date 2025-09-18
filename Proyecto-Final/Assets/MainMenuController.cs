using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private ImprovedUIButton _playButton;
    [SerializeField] private ImprovedUIButton _optionsButton;
    [SerializeField] private ImprovedUIButton _controlsButton;
    [SerializeField] private ImprovedUIButton _exitButton;
    
    [Header("Panels")]
    [SerializeField] private ImprovedUIPanel _optionsPanel;
    
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_playButton != null)
        {
            _playButton.OnClick.AddListener(() => PlayGame("GameScene"));
            _playButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Play button hovered"));
        }
        if (_optionsButton != null)
        {
            _optionsButton.OnClick.AddListener(ShowOptions);
            _optionsButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] ShowOptions button hovered"));
        }
        if (_controlsButton != null)
        {
            _controlsButton.OnClick.AddListener(() => Debug.Log("[MainMenuController] Controls button clicked"));
            _controlsButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Controls button hovered"));
        }
        if (_exitButton != null)
        {
            _exitButton.OnClick.AddListener(QuitGame);
            _exitButton.OnHover.AddListener(() => Debug.Log("[MainMenuController] Exit button hovered"));
        }
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

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
