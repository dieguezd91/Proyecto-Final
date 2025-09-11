using System.Collections;
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
    public static LunarCycleManager Instance;

    [Header("REFERENCES")]
    [SerializeField] private SpriteRenderer moonSpriteRenderer;
    [SerializeField] private Sprite[] moonPhaseSprites = new Sprite[5];

    [Header("SETTINGS")]
    [SerializeField] private bool cyclicProgression = true;

    [SerializeField] private ManaSystem manaSystem;

    public MoonPhaseChangedEvent onMoonPhaseChanged;
    private MoonPhase currentMoonPhase;
    private GameState lastGameState = GameState.None;
    private bool isFirstNight = true;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (onMoonPhaseChanged == null)
            onMoonPhaseChanged = new MoonPhaseChangedEvent();

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Start()
    {
        if (manaSystem == null)
        {
            manaSystem = FindObjectOfType<ManaSystem>();
        }

        StartCoroutine(InitializeDelayed());
    }

    private IEnumerator InitializeDelayed()
    {
        yield return null;

        SetMoonPhase(MoonPhase.NewMoon);
        isInitialized = true;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onNewDay.AddListener(OnNewDay);
            lastGameState = LevelManager.Instance.currentGameState;

            if (LevelManager.Instance.currentGameState != GameState.Night)
            {
                ShowMoon(false);
            }
            else if (LevelManager.Instance.currentGameState == GameState.Night)
            {
                ShowMoon(true);
            }
        }
    }

    private void Update()
    {
        if (!isInitialized || LevelManager.Instance == null) return;

        GameState currentState = LevelManager.Instance.currentGameState;

        if (WasDayState(lastGameState) && currentState == GameState.Night)
        {
            ShowMoon(true);

            if (!isFirstNight && cyclicProgression)
            {
                int nextPhaseIndex = ((int)currentMoonPhase + 1) % 5;
                SetMoonPhase((MoonPhase)nextPhaseIndex);
            }

            isFirstNight = false;
        }
        else if (lastGameState == GameState.Night && currentState != GameState.Night)
        {
            ShowMoon(false);
        }

        lastGameState = currentState;
    }

    private void OnNewDay(int dayCount)
    {
        Debug.Log($"Nuevo d�a: {dayCount}. Fase lunar actual: {currentMoonPhase}");
    }

    public void SetMoonPhase(MoonPhase phase)
    {
        currentMoonPhase = phase;

        if (moonSpriteRenderer != null && moonPhaseSprites != null && moonPhaseSprites.Length == 5)
        {
            int phaseIndex = (int)phase;
            if (phaseIndex >= 0 && phaseIndex < moonPhaseSprites.Length)
            {
                moonSpriteRenderer.sprite = moonPhaseSprites[phaseIndex];
            }
        }

        if (manaSystem != null)
        {
            manaSystem.OnMoonPhaseChanged(currentMoonPhase);
        }

        if (onMoonPhaseChanged != null)
        {
            onMoonPhaseChanged.Invoke(currentMoonPhase);
        }
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

    private bool WasDayState(GameState state)
    {
        return state == GameState.Day
            || state == GameState.Digging
            || state == GameState.Planting
            || state == GameState.Harvesting
            || state == GameState.Removing;
    }
}