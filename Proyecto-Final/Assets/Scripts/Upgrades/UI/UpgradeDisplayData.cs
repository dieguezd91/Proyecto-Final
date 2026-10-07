using UnityEngine;

// Presentation snapshots only; upgrade eligibility and mutations remain in UpgradeRuntime.
public sealed class LevelUpCardData
{
    public Sprite Icon;
    public string Target, Name, Stat, Bonus, Rarity, Preview;
    public Color Tint;
}

public sealed class SpecialUpgradeItemData
{
    public Sprite Icon;
    public string Target, Name, Description, Costs, Prerequisites, Stacks;
    public bool CanPurchase;
}
