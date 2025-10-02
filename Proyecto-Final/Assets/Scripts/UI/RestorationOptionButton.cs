using UnityEngine;
using TMPro;

public class RestorationOptionButton : ImprovedUIButton
{
    [Header("Restoration Option Components")]
    [SerializeField] private TextMeshProUGUI optionLabel;

    private int optionIndex;
    private HouseRestorationSystem.RestorationOption optionData;

    #region Setup
    public void Setup(int index, HouseRestorationSystem.RestorationOption option)
    {
        optionIndex = index;
        optionData = option;

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (optionLabel == null) return;

        bool hasGold = HasSufficientGold();
        bool hasMaterial = HasSufficientMaterial();

        optionLabel.text = FormatOptionText(hasGold, hasMaterial);
        Interactable = hasGold && hasMaterial;
    }
    #endregion

    #region Display Formatting
    private string FormatOptionText(bool hasGold, bool hasMaterial)
    {
        string goldColor = hasGold ? "green" : "red";
        string materialColor = hasMaterial ? "green" : "red";
        string materialIcon = GetMaterialSpriteName(optionData.materialRequired);

        string goldText = $"<color={goldColor}><sprite name=\"GoldIcon\"> {optionData.goldCost}</color>";
        string materialText = $"<color={materialColor}><sprite name=\"{materialIcon}\"> 1</color>";

        return $"{optionData.restorePercentage}% HP\n{goldText}  +  {materialText}";
    }

    private string GetMaterialSpriteName(MaterialType type)
    {
        return type switch
        {
            MaterialType.SpectralCrystal => "SpectralCrystal",
            MaterialType.WindwalkerEssence => "WindwalkerEssence",
            MaterialType.VoltaicCore => "VoltaicCore",
            MaterialType.EternalEmber => "EternalEmber",
            MaterialType.StellarFragment => "StellarFragment",
            MaterialType.LunarEssence => "LunarEssence",
            MaterialType.CrystallizedTears => "CrystallizedTears",
            MaterialType.FlameberryFruit => "FlameberryFruit",
            MaterialType.AstralRoots => "AstralRoots",
            MaterialType.VoltaicPollen => "VoltaicPollen",
            MaterialType.FrostSpores => "FrostSpores",
            MaterialType.EtherealTendrils => "EtherealTendrils",
            MaterialType.HouseHealingPotion => "HouseHealingPotion",
            MaterialType.Gold => "GoldIcon",
            _ => type.ToString(),
        };
    }
    #endregion

    #region Resource Validation
    private bool HasSufficientGold()
    {
        if (InventoryManager.Instance == null) return false;
        int playerGold = InventoryManager.Instance.GetGold();
        return playerGold >= optionData.goldCost;
    }

    private bool HasSufficientMaterial()
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.HasEnoughMaterial(optionData.materialRequired, 1);
    }
    #endregion

    #region Public API
    public int GetOptionIndex() => optionIndex;
    public HouseRestorationSystem.RestorationOption GetOptionData() => optionData;
    #endregion
}