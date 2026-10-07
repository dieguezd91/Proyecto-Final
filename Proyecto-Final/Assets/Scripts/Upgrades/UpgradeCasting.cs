using MagicGarden;
using UnityEngine;

public static class UpgradeCasting
{
    // One fan per attack. Spawned projectiles never recursively spawn another fan.
    public static Spell Cast(GameObject prefab, Vector3 position, Vector2 direction, string id, string capability, bool allowQuantity)
    {
        int quantity = allowQuantity ? Mathf.RoundToInt(UpgradeRuntime.Value(id, Stat.Quantity, 1, capability)) : 1;
        int lateral = Mathf.RoundToInt(UpgradeRuntime.EffectValue(id, EffectKind.LateralProjectiles));
        int count = Mathf.Max(quantity + lateral, 1);
        float angle = UpgradeRuntime.Current?.Balance != null ? UpgradeRuntime.Current.Balance.lateralAngle : 0;
        Spell first = null;
        for (int i = 0; i < count; i++)
        {
            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            var spell = instance.GetComponent<Spell>();
            if (spell == null) { Object.Destroy(instance); continue; }
            spell.SetUpgradeTarget(id, capability);
            // Keep the original center shot, then alternate lateral offsets.
            float offset = i == 0 ? 0 : ((i + 1) / 2) * angle * (i % 2 == 1 ? -1 : 1);
            Vector2 aim = Quaternion.Euler(0, 0, offset) * direction;
            spell.Cast(aim, position);
            if (first == null) first = spell;
        }
        return first;
    }
}
