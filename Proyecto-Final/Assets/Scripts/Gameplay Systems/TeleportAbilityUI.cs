using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleportAbilityUI : UIControllerBase
{
    [Header("UI References")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private PlayerTeleportController teleportController;

    protected override void CacheReferences()
    {
        teleportController = FindObjectOfType<PlayerTeleportController>();
    }

    protected override void SetupEventListeners()
    {
        if (teleportController != null)
        {
            teleportController.OnTeleportCooldownChanged += UpdateCooldownDisplay;
        }
    }

    protected override void CleanupEventListeners()
    {
        if (teleportController != null)
        {
            teleportController.OnTeleportCooldownChanged -= UpdateCooldownDisplay;
        }
    }

    protected override void ConfigureInitialState()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (teleportController == null) return;

        UpdateCooldownDisplay(teleportController.CurrentTeleportCooldown, teleportController.TeleportCooldown);
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
}