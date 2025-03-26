using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MoonPhase
{
    NewMoon = 0,
    CrescentMoon = 1,
    HalfMoon = 2,
    GibbousMoon = 3,
    FullMoon = 4
}

[System.Serializable]
public class MoonPhaseChangedEvent : UnityEvent<MoonPhase> { }

public class LunarCycleManager : MonoBehaviour
{
    public static LunarCycleManager instance;

    [Header("References")]
    [SerializeField] private SpriteRenderer moonSpriteRenderer;
    [SerializeField] private Sprite[] moonPhaseSprites = new Sprite[5];

    [Header("Settings")]
    [SerializeField] private bool randomInitialPhase = false;
    [SerializeField] private MoonPhase initialPhase = MoonPhase.NewMoon;
    [SerializeField] private bool cyclicProgression = true;

    public MoonPhaseChangedEvent onMoonPhaseChanged;

    private MoonPhase currentMoonPhase;
    private GameState lastGameState = GameState.None;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (onMoonPhaseChanged == null)
            onMoonPhaseChanged = new MoonPhaseChangedEvent();
    }

    private void Start()
    {
        lastGameState = GameManager.Instance.currentGameState;

        if (randomInitialPhase)
        {
            SetMoonPhase((MoonPhase)Random.Range(0, 5));
        }
        else
        {
            SetMoonPhase(initialPhase);
        }

        GameManager.Instance.onNewDay.AddListener(OnNewDay);
    }

    private void Update()
    {
        GameState currentState = GameManager.Instance.currentGameState;

        if (lastGameState == GameState.Day && currentState == GameState.Night)
        {
            Debug.Log($"LunarCycleManager: La noche ha comenzado. Fase lunar actual: {currentMoonPhase}");
            ShowMoon(true);
        }
        else if (lastGameState == GameState.Night && currentState == GameState.Day)
        {
            ShowMoon(false);
        }

        lastGameState = currentState;
    }

    private void OnNewDay(int dayCount)
    {
        if (cyclicProgression)
        {
            MoonPhase nextPhase = (MoonPhase)(((int)currentMoonPhase + 1) % 5);
            SetMoonPhase(nextPhase);
            Debug.Log($"LunarCycleManager: Nuevo día {dayCount}, nueva fase lunar: {nextPhase}");
        }
    }

    public void SetMoonPhase(MoonPhase phase)
    {
        currentMoonPhase = phase;

        if (moonSpriteRenderer != null && moonPhaseSprites.Length == 5)
        {
            moonSpriteRenderer.sprite = moonPhaseSprites[(int)phase];
        }

        onMoonPhaseChanged?.Invoke(currentMoonPhase);
    }

    private void ShowMoon(bool show)
    {
        if (moonSpriteRenderer != null)
        {
            moonSpriteRenderer.enabled = show;
        }
    }

    public MoonPhase GetCurrentMoonPhase()
    {
        return currentMoonPhase;
    }

    public float GetMoonPhaseInfluence(MoonPhaseInfluence influence)
    {
        // Devuelve un valor de influencia basado en la fase lunar actual
        switch (influence)
        {
            case MoonPhaseInfluence.PlantGrowth: // La luna llena es mejor para el crecimiento de plantas
                return GetInfluenceValue(0.2f, 0.4f, 0.6f, 0.8f, 1.0f);

            case MoonPhaseInfluence.EnemyStrength: // La luna nueva es mejor para los enemigos
                return GetInfluenceValue(1.0f, 0.8f, 0.6f, 0.4f, 0.2f);

            case MoonPhaseInfluence.PlayerDamage: // El jugador hace mas daño durante la luna llena
                return GetInfluenceValue(0.8f, 0.85f, 0.9f, 0.95f, 1.1f);

            case MoonPhaseInfluence.ResourceYield: // Los recursos son mas abundantes durante la luna llena
                return GetInfluenceValue(0.7f, 0.8f, 0.9f, 1.0f, 1.2f);

            default:
                return 1.0f;
        }
    }

    private float GetInfluenceValue(float newMoon, float crescent, float half, float gibbous, float full)
    {
        switch (currentMoonPhase)
        {
            case MoonPhase.NewMoon: return newMoon;
            case MoonPhase.CrescentMoon: return crescent;
            case MoonPhase.HalfMoon: return half;
            case MoonPhase.GibbousMoon: return gibbous;
            case MoonPhase.FullMoon: return full;
            default: return 1.0f;
        }
    }
}

public enum MoonPhaseInfluence
{
    PlantGrowth,
    EnemyStrength,
    PlayerDamage,
    ResourceYield
}