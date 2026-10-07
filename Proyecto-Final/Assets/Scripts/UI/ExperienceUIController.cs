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
            xpText.text = requiredXp > 0 ? $"{currentXp} / {requiredXp} XP" : $"{currentXp} XP (MAX)";
        }

        if (xpSlider != null)
        {
            int floor = playerExperienceSystem != null && playerExperienceSystem.ProgressionData != null
                ? playerExperienceSystem.ProgressionData.GetLevelThreshold(level) : 0;
            xpSlider.minValue = floor;
            xpSlider.maxValue = requiredXp > floor ? requiredXp : floor + 1;
            xpSlider.value = requiredXp > 0 ? currentXp : floor + 1;
        }
    }
}
