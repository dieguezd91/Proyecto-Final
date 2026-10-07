using UnityEngine;

[CreateAssetMenu(fileName = "New Spell Data", menuName = "Game Data/Spell")]
public class SpellDataSO : ScriptableObject
{
    [Tooltip("Optional stable run target id; defaults to spell:<slotIndex>.")]
    [SerializeField] private string upgradeTargetId;
    public string UpgradeTargetId => string.IsNullOrEmpty(upgradeTargetId) ? "spell:" + slotIndex : upgradeTargetId;
    [SerializeField] private int slotIndex;
    [SerializeField] private SpellType spellType;
    [SerializeField] private string spellName;
    [SerializeField] private Sprite spellIcon;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private int manaCost;
    [SerializeField] private float cooldown;

    public int SlotIndex => slotIndex;
    public SpellType SpellType => spellType;
    public string SpellName => spellName;
    public Sprite SpellIcon => spellIcon;
    public GameObject SpellPrefab => spellPrefab;
    public int ManaCost => Mathf.Max(0, manaCost);
    public float Cooldown => Mathf.Max(0f, cooldown);

    private void OnValidate()
    {
        if (slotIndex < 0) slotIndex = 0;
        if (manaCost < 0) manaCost = 0;
        if (cooldown < 0f) cooldown = 0f;
    }
}
