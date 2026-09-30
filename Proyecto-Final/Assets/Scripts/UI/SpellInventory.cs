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
    [SerializeField] public SpellSlot[] spellSlots = new SpellSlot[7];

    private int selectedSlotIndex = 0;

    public event Action<int> onSpellSlotSelected;
    public event Action<int> onCooldownUpdated;
    public event Action OnSpellInventoryChanged;

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
            if (spellSlots[i] != null && spellSlots[i].currentCooldown > 0f)
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
        if (slot == null || !slot.isUnlocked || slot.spellType == SpellType.None || slot.spellPrefab == null) return false;
        if (slot.currentCooldown > 0f) return false;
        var manaSystem = FindObjectOfType<ManaSystem>();
        if (manaSystem != null && manaSystem.GetCurrentMana() < slot.manaCost)
            return false;

        return true;
    }

    public void StartCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Length) return;

        var slot = spellSlots[slotIndex];
        if (slot != null)
        {
            slot.currentCooldown = slot.cooldown;
            onCooldownUpdated?.Invoke(slotIndex);
        }
    }

    public bool UnlockSpell(SpellDataSO spellData)
    {
        if (spellData == null) return false;

        int slotIndex = spellData.SlotIndex;
        if (slotIndex < 0 || slotIndex >= spellSlots.Length)
        {
            Debug.LogWarning($"[SpellInventory] Invalid slot index {slotIndex} for spell {spellData.SpellName}");
            return false;
        }

        if (spellSlots[slotIndex] == null)
        {
            spellSlots[slotIndex] = new SpellSlot();
        }

        spellSlots[slotIndex].spellType = spellData.SpellType;
        spellSlots[slotIndex].spellName = spellData.SpellName;
        spellSlots[slotIndex].spellIcon = spellData.SpellIcon;
        spellSlots[slotIndex].spellPrefab = spellData.SpellPrefab;
        spellSlots[slotIndex].manaCost = spellData.ManaCost;
        spellSlots[slotIndex].cooldown = spellData.Cooldown;
        spellSlots[slotIndex].isUnlocked = true;
        spellSlots[slotIndex].currentCooldown = 0f;

        var currentSelected = GetSelectedSpellSlot();
        if (currentSelected == null || !currentSelected.isUnlocked || currentSelected.spellType == SpellType.None)
        {
            SelectSlot(slotIndex);
        }

        OnSpellInventoryChanged?.Invoke();
        return true;
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

        if (spellSlots[slotIndex] == null)
        {
            spellSlots[slotIndex] = new SpellSlot();
        }

        spellSlots[slotIndex].spellType = spellType;
        spellSlots[slotIndex].spellName = spellName;
        spellSlots[slotIndex].spellIcon = spellIcon;
        spellSlots[slotIndex].spellPrefab = spellPrefab;
        spellSlots[slotIndex].manaCost = manaCost;
        spellSlots[slotIndex].cooldown = cooldown;
        spellSlots[slotIndex].isUnlocked = true;
        spellSlots[slotIndex].currentCooldown = 0f;

        var currentSelected = GetSelectedSpellSlot();
        if (currentSelected == null || !currentSelected.isUnlocked || currentSelected.spellType == SpellType.None)
        {
            SelectSlot(slotIndex);
        }

        OnSpellInventoryChanged?.Invoke();
        Debug.Log($"Spell unlocked: {spellName} in slot {slotIndex + 1}");
    }

    public void ResetUnlockedSpells()
    {
        if (spellSlots == null) return;

        for (int i = 0; i < spellSlots.Length; i++)
        {
            if (spellSlots[i] != null)
            {
                spellSlots[i].isUnlocked = false;
                spellSlots[i].currentCooldown = 0f;
            }
        }

        OnSpellInventoryChanged?.Invoke();
    }

    public float GetCooldownProgress(int slotIndex)
    {
        var slot = GetSpellSlot(slotIndex);
        if (slot == null || slot.cooldown <= 0f) return 0f;

        return slot.currentCooldown / slot.cooldown;
    }

    public void CycleSpell(int direction)
    {
        if (spellSlots == null || spellSlots.Length == 0) return;
        if (direction == 0) return;

        int step = direction > 0 ? 1 : -1;
        int totalSlots = spellSlots.Length;

        for (int i = 1; i <= totalSlots; i++)
        {
            int candidateIndex = (selectedSlotIndex + (i * step) % totalSlots + totalSlots) % totalSlots;
            var slot = spellSlots[candidateIndex];
            if (slot != null && slot.isUnlocked && slot.spellType != SpellType.None && slot.spellPrefab != null)
            {
                SelectSlot(candidateIndex);
                return;
            }
        }
    }
}