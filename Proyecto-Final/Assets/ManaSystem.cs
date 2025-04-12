using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ManaSystem : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float dayRegenerationRate = 2f;
    [SerializeField] private float nightRegenerationRate = 0.5f;

    [Header("MOON PHASES MODIFIERS")]
    [SerializeField] private bool useLunarModifiers = true;

    private GameState lastGameState = GameState.None;
    private LunarCycleManager lunarCycleManager;

    public delegate void ManaChangedHandler(float currentMana, float maxMana);
    public event ManaChangedHandler OnManaChanged;

    private void Awake()
    {
        currentMana = maxMana;
    }

    private void Start()
    {
        lunarCycleManager = LunarCycleManager.instance;
        lastGameState = GameManager.Instance.currentGameState;

        StartCoroutine(RegenerateMana());
        GameManager.Instance.uiManager.UpdateManaUI();
    }

    private void Update()
    {
        if (GameManager.Instance.currentGameState != lastGameState)
        {
            lastGameState = GameManager.Instance.currentGameState;
        }
    }

    private IEnumerator RegenerateMana()
    {
        while (true)
        {
            if (currentMana < maxMana)
            {
                float regenRate = GetCurrentRegenerationRate();
                currentMana = Mathf.Min(currentMana + (regenRate * Time.deltaTime), maxMana);
                GameManager.Instance.uiManager.UpdateManaUI();
            }
            yield return null;
        }
    }

    private float GetCurrentRegenerationRate()
    {
        bool isNight = GameManager.Instance.currentGameState == GameState.Night;
        float baseRate = isNight ? nightRegenerationRate : dayRegenerationRate;

        if (useLunarModifiers && lunarCycleManager != null)
        {
            MoonPhase currentPhase = lunarCycleManager.GetCurrentMoonPhase();

            switch (currentPhase)
            {
                case MoonPhase.NewMoon:
                    if (isNight) baseRate *= 2f;
                    break;
                case MoonPhase.FullMoon:
                    if (isNight) baseRate *= 0.5f;
                    break;
            }
        }

        return baseRate;
    }

    public bool UseMana(float amount)
    {
        if (useLunarModifiers && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.GibbousMoon)
        {
            amount *= 0.9f;
        }

        if (currentMana >= amount)
        {
            currentMana -= amount;
            GameManager.Instance.uiManager.UpdateManaUI();
            OnManaChanged?.Invoke(currentMana, maxMana);
            return true;
        }

        return false;
    }

    public void AddMana(float amount)
    {
        if (useLunarModifiers && lunarCycleManager != null &&
            lunarCycleManager.GetCurrentMoonPhase() == MoonPhase.CrescentMoon)
        {
            amount *= 1.15f;
        }

        currentMana = Mathf.Min(currentMana + amount, maxMana);
        GameManager.Instance.uiManager.UpdateManaUI();
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public float GetCurrentMana() => currentMana;
    public float GetMaxMana() => maxMana;
}