using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayGame(string name)
    {
        // Example: Unload menu (index 0), load game (index 1)
        SceneLoaderManager.Instance.LoadSceneByName(name);
    }

    public void Options()
    {
        // Example: Unload menu (index 0), load options (index 2)
        SceneLoaderManager.Instance.LoadSceneByIndex(2);
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
