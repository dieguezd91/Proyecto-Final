using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [Header("Transition Type")]
    [SerializeField] private TransitionType transitionType = TransitionType.Animated;

    [Header("Fade Configuration")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration;
    [SerializeField] private Color fadeColor;

    [Header("Animated Transition Configuration")]
    [SerializeField] private Image animatedTransitionImage;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string fadeOutAnimationName = "FadeOut";
    [SerializeField] private string fadeInAnimationName = "FadeIn";
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private bool waitForAnimationEnd = true;
    [Header("Loading Settings")]
    [SerializeField] private bool showLoadingScreen = false;
    [SerializeField] private float minimumLoadTime;

    private bool isTransitioning = false;
    private Canvas transitionCanvas;
    private Coroutine currentTransitionCoroutine;

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
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 9999;
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (transitionCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                transitionCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        if (transitionType == TransitionType.Simple && fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.raycastTarget = false;
            fadeImage.gameObject.SetActive(true);

            if (animatedTransitionImage != null)
                animatedTransitionImage.gameObject.SetActive(false);
        }
        else if (transitionType == TransitionType.Animated && animatedTransitionImage != null)
        {
            animatedTransitionImage.color = Color.white;
            animatedTransitionImage.raycastTarget = false;
            animatedTransitionImage.gameObject.SetActive(false);

            if (fadeImage != null)
                fadeImage.gameObject.SetActive(false);
        }
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
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        currentTransitionCoroutine = StartCoroutine(FadeOutCoroutine(onComplete));
    }

    public void FadeInOnly(Action onComplete = null)
    {
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        currentTransitionCoroutine = StartCoroutine(FadeInCoroutine(onComplete));
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
        if (transitionType == TransitionType.Simple)
        {
            yield return StartCoroutine(FadeOutSimple());
        }
        else
        {
            yield return StartCoroutine(FadeOutAnimated());
        }
    }

    private IEnumerator FadeIn()
    {
        if (transitionType == TransitionType.Simple)
        {
            yield return StartCoroutine(FadeInSimple());
        }
        else
        {
            yield return StartCoroutine(FadeInAnimated());
        }
    }

    private IEnumerator FadeOutSimple()
    {
        if (fadeImage == null) yield break;

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

    private IEnumerator FadeInSimple()
    {
        if (fadeImage == null) yield break;

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

    private IEnumerator FadeOutAnimated()
    {
        if (transitionAnimator == null || animatedTransitionImage == null)
        {
            yield break;
        }

        animatedTransitionImage.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        transitionAnimator.Play(fadeOutAnimationName, 0, 0f);

        if (waitForAnimationEnd)
        {
            yield return new WaitForEndOfFrame();

            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.length;

            if (animLength <= 0f)
            {
                animLength = animationDuration;
            }

            yield return new WaitForSecondsRealtime(animLength);
        }
        else
        {
            yield return new WaitForSecondsRealtime(animationDuration);
        }
    }

    private IEnumerator FadeInAnimated()
    {
        if (transitionAnimator == null || animatedTransitionImage == null)
        {
            yield break;
        }

        transitionAnimator.Play(fadeInAnimationName, 0, 0f);

        if (waitForAnimationEnd)
        {
            yield return new WaitForEndOfFrame();

            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.length;

            if (animLength <= 0f)
            {
                animLength = animationDuration;
            }

            yield return new WaitForSecondsRealtime(animLength);
        }
        else
        {
            yield return new WaitForSecondsRealtime(animationDuration);
        }

        animatedTransitionImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutCoroutine(Action onComplete = null)
    {
        yield return StartCoroutine(FadeOut());
        onComplete?.Invoke();
        currentTransitionCoroutine = null;
    }

    private IEnumerator FadeInCoroutine(Action onComplete = null)
    {
        yield return StartCoroutine(FadeIn());
        onComplete?.Invoke();
        currentTransitionCoroutine = null;
    }

    #endregion

    public bool IsTransitioning => isTransitioning;
}

public enum TransitionType
{
    Simple,
    Animated
}