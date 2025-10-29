using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlantHealthBar : MonoBehaviour
{
    [Header("Plant References")]
    [SerializeField] private LifeController lifeController;

    [Header("UI Elements")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image whiteFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Settings")]
    [SerializeField] private float whiteBarSpeed = 2f;

    private float targetFill = 1f;

    private void Start()
    {
        if (lifeController == null)
            lifeController = GetComponentInParent<LifeController>();

        if (lifeController != null)
            lifeController.onHealthChanged.AddListener(UpdateHealth);

        if (whiteFillImage != null && healthFillImage != null)
            whiteFillImage.fillAmount = healthFillImage.fillAmount;

        UpdateHealth(lifeController.currentHealth, lifeController.maxHealth);
    }

    private void UpdateHealth(float current, float max)
    {
        if (max <= 0f) return;

        float percent = current / max;
        targetFill = percent;

        if (healthFillImage != null)
            healthFillImage.fillAmount = percent;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void Update()
    {
        UpdateWhiteBar();
    }

    private void UpdateWhiteBar()
    {
        if (whiteFillImage == null || healthFillImage == null) return;

        float current = whiteFillImage.fillAmount;
        float target = targetFill;

        if (current > target)
        {
            whiteFillImage.fillAmount = Mathf.MoveTowards(current, target, whiteBarSpeed * Time.deltaTime);
        }
        else
        {
            whiteFillImage.fillAmount = target;
        }
    }
}
