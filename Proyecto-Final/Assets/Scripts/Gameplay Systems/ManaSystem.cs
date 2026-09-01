using System.Collections;
using UnityEngine;

public class ManaSystem : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] private float baseMaxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float baseDayRegenerationRate = 2f;
    [SerializeField] private float baseNightRegenerationRate = 0.5f;
    [SerializeField] public float modifiedMaxMana;

    [Header("LUNAR INFLUENCE")]
    [SerializeField] private bool useLunarInfluence = true;

    private GamePhase lastPhase = GamePhase.None;
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

        if (LevelManager.Instance != null)
        {
            lastPhase = GameFlowController.Instance.CurrentPhase;
            isNight = lastPhase == GamePhase.Night;
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

        if (LevelManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateManaUI();
        }
    }

    private void Update()
    {
        if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase != lastPhase)
        {
            bool wasNight = lastPhase == GamePhase.Night;
            isNight = GameFlowController.Instance.CurrentPhase == GamePhase.Night;

            if (wasNight != isNight)
            {
                if (isNight)
                {
                    if (useLunarInfluence && lunarInfluenceManager != null)
                    {
                        UpdateMaxMana();
                    }
                }
                else
                {
                    float oldMax = modifiedMaxMana;
                    modifiedMaxMana = baseMaxMana;
                    currentMana = Mathf.Min(currentMana, modifiedMaxMana);

                    OnManaChanged?.Invoke(currentMana, modifiedMaxMana);

                    if (LevelManager.Instance != null && UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateManaUI();
                    }
                }
            }

            lastPhase = GameFlowController.Instance.CurrentPhase;
        }
    }

    public void SetMana(float amount)
    {
        currentMana = Mathf.Clamp(amount, 0f, modifiedMaxMana);
        OnManaChanged?.Invoke(currentMana, modifiedMaxMana);

        if (LevelManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateManaUI();
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

                    if (LevelManager.Instance != null && UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateManaUI();
                    }
                }
            }
            yield return null;
        }
    }

    private float GetCurrentRegenerationRate()
    {
        bool isGameManagerValid = LevelManager.Instance != null;
        bool isNightState = isGameManagerValid && GameFlowController.Instance.CurrentPhase == GamePhase.Night;
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
        if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.Night &&
            useLunarInfluence && lunarInfluenceManager != null)
        {
            float costModifier = lunarInfluenceManager.GetManaCostModifier();
            amount *= costModifier;
        }

        if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.Night &&
            useLunarInfluence && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.GibbousMoon)
        {
            amount *= 0.9f;
        }

        if (currentMana >= amount)
        {
            currentMana -= amount;

            if (LevelManager.Instance != null && UIManager.Instance != null)
            {
                UIManager.Instance.UpdateManaUI();
            }

            OnManaChanged?.Invoke(currentMana, modifiedMaxMana);
            return true;
        }

        return false;
    }

    public void AddMana(float amount)
    {
        if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.Night &&
            useLunarInfluence && lunarInfluenceManager != null && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.CrescentMoon)
        {
            amount *= 1.15f;
        }

        currentMana = Mathf.Min(currentMana + amount, modifiedMaxMana);

        if (LevelManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateManaUI();
        }

        OnManaChanged?.Invoke(currentMana, modifiedMaxMana);
    }

    private void UpdateMaxMana()
    {
        if (useLunarInfluence && lunarInfluenceManager != null)
        {
            float maxManaModifier = lunarInfluenceManager.GetMaxManaModifier();
            float oldMax = modifiedMaxMana;

            if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.Night)
            {
                modifiedMaxMana = baseMaxMana * maxManaModifier;
            }
            else
            {
                modifiedMaxMana = baseMaxMana;
            }

            currentMana = Mathf.Min(currentMana, modifiedMaxMana);

            OnManaChanged?.Invoke(currentMana, modifiedMaxMana);

            if (LevelManager.Instance != null && UIManager.Instance != null)
            {
                UIManager.Instance.UpdateManaUI();
            }
        }
    }

    public void OnMoonPhaseChanged(MoonPhase newPhase)
    {
        if (LevelManager.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.Night)
        {
            if (lunarInfluenceManager != null)
            {
                UpdateMaxMana();
            }
            else
            {
                lunarInfluenceManager = LunarInfluenceManager.Instance;

                if (lunarInfluenceManager != null)
                {
                    UpdateMaxMana();
                }
            }
        }
    }

    public float GetCurrentMana() => currentMana;
    public float GetMaxMana() => modifiedMaxMana;
    public float GetBaseMaxMana() => baseMaxMana;
}

