using UnityEngine;

public class HouseRestorationSystem : MonoBehaviour
{
    [System.Serializable]
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

            var house = GameManager.Instance.home.GetComponent<LifeController>();
            float missingHealth = house.maxHealth - house.currentHealth;
            float desiredRestore = house.maxHealth * (opt.restorePercentage / 100f);
            float finalRestore = Mathf.Min(desiredRestore, missingHealth);
            house.TakeDamage(-finalRestore);
            GameManager.Instance.uiManager.UpdateHomeHealthBar(house.currentHealth, house.maxHealth);

            hasRestoredToday = true;
            return true;
        }

        return false;
    }

    public RestorationOption GetOption(int index)
    {
        return options[index];
    }

    public int OptionCount => options.Length;
}
