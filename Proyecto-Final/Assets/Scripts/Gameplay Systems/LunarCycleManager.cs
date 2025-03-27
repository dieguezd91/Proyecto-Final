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
    [SerializeField] private MoonPhase initialPhase;
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
            Debug.Log($"Nueva fase lunar: {nextPhase}");
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
}

public enum MoonPhaseInfluence
{
    PlantGrowth,
    EnemyStrength,
    PlayerDamage,
    ResourceYield
}