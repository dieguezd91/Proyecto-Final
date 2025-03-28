using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{

    public static LoadingManager Instance;
    public GameObject screen;
    public Slider slider;
    public Text pressAnyKey;

    public List<AsyncOperation> sceneLoading = new List<AsyncOperation>();

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    public void LoadScene(int un, int load)
    {
        screen.SetActive(true);

        sceneLoading.Add(SceneManager.UnloadSceneAsync(un));
        sceneLoading.Add(SceneManager.LoadSceneAsync(load, LoadSceneMode.Additive));
        StartCoroutine(GetSceneProgress());
    }

    public IEnumerator GetSceneProgress()
    {
        for (int i = 0; i < sceneLoading.Count; i++)
        {
            while (!sceneLoading[i].isDone)
            {
                slider.value = sceneLoading[i].progress;
                yield return null;
            }

        }

        screen.SetActive(false);
    }

}
