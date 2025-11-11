using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RitualAltar : MonoBehaviour, IInteractable
{
    [Header("Ritual Configuration")]
    [SerializeField] private float ritualDuration = 10f;
    [SerializeField] private bool canTransitionToNight = true;
    [SerializeField] private bool canRestoreHealth = true;

    [Header("Teleport Configuration")]
    [SerializeField] private bool teleportAfterRitual = true;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private float teleportDelay = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private Light2D[] candleLights;
    [SerializeField] private Color candleColor = new Color(0.7f, 0.3f, 1f);
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D DoorLight;
    [SerializeField] private float ritualLightFadeDuration = 2f;



    [Header("Candle Settings")]
    [SerializeField] private float candleFlickerSpeed = 2f;
    [SerializeField] private float candleFlickerAmount = 0.3f;
    [SerializeField] private float candleIgnitionDelay = 0.3f;

    [Header("Post Processing")]
    [SerializeField] private float ritualVignetteIntensity = 0.6f;
    [SerializeField] private bool centerVignetteOnPlayer = true;
    [SerializeField] private float vignetteFadeDuration = 1.5f;

    [Header("Sprite Change")]
    [SerializeField] private SpriteRenderer altarSpriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite nearSprite;

    private const float DAY_VIGNETTE_INTENSITY = 0.15f;
    private const float NIGHT_VIGNETTE_INTENSITY = 0.45f;
    private const float RITUAL_LIGHT_DIM = 0.05f;
    private const float RITUAL_PULSE_SPEED = 1.5f;

    [Header ("References")]
    private LevelManager levelManager;
    private LifeController playerLife;
    private DayNightLightController lightController;
    private GameObject player;
    private Camera mainCamera;
    private WorldTransitionAnimator worldTransition;
    private Vignette vignetteComponent;

    private bool isPerformingRitual = false;
    private Coroutine mainRitualCoroutine;

    private void Start()
    {
        CacheReferences();
        InitializeComponents();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void CacheReferences()
    {
        levelManager = LevelManager.Instance;

        if (levelManager?.playerLife != null)
        {
            playerLife = levelManager.playerLife;
            player = playerLife.gameObject;
        }

        lightController = FindObjectOfType<DayNightLightController>();
        worldTransition = FindObjectOfType<WorldTransitionAnimator>();
        mainCamera = Camera.main;
    }

    private void InitializeComponents()
    {
        SetupVignette();
        TurnOffAllCandles();
        UpdateAltarAppearance();
    }

    private void SetupVignette()
    {
        if (lightController?.globalVolume == null) return;

        if (lightController.globalVolume.profile.TryGet<Vignette>(out vignetteComponent))
        {
            vignetteComponent.center.overrideState = true;
            vignetteComponent.intensity.overrideState = true;
        }
    }

    private void SubscribeToEvents()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void UnsubscribeFromEvents()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (isPerformingRitual && newState != GameState.OnRitual)
        {
            ForceStopRitual();
        }
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        
        TutorialEvents.InvokeRitualAltarUsed();

        mainRitualCoroutine = StartCoroutine(PerformRitual());
    }

    public bool CanInteract()
    {
        if (isPerformingRitual || levelManager == null || playerLife == null)
            return false;

        return IsValidGameStateForRitual(levelManager.GetCurrentGameState());
    }

    private bool IsValidGameStateForRitual(GameState state)
    {
        return state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing;
    }

    private IEnumerator PerformRitual()
    {
        BeginRitual();

        yield return new WaitForSeconds(ritualDuration);

        yield return CompleteRitual();

        EndRitual();
    }

    private void BeginRitual()
    {
        isPerformingRitual = true;
        levelManager.SetGameState(GameState.OnRitual);

        StartRitualEffects();
        UIManager.Instance?.ShowRitualOverlay();
    }

    private IEnumerator CompleteRitual()
    {
        ApplyRitualBenefits();

        UIManager.Instance?.HideRitualOverlay();
        yield return new WaitForSeconds(0.1f);

        if (ShouldTeleportPlayer())
        {
            yield return TeleportPlayer();
        }

        if (canTransitionToNight)
        {
            TutorialEvents.InvokeNightStarted();
            levelManager.TransitionToNight();
        }
    }

    private void EndRitual()
    {
        EndRitualEffects();
        StartCoroutine(FadeInRitualLight());
        isPerformingRitual = false;
        mainRitualCoroutine = null;
    }

    private bool ShouldTeleportPlayer()
    {
        return teleportAfterRitual && teleportDestination != null && player != null;
    }

    private void StartRitualEffects()
    {
        StartCoroutine(RitualLightingSequence());

        if (centerVignetteOnPlayer)
        {
            StartCoroutine(UpdateVignetteContinuously());
        }
    }

    private void EndRitualEffects()
    {
        SoundManager.Instance?.Play("CandleOff");

        StartCoroutine(ExtinguishCandlesGradually());
        StartCoroutine(RestoreVignetteCoroutine());

        if (lightController != null)
        {
            lightController.RestoreLightAfterRitual(GameState.Night, vignetteFadeDuration);
        }

        UpdateAltarAppearance();
    }

    private void ApplyRitualBenefits()
    {
        if (!canRestoreHealth || playerLife == null) return;

        playerLife.currentHealth = playerLife.maxHealth;
        playerLife.onHealthChanged?.Invoke(playerLife.currentHealth, playerLife.maxHealth);

        levelManager.uiManager?.UpdateHealthBar(playerLife.currentHealth, playerLife.maxHealth);
    }

    private IEnumerator RitualLightingSequence()
    {
        StartCoroutine(ApplyRitualVignette());
        lightController?.DimLightForRitual(RITUAL_LIGHT_DIM, vignetteFadeDuration);

        yield return new WaitForSeconds(vignetteFadeDuration);
        yield return LightCandlesSequentially();
    }

    private IEnumerator LightCandlesSequentially()
    {
        if (candleLights == null || candleLights.Length == 0) yield break;

        for (int i = 0; i < candleLights.Length; i++)
        {
            IgniteCandle(i);
            yield return new WaitForSeconds(candleIgnitionDelay);
        }
    }

    private void IgniteCandle(int index)
    {
        if (candleLights[index] == null) return;

        Light2D candle = candleLights[index];
        candle.gameObject.SetActive(true);
        candle.color = candleColor;

        SoundManager.Instance?.Play("CandleOn");
        StartCoroutine(FlickerCandle(candle, index));
    }

    private IEnumerator FlickerCandle(Light2D candle, int candleIndex)
    {
        if (candle == null) yield break;

        float originalIntensity = candle.intensity;
        float randomOffset = candleIndex * 0.5f;
        float maxDuration = ritualDuration - (candleIndex * candleIgnitionDelay);
        float elapsed = 0f;

        while (elapsed < maxDuration && isPerformingRitual)
        {
            elapsed += Time.deltaTime;

            float flicker = Mathf.Sin((elapsed + randomOffset) * candleFlickerSpeed) * candleFlickerAmount;
            float ritualPulse = Mathf.PingPong(elapsed * RITUAL_PULSE_SPEED, 1f) * 0.5f;

            candle.intensity = originalIntensity + flicker + ritualPulse;
            yield return null;
        }

        candle.intensity = originalIntensity;
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

    private IEnumerator ApplyRitualVignette()
    {
        if (!IsVignetteAvailable()) yield break;

        float currentIntensity = vignetteComponent.intensity.value;

        if (centerVignetteOnPlayer && player != null && mainCamera != null)
        {
            CenterVignetteOnPlayer();
        }

        yield return AnimateVignetteIntensity(currentIntensity, ritualVignetteIntensity, vignetteFadeDuration);
    }

    private void CenterVignetteOnPlayer()
    {
        Vector3 playerScreenPos = mainCamera.WorldToViewportPoint(player.transform.position);
        vignetteComponent.center.value = new Vector2(
            Mathf.Clamp01(playerScreenPos.x),
            Mathf.Clamp01(playerScreenPos.y)
        );
    }

    private IEnumerator UpdateVignetteContinuously()
    {
        if (!IsVignetteAvailable() || player == null || mainCamera == null) yield break;

        while (isPerformingRitual)
        {
            CenterVignetteOnPlayer();
            yield return null;
        }
    }

    private IEnumerator AnimateVignetteIntensity(float from, float to, float duration)
    {
        if (!IsVignetteAvailable()) yield break;

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

    private IEnumerator RestoreVignetteCoroutine()
    {
        if (!IsVignetteAvailable()) yield break;

        float targetIntensity = GetVignetteIntensityForState(levelManager.GetCurrentGameState());

        vignetteComponent.center.value = new Vector2(0.5f, 0.5f);
        yield return AnimateVignetteIntensity(vignetteComponent.intensity.value, targetIntensity, 1f);
    }

    private void RestoreVignetteImmediate()
    {
        if (!IsVignetteAvailable()) return;

        GameState currentState = levelManager?.GetCurrentGameState() ?? GameState.Day;
        float targetIntensity = GetVignetteIntensityForState(currentState);

        vignetteComponent.center.value = new Vector2(0.5f, 0.5f);
        vignetteComponent.intensity.value = targetIntensity;
    }

    private float GetVignetteIntensityForState(GameState state)
    {
        return state == GameState.Night ? NIGHT_VIGNETTE_INTENSITY : DAY_VIGNETTE_INTENSITY;
    }

    private bool IsVignetteAvailable()
    {
        return lightController != null &&
               lightController.globalVolume != null &&
               vignetteComponent != null;
    }

    private IEnumerator TeleportPlayer()
    {
        yield return new WaitForSeconds(teleportDelay);

        if (worldTransition != null && worldTransition.IsInInterior)
        {
            worldTransition.SetState(WorldState.Night);
            player.transform.position = teleportDestination.position;
            worldTransition.ExitHouse();
        }
        else
        {
            player.transform.position = teleportDestination.position;
        }

        yield return new WaitForSeconds(0.2f);
    }

    public void ForceStopRitual()
    {
        StopRitualCoroutine();
        CleanupRitualState();
        RestoreEnvironment();
    }

    private void StopRitualCoroutine()
    {
        if (mainRitualCoroutine != null)
        {
            StopCoroutine(mainRitualCoroutine);
            mainRitualCoroutine = null;
        }

        StopAllCoroutines();
    }

    private void CleanupRitualState()
    {
        isPerformingRitual = false;
        TurnOffAllCandles();
        UIManager.Instance?.HideRitualOverlay();
        UpdateAltarAppearance();
    }

    private void RestoreEnvironment()
    {
        RestoreVignetteImmediate();

        if (lightController != null)
        {
            GameState currentState = levelManager?.GetCurrentGameState() ?? GameState.Day;
            lightController.RestoreLightAfterRitual(currentState, 0.5f);
        }
    }

    private void UpdateAltarAppearance()
    {
        if (altarSpriteRenderer != null && defaultSprite != null)
        {
            altarSpriteRenderer.sprite = defaultSprite;
        }
    }

    private IEnumerator FadeInRitualLight()
    {
        if (DoorLight == null)
            yield break;

        DoorLight.gameObject.SetActive(true);
        float startIntensity = 0f;
        float endIntensity = DoorLight.intensity;
        DoorLight.intensity = 1.5f;

        float elapsed = 0f;

        while (elapsed < ritualLightFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ritualLightFadeDuration;
            DoorLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            yield return null;
        }

        DoorLight.intensity = endIntensity;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && altarSpriteRenderer != null && nearSprite != null)
        {
            altarSpriteRenderer.sprite = nearSprite;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UpdateAltarAppearance();
        }
    }
}