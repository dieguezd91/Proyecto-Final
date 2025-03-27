using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [Header("References")]
    public Light2D globalLight;

    [Header("Light Intensity")]
    public float dayLightIntensity = 1.0f;
    public float nightLightIntensity = 0.5f;

    [Header("Transition")]
    public float transitionDuration = 2.0f;
    public bool useSmoothTransition = true;

    private Coroutine transitionCoroutine;
    private GameState lastGameState = GameState.None;
    private bool isTransitioning = false;

    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
            if (globalLight == null)
            {
                Debug.LogError("Light2D component not found");
                enabled = false;
                return;
            }
        }

        UpdateLightBasedOnGameState(GameManager.Instance.currentGameState, false);
        lastGameState = GameManager.Instance.currentGameState;
    }

    void Update()
    {
        if (!isTransitioning && GameManager.Instance.currentGameState != lastGameState)
        {
            UpdateLightBasedOnGameState(GameManager.Instance.currentGameState, useSmoothTransition);
            lastGameState = GameManager.Instance.currentGameState;
        }
    }

    void UpdateLightBasedOnGameState(GameState gameState, bool useTransition)
    {
        float targetIntensity = (gameState == GameState.Day) ? dayLightIntensity : nightLightIntensity;

        if (useTransition)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionLightIntensity(targetIntensity, transitionDuration));
        }
        else
        {
            globalLight.intensity = targetIntensity;
        }
    }

    IEnumerator TransitionLightIntensity(float targetIntensity, float duration)
    {
        isTransitioning = true;

        float startIntensity = globalLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0, 1, t);
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        globalLight.intensity = targetIntensity;
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

            transitionCoroutine = StartCoroutine(TransitionLightIntensity(dayLightIntensity, transitionDuration));
        }
    }
}