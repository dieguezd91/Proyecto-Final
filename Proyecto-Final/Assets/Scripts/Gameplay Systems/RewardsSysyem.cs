using UnityEngine;
using System.Collections.Generic;

public class RewardsSystem : MonoBehaviour
{
    [Header("REWARDS")]
    [SerializeField] private int rewardNoPlantsDestroyed = 30;
    [SerializeField] private int rewardHomeUntouched = 50;
    [SerializeField] private int rewardPlayerUnharmed = 20;

    private bool anyPlantDestroyed = false;
    private bool playerTookDamage = false;
    private float homeHealthAtNightStart;

    public static RewardsSystem Instance { get; private set; }

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

        if (LevelManager.Instance != null && LevelManager.Instance.home != null)
        {
            var homeLife = LevelManager.Instance.home.GetComponent<LifeController>();
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
        List<FloatingTextController.GoldReward> rewards = new List<FloatingTextController.GoldReward>();
        int totalGold = 0;

        if (!anyPlantDestroyed)
        {
            rewards.Add(new FloatingTextController.GoldReward
            {
                goldAmount = rewardNoPlantsDestroyed,
                message = "Defenses Intact",
                textColor = new Color(0.3f, 0.9f, 0.4f)
            });
            totalGold += rewardNoPlantsDestroyed;
        }

        float currentHomeHealth = LevelManager.Instance.home.GetComponent<LifeController>()?.currentHealth ?? 0f;
        if (Mathf.Approximately(currentHomeHealth, homeHealthAtNightStart))
        {
            rewards.Add(new FloatingTextController.GoldReward
            {
                goldAmount = rewardHomeUntouched,
                message = "Home Protected",
                textColor = new Color(1f, 0.8f, 0.2f)
            });
            totalGold += rewardHomeUntouched;
        }

        if (!playerTookDamage)
        {
            rewards.Add(new FloatingTextController.GoldReward
            {
                goldAmount = rewardPlayerUnharmed,
                message = "Witch Unharmed",
                textColor = new Color(0.7f, 0.4f, 1f)
            });
            totalGold += rewardPlayerUnharmed;
        }

        if (totalGold > 0)
        {
            InventoryManager.Instance.AddGold(totalGold);
        }

        if (FloatingTextController.Instance != null)
        {
            FloatingTextController.Instance.ShowGoldFeedbackSequence(rewards, totalGold);
        }
    }
}