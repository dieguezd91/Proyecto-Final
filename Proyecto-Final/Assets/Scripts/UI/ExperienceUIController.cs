using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private PlayerExperienceSystem playerExperienceSystem;

    private bool isSubscribed;

    private void OnEnable()
    {
        EnsurePlayerExperienceSystem();
        SubscribeToEvents();
        UpdateUIFromCurrentState();
    }

    private void Start()
    {
        EnsurePlayerExperienceSystem();
        SubscribeToEvents();
        UpdateUIFromCurrentState();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void EnsurePlayerExperienceSystem()
    {
        if (playerExperienceSystem == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerExperienceSystem = player.GetComponent<PlayerExperienceSystem>();
            }

            if (playerExperienceSystem == null)
            {
                playerExperienceSystem = FindObjectOfType<PlayerExperienceSystem>();
            }
        }
    }

    private void SubscribeToEvents()
    {
        if (playerExperienceSystem != null && !isSubscribed)
        {
            playerExperienceSystem.OnExperienceChanged += HandleExperienceChanged;
            isSubscribed = true;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (isSubscribed)
        {
            if (playerExperienceSystem != null)
            {
                playerExperienceSystem.OnExperienceChanged -= HandleExperienceChanged;
            }
            isSubscribed = false;
        }
    }

    private void UpdateUIFromCurrentState()
    {
        if (playerExperienceSystem != null)
        {
            HandleExperienceChanged(
                playerExperienceSystem.CurrentLevel,
                playerExperienceSystem.CurrentExperience,
                playerExperienceSystem.ExperienceRequired
            );
        }
    }

    private void HandleExperienceChanged(int level, int currentXp, int requiredXp)
    {
        if (levelText != null)
        {
            levelText.text = $"LV. {level}";
        }

        if (xpText != null)
        {
            xpText.text = $"{currentXp} / {requiredXp} XP";
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = requiredXp;
            xpSlider.value = currentXp;
        }
    }
}
