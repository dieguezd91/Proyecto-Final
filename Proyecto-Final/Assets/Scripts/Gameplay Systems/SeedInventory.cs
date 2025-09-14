using System;
using System.Collections;
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

    public int seedCount = 0;

    public PlantDataSO data;

}

public class SeedInventory : MonoBehaviour
{
    public static SeedInventory Instance;

    [Header("SEED INVENTORY")]
    [SerializeField] private PlantSlot[] plantSlots = new PlantSlot[9];
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
    }

    private void Start()
    {
        StartCoroutine(DelayedSelectFirstSlot());
    }

    private IEnumerator DelayedSelectFirstSlot()
    {
        yield return null;
        SelectSlot(0);
    }

    //private void Update()
    //{
    //    for (int i = 0; i < 9; i++)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Alpha1 + i))
    //        {
    //            SelectSlot(i);
    //        }
    //    }
    //}

    public void RemoveSeedFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < plantSlots.Length)
        {
            plantSlots[slotIndex].seedType = SeedsEnum.None;
            plantSlots[slotIndex].plantPrefab = null;
            plantSlots[slotIndex].plantName = "";
            plantSlots[slotIndex].plantIcon = null;
            plantSlots[slotIndex].daysToGrow = 0;
            plantSlots[slotIndex].description = "";
            plantSlots[slotIndex].seedCount = 0;

            Debug.Log($"Slot {slotIndex + 1} vaciado");
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < plantSlots.Length)
        {
            selectedSlotIndex = slotIndex;
            onSlotSelected?.Invoke(selectedSlotIndex);
        }
        LevelManager.Instance.uiManager.UpdateSeedCountsUI();
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

    public bool HasSeedsInSelectedSlot()
    {
        var slot = GetSelectedPlantSlot();
        return slot != null && slot.seedCount > 0;
    }

    public void ConsumeSeedInSelectedSlot()
    {
        var slot = GetSelectedPlantSlot();
        if (slot != null && slot.seedCount > 0)
        {
            slot.seedCount--;
            //Debug.Log($"Semillas restantes de {slot.plantName}: {slot.seedCount}");

            if (slot.seedCount <= 0)
            {
                int index = selectedSlotIndex;
                RemoveSeedFromSlot(index);
            }

            LevelManager.Instance.uiManager.UpdateSeedCountsUI();
            LevelManager.Instance.uiManager.InitializeSeedSlotsUI();
        }
    }

    public int GetSeedCountInSlot(int index)
    {
        var slot = GetPlantSlot(index);
        return slot != null ? slot.seedCount : 0;
    }

    public void UnlockPlant(
        SeedsEnum seedType,
        GameObject plantPrefab,
        string plantName,
        Sprite plantIcon,
        int slotIndex,
        int daysToGrow,
        string description = "",
        PlantDataSO data = null)
    {
        if (slotIndex >= 0 && slotIndex < plantSlots.Length)
        {
            plantSlots[slotIndex].seedType = seedType;
            plantSlots[slotIndex].plantPrefab = plantPrefab;
            plantSlots[slotIndex].plantName = plantName;
            plantSlots[slotIndex].plantIcon = plantIcon;
            plantSlots[slotIndex].daysToGrow = daysToGrow;
            plantSlots[slotIndex].description = description;
            plantSlots[slotIndex].data = data;

            Debug.Log($"Unlocked new plant: {plantName} (Seed: {seedType}) in slot {slotIndex + 1}");
        }
        LevelManager.Instance.uiManager.UpdateSeedCountsUI();
    }
}