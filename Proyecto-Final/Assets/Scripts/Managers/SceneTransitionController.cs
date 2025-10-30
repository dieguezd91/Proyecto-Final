using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [Header("Fade Configuration")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Loading Settings")]
    [SerializeField] private bool showLoadingScreen = false;
    [SerializeField] private float minimumLoadTime = 0.5f;

    private bool isTransitioning = false;
    private Canvas transitionCanvas;

    private void Awake()
    {
        transitionCanvas = GetComponentInParent<Canvas>();

        if (Instance == null)
        {
            Instance = this;

            if (transitionCanvas != null)
            {
                DontDestroyOnLoad(transitionCanvas.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }

            Initialize();
        }
        else
        {
            if (transitionCanvas != null)
            {
                Destroy(transitionCanvas.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void Initialize()
    {
        if (fadeImage == null)
        {
            return;
        }

        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 9999;
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (transitionCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                transitionCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;
    }

    private void Start()
    {
        if (Instance == this)
        {
            StartCoroutine(FadeIn());
        }
    }

    #region Public API

    public void LoadScene(string sceneName, Action onSceneLoaded = null)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(sceneName, onSceneLoaded));
        }
    }

    public void LoadScene(int sceneIndex, Action onSceneLoaded = null)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(sceneIndex, onSceneLoaded));
        }
    }

    public void FadeOutOnly(Action onComplete = null)
    {
        StartCoroutine(FadeOutCoroutine(onComplete));
    }

    public void FadeInOnly(Action onComplete = null)
    {
        StartCoroutine(FadeInCoroutine(onComplete));
    }

    #endregion

    #region Scene Transition Logic

    private IEnumerator TransitionToScene(string sceneName, Action onSceneLoaded = null)
    {
        isTransitioning = true;
        float startTime = Time.realtimeSinceStartup;

        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        onSceneLoaded?.Invoke();

        yield return new WaitForSecondsRealtime(0.1f);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator TransitionToScene(int sceneIndex, Action onSceneLoaded = null)
    {
        isTransitioning = true;
        float startTime = Time.realtimeSinceStartup;

        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        onSceneLoaded?.Invoke();

        yield return new WaitForSecondsRealtime(0.1f);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    #endregion

    #region Fade Effects

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fadeImage.color = Color.Lerp(startColor, endColor, smoothT);
            yield return null;
        }

        fadeImage.color = endColor;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fadeImage.color = Color.Lerp(startColor, endColor, smoothT);
            yield return null;
        }

        fadeImage.color = endColor;
    }

    private IEnumerator FadeOutCoroutine(Action onComplete = null)
    {
        yield return StartCoroutine(FadeOut());
        onComplete?.Invoke();
    }

    private IEnumerator FadeInCoroutine(Action onComplete = null)
    {
        yield return StartCoroutine(FadeIn());
        onComplete?.Invoke();
    }

    #endregion

    public bool IsTransitioning => isTransitioning;
}