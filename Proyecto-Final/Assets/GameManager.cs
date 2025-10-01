using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SettingsManager SettingsManager { get; private set; }
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SettingsManager = FindObjectOfType<SettingsManager>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        UIManager.Instance?.inventoryUI?.HideInventory();
    }


    public bool IsGamePaused()
    {
        return isPaused;
    }

    public void LoadGameScene()
    {
        SceneLoaderManager.Instance.LoadSceneByName(name);
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
