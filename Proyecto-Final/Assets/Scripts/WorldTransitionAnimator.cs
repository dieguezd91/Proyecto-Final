using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

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

    [Header("Interior Lighting")]
    [SerializeField] private Light2D[] interiorLights;
    [SerializeField] private float interiorLightIntensity = 1f;
    [SerializeField] private bool autoFindInteriorLights = true;
    [SerializeField] private string interiorLightTag = "InteriorLight";

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
        SetupInteriorLights();

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

    public void SetState(WorldState state)
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
        UpdateInteriorLights(state == WorldState.Interior);

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

    private void SetupInteriorLights()
    {
        if (!autoFindInteriorLights) return;

        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag(interiorLightTag);

        if (lightObjects.Length == 0) return;

        interiorLights = new Light2D[lightObjects.Length];

        for (int i = 0; i < lightObjects.Length; i++)
        {
            interiorLights[i] = lightObjects[i].GetComponent<Light2D>();
        }

        UpdateInteriorLights(false);
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

    private void UpdateInteriorLights(bool turnOn)
    {
        if (interiorLights == null || interiorLights.Length == 0) return;

        float targetIntensity = turnOn ? interiorLightIntensity : 0f;

        foreach (var light in interiorLights)
        {
            if (light != null)
            {
                light.intensity = targetIntensity;
            }
        }
    }

    public void SetInteriorLightIntensity(float intensity, float duration = 0f)
    {
        if (interiorLights == null || interiorLights.Length == 0) return;

        if (duration > 0f)
        {
            StartCoroutine(TransitionInteriorLights(intensity, duration));
        }
        else
        {
            foreach (var light in interiorLights)
            {
                if (light != null)
                {
                    light.intensity = intensity;
                }
            }
        }
    }

    public void RestoreInteriorLightIntensity(float duration = 0f)
    {
        if (!IsInInterior) return;

        if (duration > 0f)
        {
            StartCoroutine(TransitionInteriorLights(interiorLightIntensity, duration));
        }
        else
        {
            UpdateInteriorLights(true);
        }
    }

    private IEnumerator TransitionInteriorLights(float targetIntensity, float duration)
    {
        if (interiorLights == null || interiorLights.Length == 0) yield break;

        float[] startIntensities = new float[interiorLights.Length];
        for (int i = 0; i < interiorLights.Length; i++)
        {
            if (interiorLights[i] != null)
            {
                startIntensities[i] = interiorLights[i].intensity;
            }
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < interiorLights.Length; i++)
            {
                if (interiorLights[i] != null)
                {
                    interiorLights[i].intensity = Mathf.Lerp(startIntensities[i], targetIntensity, t);
                }
            }

            yield return null;
        }

        foreach (var light in interiorLights)
        {
            if (light != null)
            {
                light.intensity = targetIntensity;
            }
        }
    }
}

public enum WorldState
{
    Day,
    Night,
    Interior
}