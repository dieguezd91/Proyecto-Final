using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaUIController : UIControllerBase
{
    [Header("Mana UI")]
    [SerializeField] private Slider manaBar;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Gradient manaGradient;
    [SerializeField] private TextMeshProUGUI manaText;

    private ManaSystem manaSystem;

    protected override void CacheReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            manaSystem = player.GetComponent<ManaSystem>();
    }

    protected override void ConfigureInitialState()
    {
        if (manaSystem != null && manaBar != null)
        {
            manaBar.maxValue = manaSystem.GetMaxMana();
            manaBar.value = manaSystem.GetCurrentMana();
            UpdateManaDisplay();
        }
    }

    public void UpdateMana()
    {
        if (manaBar == null || manaSystem == null) return;

        float current = manaSystem.GetCurrentMana();
        float max = manaSystem.GetMaxMana();
        float percentage = Mathf.Clamp01(current / max);

        manaBar.maxValue = max;
        manaBar.value = current;

        UpdateManaFillColor(percentage);
        UpdateManaText(current, max);
    }

    private void UpdateManaDisplay()
    {
        UpdateMana();
    }

    private void UpdateManaFillColor(float percentage)
    {
        if (manaFillImage != null && manaGradient != null)
            manaFillImage.color = manaGradient.Evaluate(percentage);
    }

    private void UpdateManaText(float current, float max)
    {
        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public IEnumerator AnimateRespawnRecovery(float duration)
    {
        if (manaSystem == null) yield break;

        float startMana = 0f;
        float endMana = manaSystem.GetMaxMana();
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float currentMana = Mathf.Lerp(startMana, endMana, t);

            manaSystem.SetMana(currentMana);
            UpdateMana();

            time += Time.deltaTime;
            yield return null;
        }

        manaSystem.SetMana(endMana);
        UpdateMana();
    }
}

