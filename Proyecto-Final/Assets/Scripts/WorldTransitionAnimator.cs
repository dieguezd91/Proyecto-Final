using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldTransitionAnimator : MonoBehaviour
{
    [SerializeField] private Animator worldAnimator;
    [SerializeField] private string dayStateName = "BaseMap";
    [SerializeField] private string nightStateName = "HellMap";
    [SerializeField] private string interiorStateName = "InsideHouse";
    [SerializeField] private string transitionTrigger = "ChangeWorld";
    [SerializeField] private string houseEnterTrigger = "EnterHouse";
    [SerializeField] private string houseExitTrigger = "ExitHouse";

    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [SerializeField] private GameObject[] houseObjects;
    [SerializeField] private bool autoFindHouse = true;
    [SerializeField] private string houseTag = "Home";

    private WorldState currentState = WorldState.Day;
    private WorldState stateBeforeInterior = WorldState.Day;
    private bool isTransitioning = false;

    private Canvas fadeCanvas;
    private Image fadeImage;

    public bool IsNightMode => currentState == WorldState.Night;
    public bool IsInInterior => currentState == WorldState.Interior;
    public bool IsTransitioning => isTransitioning;

    public Action<WorldState> OnStateChanged;

    void Start()
    {
        SetupAnimator();
        SetupFadeSystem();
        SetupHouseObjects();

        ChangeState(WorldState.Day, false);
    }

    public void TransitionToDay()
    {
        if (currentState == WorldState.Interior)
        {
            stateBeforeInterior = WorldState.Day;
            return;
        }
        ChangeState(WorldState.Day, true);
    }

    public void TransitionToNight()
    {
        if (currentState == WorldState.Interior)
        {
            stateBeforeInterior = WorldState.Night;
            return;
        }
        ChangeState(WorldState.Night, true);
    }

    public void EnterHouse()
    {
        if (currentState == WorldState.Interior) return;

        stateBeforeInterior = currentState;
        ChangeState(WorldState.Interior, true);
    }

    public void ExitHouse()
    {
        if (currentState != WorldState.Interior) return;

        ChangeState(stateBeforeInterior, true);
    }

    public void SetStateBeforeInterior(WorldState state)
    {
        stateBeforeInterior = state;
    }

    public void ForceDayMode()
    {
        ChangeState(WorldState.Day, false);
    }

    public void ForceNightMode()
    {
        ChangeState(WorldState.Night, false);
    }

    private void ChangeState(WorldState newState, bool useTransition)
    {
        if (currentState == newState || isTransitioning) return;

        WorldState oldState = currentState;
        currentState = newState;

        if (useTransition && useFade)
        {
            StartCoroutine(FadeTransition(oldState, newState));
        }
        else
        {
            ApplyState(newState);
        }

        OnStateChanged?.Invoke(newState);
    }

    private void ApplyState(WorldState state)
    {
        SetHouseVisibility(state != WorldState.Interior);

        if (worldAnimator == null) return;

        string targetState = GetAnimatorStateName(state);

        worldAnimator.Play(targetState, 0, 1f);
    }

    private string GetAnimatorStateName(WorldState state)
    {
        switch (state)
        {
            case WorldState.Day: return dayStateName;
            case WorldState.Night: return nightStateName;
            case WorldState.Interior: return interiorStateName;
            default: return dayStateName;
        }
    }

    private void SetupFadeSystem()
    {
        if (!useFade) return;

        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform);

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeTransition(WorldState oldState, WorldState newState)
    {
        isTransitioning = true;

        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);

            yield return FadeToColor(fadeColor, fadeDuration * 0.4f);

            ApplyState(newState);

            yield return FadeToColor(Color.clear, fadeDuration * 0.6f);

            fadeCanvas.gameObject.SetActive(false);
        }
        else
        {
            ApplyState(newState);
            yield return new WaitForSeconds(0.1f);
        }

        isTransitioning = false;
    }

    private IEnumerator FadeToColor(Color targetColor, float duration)
    {
        Color startColor = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;
    }

    private void SetupAnimator()
    {
        if (worldAnimator == null)
            worldAnimator = GetComponent<Animator>();
    }

    private void SetupHouseObjects()
    {
        if (!autoFindHouse) return;

        GameObject[] houses = GameObject.FindGameObjectsWithTag(houseTag);
        if (houses.Length > 0)
        {
            houseObjects = houses;
        }
    }

    private void SetHouseVisibility(bool visible)
    {
        if (houseObjects == null) return;

        foreach (var house in houseObjects)
        {
            if (house != null)
                house.SetActive(visible);
        }
    }
}

public enum WorldState
{
    Day,
    Night,
    Interior
}