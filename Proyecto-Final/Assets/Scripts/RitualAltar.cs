using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RitualAltar : MonoBehaviour
{
    [Header("Ritual Configuration")]
    [SerializeField] private float ritualDuration = 10f;
    [SerializeField] private bool canTransitionToNight = true;
    [SerializeField] private bool canRestoreHealth = true;

    [Header("Visual Effects")]
    [SerializeField] private Light2D[] candleLights;

    [Header("Colors & Materials")]
    [SerializeField] private Color candleColor = new Color(0.7f, 0.3f, 1f);

    [Header("Candle Settings")]
    [SerializeField] private float candleFlickerSpeed = 2f;
    [SerializeField] private float candleFlickerAmount = 0.3f;
    [SerializeField] private float candleIgnitionDelay = 0.3f;

    [Header("Post Processing")]
    [SerializeField] private float ritualVignetteIntensity;
    [SerializeField] private bool centerVignetteOnPlayer = true;

    [Header("Sprite Change")]
    [SerializeField] private SpriteRenderer altarSpriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite nearSprite;

    private bool isPerformingRitual = false;
    private bool playerInRange = false;
    private LevelManager levelManager;
    private LifeController playerLife;
    private DayNightLightController lightController;
    private GameObject player;
    private Camera mainCamera;
    private Vector2 originalVignetteCenter;

    [SerializeField] private GameObject interactionPromptCanvas;

    private void Start()
    {
        levelManager = LevelManager.Instance;

        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);

        if (levelManager != null && levelManager.playerLife != null)
        {
            playerLife = levelManager.playerLife;
            player = playerLife.gameObject;
        }

        lightController = FindObjectOfType<DayNightLightController>();
        mainCamera = Camera.main;

        VerifyVignetteSetup();

        TurnOffAllCandles();

        UpdateAltarAppearance();
    }

    private void Update()
    {
        if (playerInRange)
        {
            interactionPromptCanvas.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F) && CanPerformRitual())
            {
                StartCoroutine(PerformRitualCoroutine());
            }
        }
        else
            interactionPromptCanvas.SetActive(false);
    }

    private bool CanPerformRitual()
    {
        if (isPerformingRitual || levelManager == null || playerLife == null)
            return false;

        GameState currentState = levelManager.GetCurrentGameState();
        return currentState == GameState.Digging ||
               currentState == GameState.Planting ||
               currentState == GameState.Harvesting ||
               currentState == GameState.Removing;
    }

    private IEnumerator PerformRitualCoroutine()
    {
        isPerformingRitual = true;
        GameState previousState = levelManager.GetCurrentGameState();
        levelManager.SetGameState(GameState.OnRitual);

        StartRitualEffects();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowRitualOverlay();

        yield return new WaitForSeconds(ritualDuration);

        ApplyRitualEffects();

        if (canTransitionToNight && levelManager.GetCurrentGameState() != GameState.Night)
            levelManager.TransitionToNight();
        else
            levelManager.SetGameState(previousState);

        if (UIManager.Instance != null)
            UIManager.Instance.HideRitualOverlay();

        EndRitualEffects();

        isPerformingRitual = false;
    }


    private void StartRitualEffects()
    {
        StartCoroutine(RitualLightingSequence());

        if (centerVignetteOnPlayer)
        {
            StartCoroutine(UpdateVignetteCenterDuringRitual());
        }
    }

    private IEnumerator RitualLightingSequence()
    {
        Coroutine vignetteCoroutine = StartCoroutine(ApplyRitualVignetteCoroutine());
        lightController?.DimLightForRitual(0.05f, 1.5f);

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(LightCandlesSequentially());
    }

    private IEnumerator LightCandlesSequentially()
    {
        if (candleLights == null || candleLights.Length == 0) yield break;

        for (int i = 0; i < candleLights.Length; i++)
        {
            if (candleLights[i] != null)
            {
                candleLights[i].gameObject.SetActive(true);
                candleLights[i].color = candleColor;
                SoundManager.Instance.Play("CandleOn");
                StartCoroutine(FlickerCandle(candleLights[i], i));
            }
            yield return new WaitForSeconds(candleIgnitionDelay);

        }
    }

    private IEnumerator FlickerCandle(Light2D candle, int candleIndex)
    {
        if (candle == null) yield break;

        float originalIntensity = candle.intensity;
        float randomOffset = candleIndex * 0.5f;

        float elapsed = 0f;
        while (elapsed < ritualDuration - (candleIndex * candleIgnitionDelay))
        {
            elapsed += Time.deltaTime;

            float flicker = Mathf.Sin((elapsed + randomOffset) * candleFlickerSpeed) * candleFlickerAmount;
            float ritualPulse = Mathf.PingPong(elapsed * 1.5f, 1f) * 0.5f;

            candle.intensity = originalIntensity + flicker + ritualPulse;

            yield return null;
        }
        candle.intensity = originalIntensity;
    }

    private void ApplyRitualEffects()
    {
        if (canRestoreHealth && playerLife != null)
        {
            playerLife.currentHealth = playerLife.maxHealth;
            playerLife.onHealthChanged?.Invoke(playerLife.currentHealth, playerLife.maxHealth);

            if (levelManager.uiManager != null)
            {
                levelManager.uiManager.UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
            }
        }
    }

    private IEnumerator ApplyRitualVignetteCoroutine()
    {
        if (lightController == null || lightController.globalVolume == null) yield break;

        if (lightController.globalVolume.profile.TryGet<Vignette>(out var vignetteComponent))
        {
            vignetteComponent.center.overrideState = true;
            vignetteComponent.intensity.overrideState = true;

            originalVignetteCenter = vignetteComponent.center.value;

            float current = vignetteComponent.intensity.value;
            float duration = 1.5f;

            if (centerVignetteOnPlayer && player != null && mainCamera != null)
            {
                Vector3 playerScreenPos = mainCamera.WorldToViewportPoint(player.transform.position);
                Vector2 vignetteCenter = new Vector2(
                    Mathf.Clamp01(playerScreenPos.x),
                    Mathf.Clamp01(playerScreenPos.y)
                );
                vignetteComponent.center.value = vignetteCenter;
            }

            yield return StartCoroutine(AnimateVignetteIntensity(current, ritualVignetteIntensity, duration));
        }
    }

    private void EndRitualEffects()
    {
        RestoreCorrectVignette();
        SoundManager.Instance.Play("CandleOff");

        StartCoroutine(ExtinguishCandlesGradually());

        lightController.RestoreLightAfterRitual(GameState.Night, 1.5f);
        UpdateAltarAppearance();
    }

    private void TurnOffAllCandles()
    {
        if (candleLights == null) return;

        foreach (Light2D candle in candleLights)
        {
            if (candle != null)
            {
                candle.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator ExtinguishCandlesGradually()
    {
        if (candleLights == null || candleLights.Length == 0) yield break;

        yield return new WaitForSeconds(1f);

        for (int i = candleLights.Length - 1; i >= 0; i--)
        {
            if (candleLights[i] != null && candleLights[i].gameObject.activeInHierarchy)
            {
                candleLights[i].gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(candleIgnitionDelay * 0.5f);
        }
    }

    private IEnumerator UpdateVignetteCenterDuringRitual()
    {
        if (lightController == null || lightController.globalVolume == null || player == null || mainCamera == null)
            yield break;

        if (!lightController.globalVolume.profile.TryGet<Vignette>(out var vignetteComponent))
            yield break;

        while (isPerformingRitual)
        {
            Vector3 playerScreenPos = mainCamera.WorldToViewportPoint(player.transform.position);

            Vector2 vignetteCenter = new Vector2(
                Mathf.Clamp01(playerScreenPos.x),
                Mathf.Clamp01(playerScreenPos.y)
            );
            vignetteComponent.center.value = vignetteCenter;

            yield return null;
        }
    }

    private void VerifyVignetteSetup()
    {
        if (lightController == null || lightController.globalVolume == null)
        {
            return;
        }

        if (lightController.globalVolume.profile.TryGet<Vignette>(out var vignetteComponent))
        {
            vignetteComponent.center.overrideState = true;
        }
    }

    private void RestoreCorrectVignette()
    {
        if (lightController == null || lightController.globalVolume == null) return;

        if (lightController.globalVolume.profile.TryGet<Vignette>(out var vignetteComponent))
        {
            bool isNight = levelManager.GetCurrentGameState() == GameState.Night;
            float targetVignette = isNight ? lightController.nightVignetteIntensity : lightController.dayVignetteIntensity;

            float current = vignetteComponent.intensity.value;
            StopCoroutine("AnimateVignetteIntensity");
            StartCoroutine(AnimateVignetteIntensity(current, targetVignette, 1.5f));

            vignetteComponent.center.value = originalVignetteCenter;
        }
    }

    private void UpdateAltarAppearance()
    {
        if (levelManager != null)
        {
            bool isCurrentlyNight = levelManager.GetCurrentGameState() == GameState.Night;

            if (candleLights != null)
            {
                foreach (Light2D candle in candleLights)
                {
                    if (candle != null && candle.gameObject.activeInHierarchy)
                    {
                        candle.color = candleColor;
                    }
                }
            }
        }
    }

    private IEnumerator AnimateVignetteIntensity(float from, float to, float duration)
    {
        if (lightController == null || lightController.globalVolume == null) yield break;

        if (!lightController.globalVolume.profile.TryGet<Vignette>(out var vignetteComponent)) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            vignetteComponent.intensity.value = Mathf.Lerp(from, to, t);
            yield return null;
        }

        vignetteComponent.intensity.value = to;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (altarSpriteRenderer != null && nearSprite != null)
                altarSpriteRenderer.sprite = nearSprite;

            if (CanPerformRitual())
            {
                Debug.Log("Presiona E para realizar el ritual de preparaci�n");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (altarSpriteRenderer != null && defaultSprite != null)
                altarSpriteRenderer.sprite = defaultSprite;
        }
    }


    public bool IsRitualAvailable()
    {
        return !isPerformingRitual && CanPerformRitual();
    }

    public void ActivateRitual()
    {
        if (CanPerformRitual())
        {
            StartCoroutine(PerformRitualCoroutine());
        }
    }
}