using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [Header("REFERENCES")]
    public Light2D globalLight;
    public Volume globalVolume;
    private Bloom bloomComponent;

    [Header("lIGHT INTENSITY")]
    public float dayLightIntensity = 1.0f;
    public float nightLightIntensity = 0.5f;
    public float dayGlobalVolumeIntensity = 0f;
    public float nightGlobalVolumeIntensity = 5f;

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
            bloomComponent.intensity.value = (GameManager.Instance.currentGameState == GameState.Day)
                ? dayGlobalVolumeIntensity
                : nightGlobalVolumeIntensity;
        }
        else
        {
            Debug.LogWarning("No se encontró el componente Bloom en el Volume.");
        }

        UpdateLightBasedOnGameState(GameManager.Instance.currentGameState, false);
        lastGameState = GameManager.Instance.currentGameState;

        if (GameManager.Instance.currentGameState != GameState.Paused)
        {
            lastState = GameManager.Instance.currentGameState;
        }
    }

    void Update()
    {
        GameState currentState = GameManager.Instance.currentGameState;

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
        if (gameState == GameState.Paused)
            return;

        float targetLight = (gameState == GameState.Day) ? dayLightIntensity : nightLightIntensity;
        float targetBloom = (gameState == GameState.Day) ? dayGlobalVolumeIntensity : nightGlobalVolumeIntensity;

        if (useTransition)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionLightAndBloom(targetLight, targetBloom, transitionDuration));
        }
        else
        {
            globalLight.intensity = targetLight;
            if (bloomComponent != null)
            {
                bloomComponent.intensity.value = targetBloom;
            }
        }
    }

    IEnumerator TransitionLightAndBloom(float targetLight, float targetBloom, float duration)
    {
        isTransitioning = true;

        float startLight = globalLight.intensity;
        float startBloom = bloomComponent != null ? bloomComponent.intensity.value : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (GameManager.Instance.currentGameState == GameState.Paused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0, 1, t);

            globalLight.intensity = Mathf.Lerp(startLight, targetLight, t);
            if (bloomComponent != null)
            {
                bloomComponent.intensity.value = Mathf.Lerp(startBloom, targetBloom, t);
            }

            yield return null;
        }

        globalLight.intensity = targetLight;
        if (bloomComponent != null)
        {
            bloomComponent.intensity.value = targetBloom;
        }

        transitionCoroutine = null;
        isTransitioning = false;
    }

    public void OnHordeCompleted()
    {
        if (GameManager.Instance.currentGameState == GameState.Night)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionLightAndBloom(dayLightIntensity, dayGlobalVolumeIntensity, transitionDuration));
        }
    }
}