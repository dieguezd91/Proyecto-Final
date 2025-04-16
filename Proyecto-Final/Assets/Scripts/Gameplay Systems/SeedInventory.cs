using System;
using UnityEngine;

[Serializable]
public class PlantSlot
{
    public SeedsEnum seedType;
    public GameObject plantPrefab;
    public string plantName;
    public Sprite plantIcon;
    public int daysToGrow;
    public string description;
}

public class SeedInventory : MonoBehaviour
{
    public static SeedInventory Instance;

    [Header("SEED INVENTORY")]
    [SerializeField] private PlantSlot[] plantSlots = new PlantSlot[5];
    [SerializeField] private int selectedSlotIndex = 0;

    [Header("UI EVENTS")]
    public Action<int> onSlotSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < plantSlots.Length; i++)
        {
            if (plantSlots[i] == null)
                plantSlots[i] = new PlantSlot();
        }

        Debug.Log("SeedInventory initialized");
    }

    private void Start()
    {
        SelectSlot(0);
    }

    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < plantSlots.Length)
        {
            selectedSlotIndex = slotIndex;
            onSlotSelected?.Invoke(selectedSlotIndex);
        }
    }

    public GameObject GetSelectedPlantPrefab()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < plantSlots.Length)
        {
            return plantSlots[selectedSlotIndex].plantPrefab;
        }
        return null;
    }

    public PlantSlot GetPlantSlot(int index)
    {
        if (index >= 0 && index < plantSlots.Length)
        {
            return plantSlots[index];
        }
        return null;
    }

    public PlantSlot GetSelectedPlantSlot()
    {
        return GetPlantSlot(selectedSlotIndex);
    }

    public string GetSelectedPlantName()
    {
        PlantSlot slot = GetSelectedPlantSlot();
        return slot != null ? slot.plantName : "None";
    }

    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public void UnlockPlant(
        SeedsEnum seedType,
        GameObject plantPrefab,
        string plantName,
        Sprite plantIcon,
        int slotIndex,
        int daysToGrow,
        string description = "")
    {
        if (slotIndex >= 0 && slotIndex < plantSlots.Length)
        {
            plantSlots[slotIndex].seedType = seedType;
            plantSlots[slotIndex].plantPrefab = plantPrefab;
            plantSlots[slotIndex].plantName = plantName;
            plantSlots[slotIndex].plantIcon = plantIcon;
            plantSlots[slotIndex].daysToGrow = daysToGrow;
            plantSlots[slotIndex].description = description;

            Debug.Log($"Unlocked new plant: {plantName} (Seed: {seedType}) in slot {slotIndex + 1}");
        }
    }
}