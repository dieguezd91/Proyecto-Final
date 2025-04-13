using System.Collections;
using UnityEngine;

public class ManaSystem : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] private float baseMaxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float baseDayRegenerationRate = 2f;
    [SerializeField] private float baseNightRegenerationRate = 0.5f;
    [SerializeField] private float modifiedMaxMana;

    [Header("LUNAR INFLUENCE")]
    [SerializeField] private bool useLunarInfluence = true;

    private GameState lastGameState = GameState.None;
    private LunarCycleManager lunarCycleManager;
    private LunarInfluenceManager lunarInfluenceManager;
    private bool isNight = false;

    public delegate void ManaChangedHandler(float currentMana, float maxMana);
    public event ManaChangedHandler OnManaChanged;

    private void Awake()
    {
        modifiedMaxMana = baseMaxMana;
        currentMana = modifiedMaxMana;
    }

    private void Start()
    {
        if (lunarCycleManager == null)
        {
            lunarCycleManager = LunarCycleManager.Instance;
        }

        if (lunarInfluenceManager == null)
        {
            lunarInfluenceManager = LunarInfluenceManager.Instance;
        }

        if (GameManager.Instance != null)
        {
            lastGameState = GameManager.Instance.currentGameState;
            isNight = lastGameState == GameState.Night;
        }

        if (isNight)
        {
            if (useLunarInfluence && lunarInfluenceManager != null)
            {
                UpdateMaxMana();
            }
        }
        else
        {
            modifiedMaxMana = baseMaxMana;
            currentMana = Mathf.Min(currentMana, modifiedMaxMana);
        }

        StartCoroutine(RegenerateMana());

        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateManaUI();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != lastGameState)
        {
            bool wasNight = lastGameState == GameState.Night;
            isNight = GameManager.Instance.currentGameState == GameState.Night;

            if (wasNight != isNight)
            {
                if (isNight)
                {
                    if (useLunarInfluence && lunarInfluenceManager != null)
                    {
                        UpdateMaxMana();
                        Debug.Log("Transición a noche: Aplicando modificadores lunares al maná");
                    }
                }
                else
                {
                    float oldMax = modifiedMaxMana;
                    modifiedMaxMana = baseMaxMana;
                    currentMana = Mathf.Min(currentMana, modifiedMaxMana);

                    OnManaChanged?.Invoke(currentMana, modifiedMaxMana);

                    if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                    {
                        GameManager.Instance.uiManager.UpdateManaUI();
                    }

                    Debug.Log($"Transición a día: Restaurando maná máximo base: {oldMax} -> {modifiedMaxMana}");
                }
            }

            lastGameState = GameManager.Instance.currentGameState;
        }
    }

    private IEnumerator RegenerateMana()
    {
        while (true)
        {
            if (currentMana < modifiedMaxMana)
            {
                float regenRate = GetCurrentRegenerationRate();
                float manaToAdd = regenRate * Time.deltaTime;

                if (manaToAdd > 0)
                {
                    currentMana = Mathf.Min(currentMana + manaToAdd, modifiedMaxMana);

                    if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                    {
                        GameManager.Instance.uiManager.UpdateManaUI();
                    }
                }
            }
            yield return null;
        }
    }

    private float GetCurrentRegenerationRate()
    {
        bool isGameManagerValid = GameManager.Instance != null;
        bool isNightState = isGameManagerValid && GameManager.Instance.currentGameState == GameState.Night;
        float baseRate = isNightState ? baseNightRegenerationRate : baseDayRegenerationRate;
        float lunarModifier = 1.0f;

        if (!isGameManagerValid)
        {
            return baseRate;
        }

        if (isNightState && useLunarInfluence && lunarInfluenceManager != null && lunarCycleManager != null)
        {
            lunarModifier = lunarInfluenceManager.GetManaRegenerationModifier();

            MoonPhase currentPhase = lunarCycleManager.GetCurrentMoonPhase();
            if (currentPhase == MoonPhase.NewMoon)
            {
                return 1.0f;
            }
            else if (currentPhase == MoonPhase.FullMoon)
            {
                return 0.25f;
            }
        }

        return baseRate * lunarModifier;
    }

    public bool UseMana(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night &&
            useLunarInfluence && lunarInfluenceManager != null)
        {
            float costModifier = lunarInfluenceManager.GetManaCostModifier();
            amount *= costModifier;
        }

        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night &&
            useLunarInfluence && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.GibbousMoon)
        {
            amount *= 0.9f;
        }

        if (currentMana >= amount)
        {
            currentMana -= amount;

            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.UpdateManaUI();
            }

            OnManaChanged?.Invoke(currentMana, modifiedMaxMana);
            return true;
        }

        return false;
    }

    public void AddMana(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night &&
            useLunarInfluence && lunarInfluenceManager != null && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.CrescentMoon)
        {
            amount *= 1.15f;
        }

        currentMana = Mathf.Min(currentMana + amount, modifiedMaxMana);

        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateManaUI();
        }

        OnManaChanged?.Invoke(currentMana, modifiedMaxMana);
    }

    private void UpdateMaxMana()
    {
        if (useLunarInfluence && lunarInfluenceManager != null)
        {
            float maxManaModifier = lunarInfluenceManager.GetMaxManaModifier();
            float oldMax = modifiedMaxMana;

            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night)
            {
                modifiedMaxMana = baseMaxMana * maxManaModifier;
            }
            else
            {
                modifiedMaxMana = baseMaxMana;
            }

            currentMana = Mathf.Min(currentMana, modifiedMaxMana);

            OnManaChanged?.Invoke(currentMana, modifiedMaxMana);

            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.UpdateManaUI();
            }

            Debug.Log($"Maná máximo actualizado: {oldMax} -> {modifiedMaxMana} (Modificador lunar: x{maxManaModifier})");
        }
    }

    public void OnMoonPhaseChanged(MoonPhase newPhase)
    {
        Debug.Log($"Fase lunar cambiada a: {newPhase}");

        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Night)
        {
            if (lunarInfluenceManager != null)
            {
                UpdateMaxMana();
            }
            else
            {
                Debug.LogWarning("ManaSystem: No se pudo actualizar el maná porque LunarInfluenceManager es nulo");

                lunarInfluenceManager = LunarInfluenceManager.Instance;

                if (lunarInfluenceManager != null)
                {
                    UpdateMaxMana();
                }
                else
                {
                    Debug.LogError("ManaSystem: No se pudo encontrar LunarInfluenceManager en la escena");
                }
            }
        }
    }

    public float GetCurrentMana() => currentMana;
    public float GetMaxMana() => modifiedMaxMana;
    public float GetBaseMaxMana() => baseMaxMana;
}