using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private LifeController lifeController;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image whiteFillImage;
    [SerializeField] private float whiteBarSpeed = 2f;

    private float targetFill = 1f;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeOutTime = 1f;


    private float fadeTimer;

    private void Start()
    {
        if (lifeController == null)
            lifeController = GetComponentInParent<LifeController>();

        if (lifeController != null)
            lifeController.onHealthChanged.AddListener(UpdateHealth);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (whiteFillImage != null && healthFillImage != null)
            whiteFillImage.fillAmount = healthFillImage.fillAmount;

    }

    private void UpdateHealth(float current, float max)
    {
        if (healthFillImage != null)
        {
            float percent = current / max;
            healthFillImage.fillAmount = percent;
            targetFill = percent;
        }


        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            fadeTimer = fadeOutTime;
        }
    }

    private void Update()
    {
        if (canvasGroup != null)
        {
            if (fadeTimer > 0f)
            {
                fadeTimer -= Time.deltaTime;
            }
            else if (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime;
            }
        }

        UpdateWhiteBar();

    }

    private void UpdateWhiteBar()
    {
        if (whiteFillImage == null || healthFillImage == null) return;

        float current = whiteFillImage.fillAmount;
        float target = healthFillImage.fillAmount;

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