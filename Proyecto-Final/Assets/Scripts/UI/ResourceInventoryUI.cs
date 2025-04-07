using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private int maxDisplayedResources = 12;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private Dictionary<string, InventorySlotUI> resourceToSlot = new Dictionary<string, InventorySlotUI>();

    private void Start()
    {
        if (slotsContainer == null)
        {
            slotsContainer = transform.Find("SlotsContainer");
        }

        InitializeSlots();

        if (ResourceInventory.Instance != null)
        {
            ResourceInventory.Instance.onResourceChanged += OnResourceChanged;
        }
    }

    private void OnEnable()
    {
        UpdateAllSlots();
        Debug.Log("actualizando slots");
    }

    private void OnDestroy()
    {
        if (ResourceInventory.Instance != null)
        {
            ResourceInventory.Instance.onResourceChanged -= OnResourceChanged;
        }
    }

    private void InitializeSlots()
    {
        if (slotsContainer == null)
        {
            return;
        }

        uiSlots.Clear();
        resourceToSlot.Clear();

        InventorySlotUI[] slots = slotsContainer.GetComponentsInChildren<InventorySlotUI>(true);

        foreach (InventorySlotUI slot in slots)
        {
            uiSlots.Add(slot);
            slot.Clear();
        }
    }

    private void OnResourceChanged(string resourceName, int amount)
    {
        UpdateResourceSlot(resourceName, amount);
    }

    private void UpdateResourceSlot(string resourceName, int amount)
    {
        if (resourceToSlot.TryGetValue(resourceName, out InventorySlotUI slot))
        {
            if (amount <= 0)
            {
                slot.Clear();
                resourceToSlot.Remove(resourceName);
            }
            else
            {
                slot.UpdateAmount(amount);
            }
        }
        else if (amount > 0)
        {
            InventorySlotUI freeSlot = FindFreeSlot();
            if (freeSlot != null)
            {
                Sprite resourceIcon = GetResourceIcon(resourceName);

                freeSlot.Setup(resourceName, amount, resourceIcon);
                resourceToSlot[resourceName] = freeSlot;
            }
        }
    }

    public void UpdateAllSlots()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            slot.Clear();
        }
        resourceToSlot.Clear();

        if (ResourceInventory.Instance != null)
        {
            List<ResourceItem> allResources = ResourceInventory.Instance.GetAllResources();

            int slotIndex = 0;
            foreach (ResourceItem resource in allResources)
            {
                if (slotIndex < uiSlots.Count)
                {
                    InventorySlotUI slot = uiSlots[slotIndex];
                    slot.Setup(resource.name, resource.amount, resource.icon);
                    resourceToSlot[resource.name] = slot;
                    slotIndex++;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private InventorySlotUI FindFreeSlot()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (!slot.IsOccupied())
            {
                return slot;
            }
        }
        return null;
    }

    private Sprite GetResourceIcon(string resourceName)
    {
        if (ResourceInventory.Instance != null)
        {
            List<ResourceItem> allResources = ResourceInventory.Instance.GetAllResources();
            ResourceItem resource = allResources.Find(r => r.name == resourceName);
            if (resource != null && resource.icon != null)
            {
                return resource.icon;
            }
        }

        return null;
    }

    public void ShowInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            UpdateAllSlots();
        }
    }

    public void HideInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void ForceRefresh()
    {
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        yield return null;
        UpdateAllSlots();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(newState);

            if (newState)
            {
                UpdateAllSlots();
            }
        }
    }
}