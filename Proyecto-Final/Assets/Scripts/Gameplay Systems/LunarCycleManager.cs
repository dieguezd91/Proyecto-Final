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

        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        //UnityEngine.Cursor.visible = false;
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onNewDay.AddListener(OnNewDay);
            lastGameState = GameManager.Instance.currentGameState;

            if (GameManager.Instance.currentGameState == GameState.Day)
            {
                ShowMoon(false);
            }
            else if (GameManager.Instance.currentGameState == GameState.Night)
            {
                ShowMoon(true);
            }
        }

        Debug.Log($"LunarCycleManager iniciado. Fase lunar inicial: {currentMoonPhase}");
    }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance == null) return;

        GameState currentState = GameManager.Instance.currentGameState;

        if (lastGameState == GameState.Day && currentState == GameState.Night)
        {
            ShowMoon(true);
            Debug.Log($"Transición a noche. Fase lunar actual: {currentMoonPhase}");

            if (!isFirstNight && cyclicProgression)
            {
                int nextPhaseIndex = ((int)currentMoonPhase + 1) % 5;
                SetMoonPhase((MoonPhase)nextPhaseIndex);
                Debug.Log($"Avanzando a nueva fase lunar: {(MoonPhase)nextPhaseIndex}");
            }

            isFirstNight = false;
        }
        else if (lastGameState == GameState.Night && currentState == GameState.Day)
        {
            ShowMoon(false);
            Debug.Log("Transición a día. Ocultando luna.");
        }

        lastGameState = currentState;
    }

    private void OnNewDay(int dayCount)
    {
        Debug.Log($"Nuevo día: {dayCount}. Fase lunar actual: {currentMoonPhase}");
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
            else
            {
                Debug.LogError($"LunarCycleManager: Índice de fase lunar fuera de rango: {phaseIndex}");
            }
        }

        if (manaSystem != null)
        {
            try
            {
                manaSystem.OnMoonPhaseChanged(currentMoonPhase);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al notificar al ManaSystem: {e.Message}");
            }
        }

        if (onMoonPhaseChanged != null)
        {
            try
            {
                onMoonPhaseChanged.Invoke(currentMoonPhase);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al invocar evento de cambio de fase lunar: {e.Message}");
            }
        }

        Debug.Log($"Fase lunar establecida a: {phase}. Notificando a los sistemas...");
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

    public string GetCurrentPhaseDescription()
    {
        switch (currentMoonPhase)
        {
            case MoonPhase.NewMoon:
                return "Luna Nueva: Regeneración de maná nocturno aumentada, enemigos más débiles pero en mayor cantidad.";
            case MoonPhase.CrescentMoon:
                return "Luna Creciente: Las esencias elementales proporcionan más maná, plantas de Hielo y Eléctrico potenciadas.";
            case MoonPhase.HalfMoon:
                return "Media Luna: Equilibrio neutral, sin bonificaciones ni penalizaciones especiales.";
            case MoonPhase.GibbousMoon:
                return "Luna Gibosa: Reducción en costo de hechizos, plantas de Fuego y Viento potenciadas.";
            case MoonPhase.FullMoon:
                return "Luna Llena: Mayor capacidad de maná pero regeneración reducida, enemigos potenciados.";
            default:
                return "Fase lunar desconocida";
        }
    }
}