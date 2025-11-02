using System;
using UnityEngine;

public enum SpellType
{
    None = 0,
    Range = 1,
    Melee = 2,
    Area = 3,
    Teleport = 4,
}

[Serializable]
public class SpellSlot
{
    public SpellType spellType;
    public string spellName;
    public Sprite spellIcon;
    public GameObject spellPrefab;
    public int manaCost;
    public float cooldown;
    public float currentCooldown;
    public bool isUnlocked;

    public SpellSlot()
    {
        spellType = SpellType.None;
        spellName = "Empty";
        spellIcon = null;
        spellPrefab = null;
        manaCost = 0;
        cooldown = 0f;
        currentCooldown = 0f;
        isUnlocked = false;
    }
}

public class SpellInventory : MonoBehaviour
{
    public static SpellInventory Instance { get; private set; }

    [Header("Spell Slots Configuration")]
    [SerializeField] private SpellSlot[] spellSlots = new SpellSlot[7];

    private int selectedSlotIndex = 0;

    public event Action<int> onSpellSlotSelected;
    public event Action<int> onCooldownUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSpellSlots();
    }

    private void Update()
    {
        UpdateCooldowns();
    }

    private void UpdateCooldowns()
    {
        for (int i = 0; i < spellSlots.Length; i++)
        {
            if (spellSlots[i].currentCooldown > 0f)
            {
                spellSlots[i].currentCooldown -= Time.deltaTime;

                if (spellSlots[i].currentCooldown <= 0f)
                {
                    spellSlots[i].currentCooldown = 0f;
                }

                onCooldownUpdated?.Invoke(i);
            }
        }
    }

    private void InitializeSpellSlots()
    {
        for (int i = 0; i < spellSlots.Length; i++)
        {
            if (spellSlots[i] == null)
            {
                spellSlots[i] = new SpellSlot();
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= spellSlots.Length) return;

        selectedSlotIndex = index;
        onSpellSlotSelected?.Invoke(selectedSlotIndex);
    }

    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public SpellSlot GetSelectedSpellSlot()
    {
        return GetSpellSlot(selectedSlotIndex);
    }

    public SpellSlot GetSpellSlot(int index)
    {
        if (index < 0 || index >= spellSlots.Length) return null;
        return spellSlots[index];
    }

    public bool CanCastSelectedSpell()
    {
        var slot = GetSelectedSpellSlot();
        if (slot == null || !slot.isUnlocked) return false;
        if (slot.currentCooldown > 0f) return false;

        // Verificar si hay suficiente maná
        var manaSystem = FindObjectOfType<ManaSystem>();
        if (manaSystem != null && manaSystem.GetCurrentMana() < slot.manaCost)
            return false;

        return true;
    }

    public void StartCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Length) return;

        var slot = spellSlots[slotIndex];
        slot.currentCooldown = slot.cooldown;
        onCooldownUpdated?.Invoke(slotIndex);
    }

    public void UnlockSpell(
        int slotIndex,
        SpellType spellType,
        string spellName,
        Sprite spellIcon,
        GameObject spellPrefab,
        int manaCost,
        float cooldown)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Length) return;

        spellSlots[slotIndex].spellType = spellType;
        spellSlots[slotIndex].spellName = spellName;
        spellSlots[slotIndex].spellIcon = spellIcon;
        spellSlots[slotIndex].spellPrefab = spellPrefab;
        spellSlots[slotIndex].manaCost = manaCost;
        spellSlots[slotIndex].cooldown = cooldown;
        spellSlots[slotIndex].isUnlocked = true;
        spellSlots[slotIndex].currentCooldown = 0f;

        Debug.Log($"Spell unlocked: {spellName} in slot {slotIndex + 1}");
    }

    public float GetCooldownProgress(int slotIndex)
    {
        var slot = GetSpellSlot(slotIndex);
        if (slot == null || slot.cooldown <= 0f) return 0f;

        return slot.currentCooldown / slot.cooldown;
    }
}