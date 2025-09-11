using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [Header("REFERENCES")]
    public Light2D globalLight;
    public Volume globalVolume;
    private Bloom bloomComponent;
    private ColorAdjustments colorAdjustmentsComponent;
    private Vignette vignetteComponent;

    [Header("LIGHT INTENSITY")]
    public float dayLightIntensity = 1.0f;
    public float nightLightIntensity = 0.5f;
    public float dayGlobalVolumeIntensity = 0f;
    public float nightGlobalVolumeIntensity = 5f;

    [Header("VIGNETTE")]
    public float dayVignetteIntensity = 0f;
    public float nightVignetteIntensity = 0.4f;

    [Header("COLOR ADJUSTMENTS")]
    public float dayExposure = 0f;
    public float nightExposure = -0.5f;

    [Header("TRANSITION")]
    public float transitionDuration = 2.0f;
    public bool useSmoothTransition = true;

    private Coroutine transitionCoroutine;
    private GameState lastGameState = GameState.None;
    private bool isTransitioning = false;
    private GameState lastState = GameState.Day;

    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
            if (globalLight == null)
            {
                enabled = false;
                return;
            }
        }

        if (globalVolume.profile.TryGet<Bloom>(out bloomComponent))
        {
            bloomComponent.intensity.value = (LevelManager.Instance.currentGameState != GameState.Night)
                ? dayGlobalVolumeIntensity
                : nightGlobalVolumeIntensity;
        }

        if (globalVolume.profile.TryGet<Vignette>(out vignetteComponent))
        {
            vignetteComponent.intensity.overrideState = true;
        }

        if (globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustmentsComponent))
        {
            colorAdjustmentsComponent.colorFilter.overrideState = true;
            colorAdjustmentsComponent.postExposure.overrideState = true;
        }

        UpdateLightBasedOnGameState(LevelManager.Instance.currentGameState, false);
        lastGameState = LevelManager.Instance.currentGameState;

        if (LevelManager.Instance.currentGameState != GameState.Paused)
        {
            lastState = LevelManager.Instance.currentGameState;
        }
    }

    void Update()
    {
        GameState currentState = LevelManager.Instance.currentGameState;

        if (!isTransitioning && currentState != lastGameState)
        {
            if (currentState != GameState.Paused)
            {
                lastState = currentState;
            }

            bool isPauseTransition = (currentState == GameState.Paused || lastGameState == GameState.Paused);

            if (!isPauseTransition)
            {
                UpdateLightBasedOnGameState(currentState, useSmoothTransition);
            }

            lastGameState = currentState;
        }
    }

    void UpdateLightBasedOnGameState(GameState gameState, bool useTransition)
    {
        if (gameState == GameState.Paused || gameState == GameState.OnRitual)
            return;

        bool isDayState = gameState != GameState.Night;

        float targetLight = isDayState ? dayLightIntensity : nightLightIntensity;
        float targetBloom = isDayState ? dayGlobalVolumeIntensity : nightGlobalVolumeIntensity;
        float targetExposure = isDayState ? dayExposure : nightExposure;
        float targetVignette = isDayState ? dayVignetteIntensity : nightVignetteIntensity;

        if (useTransition)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionVisuals(targetLight, targetBloom, targetExposure, targetVignette, transitionDuration));
        }
        else
        {
            globalLight.intensity = targetLight;

            if (bloomComponent != null)
                bloomComponent.intensity.value = targetBloom;

            if (colorAdjustmentsComponent != null)
            {
                colorAdjustmentsComponent.postExposure.value = targetExposure;
            }

            if (vignetteComponent != null)
                vignetteComponent.intensity.value = targetVignette;
        }
    }

    IEnumerator TransitionVisuals(float targetLight, float targetBloom, float targetExposure, float targetVignette, float duration)
    {
        isTransitioning = true;

        float startLight = globalLight.intensity;
        float startBloom = bloomComponent != null ? bloomComponent.intensity.value : 0f;
        float startExposure = colorAdjustmentsComponent != null ? colorAdjustmentsComponent.postExposure.value : 0f;
        Color startColorFilter = colorAdjustmentsComponent != null ? colorAdjustmentsComponent.colorFilter.value : Color.white;
        float startVignette = vignetteComponent != null ? vignetteComponent.intensity.value : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (LevelManager.Instance.currentGameState == GameState.Paused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration);

            globalLight.intensity = Mathf.Lerp(startLight, targetLight, t);

            if (bloomComponent != null)
                bloomComponent.intensity.value = Mathf.Lerp(startBloom, targetBloom, t);

            if (vignetteComponent != null)
                vignetteComponent.intensity.value = Mathf.Lerp(startVignette, targetVignette, t);

            yield return null;
        }

        isTransitioning = false;
    }

    public void OnHordeCompleted()
    {
        if (LevelManager.Instance.currentGameState == GameState.Night)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionVisuals(dayLightIntensity, dayGlobalVolumeIntensity, dayExposure, dayVignetteIntensity, transitionDuration));
        }
    }

    public void DimLightForRitual(float targetIntensity, float duration)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionVisuals(targetIntensity, bloomComponent?.intensity.value ?? 0f, colorAdjustmentsComponent?.postExposure.value ?? 0f, vignetteComponent?.intensity.value ?? 0f, duration));
    }

    public void RestoreLightAfterRitual(GameState targetState, float duration)
    {
        bool isDayState = targetState != GameState.Night;

        float targetLight = isDayState ? dayLightIntensity : nightLightIntensity;
        float targetBloom = isDayState ? dayGlobalVolumeIntensity : nightGlobalVolumeIntensity;
        float targetExposure = isDayState ? dayExposure : nightExposure;
        float targetVignette = isDayState ? dayVignetteIntensity : nightVignetteIntensity;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionVisuals(targetLight, targetBloom, targetExposure, targetVignette, duration));
    }
}