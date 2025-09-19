using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : MonoBehaviour
{
    public static SceneLoaderManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public void LoadMenuScene() => LoadSceneByIndex(0);
    public void LoadGameScene() => LoadSceneByIndex(1);

    public void LoadSceneByIndex(int loadSceneIndex)
    {
        SceneManager.LoadScene(loadSceneIndex);
    }

    public void LoadSceneByName(string loadSceneName)
    {
        SceneManager.LoadScene(loadSceneName);
    }
}
