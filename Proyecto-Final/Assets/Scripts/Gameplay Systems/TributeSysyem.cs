using UnityEngine;

public class TributeSystem : MonoBehaviour
{
    [Header("REWARDS")]
    [SerializeField] private int rewardNoPlantsDestroyed = 30;
    [SerializeField] private int rewardHomeUntouched = 50;
    [SerializeField] private int rewardPlayerUnharmed = 20;

    private bool anyPlantDestroyed = false;
    private bool playerTookDamage = false;
    private float homeHealthAtNightStart;

    public static TributeSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartNightEvaluation()
    {
        anyPlantDestroyed = false;
        playerTookDamage = false;

        if (GameManager.Instance != null && GameManager.Instance.home != null)
        {
            var homeLife = GameManager.Instance.home.GetComponent<LifeController>();
            if (homeLife != null)
                homeHealthAtNightStart = homeLife.currentHealth;
        }
    }

    public void NotifyPlantDestroyed()
    {
        anyPlantDestroyed = true;
    }

    public void NotifyPlayerDamaged()
    {
        playerTookDamage = true;
    }

    public void EvaluateAndGrantReward()
    {
        int totalGold = 0;
        string feedback = "";

        if (!anyPlantDestroyed)
        {
            totalGold += rewardNoPlantsDestroyed;
            feedback += "🌿 Las defensas se mantuvieron firmes. +30 oro\n";
        }

        float currentHomeHealth = GameManager.Instance.home.GetComponent<LifeController>()?.currentHealth ?? 0f;
        if (Mathf.Approximately(currentHomeHealth, homeHealthAtNightStart))
        {
            totalGold += rewardHomeUntouched;
            feedback += "🏠 La casa no recibió daño. +50 oro\n";
        }

        if (!playerTookDamage)
        {
            totalGold += rewardPlayerUnharmed;
            feedback += "🧙‍♀️ La hechicera no fue herida. +20 oro\n";
        }

        if (totalGold > 0)
        {
            InventoryManager.Instance.AddGold(totalGold);
            feedback += $"\n💰 Recompensa total: {totalGold} oro";
        }
        else
        {
            feedback = "😈 Las sombras observan en silencio... No obtuviste recompensa.";
        }

        ShowTributeFeedback(feedback);
    }

    private void ShowTributeFeedback(string message)
    {
        Debug.Log("[Tributo Demoníaco] " + message);
    }
}
