using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;
    private GameFlowController gameFlowController;
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
    private bool isTransitioning = false;

    void Start()
    {
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>();

        gameFlowController = GameFlowController.Instance;
        if (gameFlowController == null)
        {
            gameFlowController = FindObjectOfType<GameFlowController>();
        }

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
            if (gameFlowController != null)
            {
                bloomComponent.intensity.value = (gameFlowController.CurrentPhase != GamePhase.Night)
                    ? dayGlobalVolumeIntensity
                    : nightGlobalVolumeIntensity;
            }
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

        if (gameFlowController != null)
        {
            UpdateLightBasedOnGameState(gameFlowController.CurrentPhase, false);
            gameFlowController.OnPhaseChanged += OnPhaseChanged;
        }
    }

    private void OnDestroy()
    {
        if (gameFlowController != null)
        {
            gameFlowController.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(GamePhase newPhase)
    {
        UpdateLightBasedOnGameState(newPhase, useSmoothTransition);
    }

    void UpdateLightBasedOnGameState(GamePhase gameState, bool useTransition)
    {
        if ((pauseController != null && pauseController.IsPaused) || gameState == GamePhase.OnRitual || gameState == GamePhase.GameOver)
            return;

        bool isDayState = gameState != GamePhase.Night;

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
            if ((pauseController != null && pauseController.IsPaused))
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
        if (gameFlowController != null && gameFlowController.CurrentPhase == GamePhase.Night)
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

    public void RestoreLightAfterRitual(GamePhase targetState, float duration)
    {
        bool isDayState = targetState != GamePhase.Night;

        float targetLight = isDayState ? dayLightIntensity : nightLightIntensity;
        float targetBloom = isDayState ? dayGlobalVolumeIntensity : nightGlobalVolumeIntensity;
        float targetExposure = isDayState ? dayExposure : nightExposure;
        float targetVignette = isDayState ? dayVignetteIntensity : nightVignetteIntensity;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionVisuals(targetLight, targetBloom, targetExposure, targetVignette, duration));
    }
}
