using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : MonoBehaviour
{
    public static SceneLoaderManager Instance;

    [Header("Transition Settings")]
    [SerializeField] private bool useFadeTransitions = true;

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

    #region Public API con Fade

    public void LoadMenuScene() => LoadSceneByIndexWithTransition(0);
    public void LoadGameScene() => LoadSceneByIndexWithTransition(1);

    public void LoadSceneByIndexWithTransition(int sceneIndex, System.Action onLoaded = null)
    {
        if (useFadeTransitions && SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.LoadScene(sceneIndex, onLoaded);
        }
        else
        {
            LoadSceneByIndex(sceneIndex);
            onLoaded?.Invoke();
        }
    }

    public void LoadSceneByNameWithTransition(string sceneName, System.Action onLoaded = null)
    {
        if (useFadeTransitions && SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.LoadScene(sceneName, onLoaded);
        }
        else
        {
            LoadSceneByName(sceneName);
            onLoaded?.Invoke();
        }
    }

    #endregion

    public void LoadSceneByIndex(int loadSceneIndex)
    {
        SceneManager.LoadScene(loadSceneIndex);
    }

    public void LoadSceneByName(string loadSceneName)
    {
        SceneManager.LoadScene(loadSceneName);
    }
}