using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [Header("Referencias")]
    public Light2D globalLight;
    public WaveSpawner waveSpawner;

    [Header("Intensidad")]
    public float dayLightIntensity = 1.0f;
    public float nightLightIntensity = 0.5f;

    [Header("Transicion")]
    public float transitionDuration = 2.0f;
    public bool useSmoothTransition = true;

    private Coroutine transitionCoroutine;
    private GameState lastGameState = GameState.None;

    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();

            if (globalLight == null)
            {
                Debug.Log("No se encontro Light2D");
                enabled = false;
                return;
            }
        }

        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            if (waveSpawner == null)
            {
                Debug.Log("No se encontro el WaveSpawner");
                enabled = false;
                return;
            }
        }

        UpdateLightBasedOnGameState(GameManager.Instance.currentGameState, false);
        lastGameState = GameManager.Instance.currentGameState;
    }

    void Update()
    {
        if (GameManager.Instance.currentGameState != lastGameState)
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

            transitionCoroutine = StartCoroutine(TransitionLightIntensity(targetIntensity));
        }
        else
        {
            globalLight.intensity = targetIntensity;
        }
    }

    IEnumerator TransitionLightIntensity(float targetIntensity)
    {
        float startIntensity = globalLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            t = Mathf.SmoothStep(0, 1, t);

            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        globalLight.intensity = targetIntensity;
        transitionCoroutine = null;
    }
}