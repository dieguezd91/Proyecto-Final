using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUIController : UIControllerBase
{
    [Header("Player Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image whiteFillImage;
    [SerializeField] private float whiteBarSpeed = 1.5f;

    [Header("Home Health")]
    [SerializeField] private Slider homeHealthBar;
    [SerializeField] private Image homeFillImage;
    [SerializeField] private Gradient homeHealthGradient;
    [SerializeField] private TextMeshProUGUI homeHealthText;

    private float targetFill = 1f;
    private LifeController playerLife;
    private HouseLifeController homeLife;
    private GameObject player;

    protected override void CacheReferences()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerLife = player.GetComponent<LifeController>();

        if (LevelManager.Instance?.home != null)
            homeLife = LevelManager.Instance.home.GetComponent<HouseLifeController>();
    }

    protected override void SetupEventListeners()
    {
        if (playerLife != null)
            playerLife.onHealthChanged.AddListener(UpdatePlayerHealth);

        if (homeLife != null)
            homeLife.onHealthChanged.AddListener(UpdateHomeHealth);
    }

    protected override void ConfigureInitialState()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = playerLife?.maxHealth ?? 100f;
            healthBar.value = playerLife?.currentHealth ?? 100f;
        }

        if (homeHealthBar != null && homeLife != null)
        {
            homeHealthBar.minValue = 0;
            homeHealthBar.maxValue = homeLife.MaxHealth;
            homeHealthBar.value = homeLife.CurrentHealth;
        }

        if (whiteFillImage != null && playerLife != null)
            whiteFillImage.fillAmount = playerLife.GetHealthPercentage();
    }

    public override void HandleUpdate()
    {
        UpdateWhiteBar();
    }

    public void UpdatePlayerHealth(float currentHealth, float maxHealth)
    {
        if (healthBar == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFill = healthPercentage;

        healthBar.value = currentHealth;
        UpdatePlayerFillColor(healthPercentage);
        UpdatePlayerHealthText(currentHealth, maxHealth);
    }

    public void UpdateHomeHealth(float currentHealth, float maxHealth)
    {
        if (homeHealthBar == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        homeHealthBar.value = currentHealth;
        UpdateHomeFillColor(healthPercentage);
        UpdateHomeHealthText(currentHealth, maxHealth);
    }

    private void UpdatePlayerFillColor(float healthPercentage)
    {
        if (fillImage != null && healthGradient != null)
            fillImage.color = healthGradient.Evaluate(healthPercentage);
    }

    private void UpdateHomeFillColor(float healthPercentage)
    {
        if (homeFillImage != null && homeHealthGradient != null)
            homeFillImage.color = homeHealthGradient.Evaluate(healthPercentage);
    }

    private void UpdatePlayerHealthText(float current, float max)
    {
        if (playerHealthText != null)
            playerHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateHomeHealthText(float current, float max)
    {
        if (homeHealthText != null)
            homeHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateWhiteBar()
    {
        if (whiteFillImage == null || fillImage == null) return;

        float current = whiteFillImage.fillAmount;
        float target = fillImage.fillAmount;

        if (current > target)
        {
            whiteFillImage.fillAmount = Mathf.MoveTowards(current, target, whiteBarSpeed * Time.deltaTime);
        }
        else
        {
            whiteFillImage.fillAmount = target;
        }
    }

    public IEnumerator AnimateRespawnRecovery(float duration)
    {
        if (playerLife == null) yield break;

        float startHealth = 0f;
        float endHealth = playerLife.maxHealth;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float currentHealth = Mathf.Lerp(startHealth, endHealth, t);

            UpdatePlayerHealth(currentHealth, playerLife.maxHealth);

            time += Time.deltaTime;
            yield return null;
        }

        UpdatePlayerHealth(endHealth, playerLife.maxHealth);
    }

    protected override void CleanupEventListeners()
    {
        if (playerLife != null)
            playerLife.onHealthChanged.RemoveListener(UpdatePlayerHealth);

        if (homeLife != null)
            homeLife.onHealthChanged.RemoveListener(UpdateHomeHealth);
    }
}