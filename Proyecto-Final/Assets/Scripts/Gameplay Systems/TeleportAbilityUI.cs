using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleportAbilityUI : UIControllerBase
{
    [Header("UI References")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private PlayerAbilitySystem abilitySystem;
    private ManaSystem manaSystem;

    protected override void CacheReferences()
    {
        abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        manaSystem = FindObjectOfType<ManaSystem>();
    }

    protected override void SetupEventListeners()
    {
        if (abilitySystem != null)
        {
            abilitySystem.OnTeleportCooldownChanged += UpdateCooldownDisplay;
        }
    }

    protected override void CleanupEventListeners()
    {
        if (abilitySystem != null)
        {
            abilitySystem.OnTeleportCooldownChanged -= UpdateCooldownDisplay;
        }
    }

    protected override void ConfigureInitialState()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (abilitySystem == null) return;

        UpdateCooldownDisplay(abilitySystem.CurrentTeleportCooldown, abilitySystem.TeleportCooldown);
    }

    private void UpdateCooldownDisplay(float current, float max)
    {
        if (cooldownFillImage != null)
        {
            float fillAmount = max > 0f ? current / max : 0f;
            cooldownFillImage.fillAmount = fillAmount;
        }

        if (cooldownText != null)
        {
            if (current > 0f)
            {
                cooldownText.text = Mathf.Ceil(current).ToString("F0");
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }

    public override void HandleUpdate()
    {
        if (abilitySystem != null && manaSystem != null)
        {
            bool canUse = abilitySystem.CanUseTeleport();
        }
    }
}