using System;
using UnityEngine;

public class HouseRestorationSystem : MonoBehaviour
{
    [Serializable]
    public struct RestorationOption
    {
        public string label;
        public int goldCost;
        public MaterialType materialRequired;
        public int restorePercentage;
    }

    [Header("Restoration Options")]
    public RestorationOption[] options;

    private bool hasRestoredToday = false;

    private void Start()
    {
        GameManager.Instance.onNewDay.AddListener(OnNewDayStarted);
    }

    public bool CanRestore() => !hasRestoredToday;

    public void ResetForNewDay() => hasRestoredToday = false;

    public bool TryRestore(int optionIndex)
    {
        if (hasRestoredToday || optionIndex < 0 || optionIndex >= options.Length)
            return false;

        var opt = options[optionIndex];

        if (InventoryManager.Instance.HasEnoughMaterial(opt.materialRequired, 1) &&
            InventoryManager.Instance.SpendGold(opt.goldCost))
        {
            InventoryManager.Instance.UseMaterial(opt.materialRequired, 1);

            var house = GameManager.Instance.home.GetComponent<HouseLifeController>();
            float missingHealth = house.MaxHealth - house.CurrentHealth;
            float desiredRestore = house.MaxHealth * (opt.restorePercentage / 100f);
            float finalRestore = Mathf.Min(desiredRestore, missingHealth);
            house.Restore(finalRestore);
            GameManager.Instance.uiManager.UpdateHomeHealthBar(house.CurrentHealth, house.MaxHealth);

            hasRestoredToday = true;
            return true;
        }

        return false;
    }

    public RestorationOption GetOption(int index)
    {
        return options[index];
    }

    private void OnNewDayStarted(int day)
    {
        ResetForNewDay();
    }

    public bool HasRestoredToday()
    {
        return hasRestoredToday;
    }

    public int OptionCount => options.Length;
}
