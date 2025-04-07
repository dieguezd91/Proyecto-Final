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
            if (GameManager.Instance.currentGameState == GameState.Paused)
            {
                yield return null;
                continue;
            }

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