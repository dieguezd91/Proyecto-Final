using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private int maxDisplayedResources = 12;

    [Header("DESCRIPTION PANEL")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Image descriptionIcon;
    [SerializeField] private TextMeshProUGUI descriptionName;
    [SerializeField] private TextMeshProUGUI descriptionDetails;

    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Tabs")]
    [SerializeField] private List<GameObject> tabPanels = new List<GameObject>();
    [SerializeField] private int optionsTabIndex = 0;
    [SerializeField] private GameObject optionsPanelFallback;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private Dictionary<MaterialType, InventorySlotUI> resourceToSlot = new Dictionary<MaterialType, InventorySlotUI>();

    public bool IsInventoryOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    private void Start()
    {
        if (slotsContainer == null)
            slotsContainer = transform.Find("SlotsContainer");

        InitializeSlots();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onMaterialChanged += OnMaterialChanged;
            InventoryManager.Instance.onGoldChanged += UpdateGoldDisplay;
        }

        UpdateGoldDisplay(InventoryManager.Instance != null ? InventoryManager.Instance.GetGold() : 0);
        ClearDescriptionPanel();
    }

    private void OnEnable()
    {
        UpdateAllSlots();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onMaterialChanged -= OnMaterialChanged;
            InventoryManager.Instance.onGoldChanged -= UpdateGoldDisplay;
        }
    }

    private void InitializeSlots()
    {
        if (slotsContainer == null) return;

        uiSlots.Clear();
        resourceToSlot.Clear();

        InventorySlotUI[] slots = slotsContainer.GetComponentsInChildren<InventorySlotUI>(true);
        foreach (InventorySlotUI slot in slots)
        {
            uiSlots.Add(slot);
            slot.onLeftClick += HandleSlotClicked;
            slot.Clear();
        }
    }

    private void HandleSlotClicked(MaterialType type)
    {
        var data = InventoryManager.Instance.GetMaterialData(type);
        if (data == null) return;

        descriptionIcon.sprite = data.materialIcon;
        var color = descriptionIcon.color;
        color.a = 1f;
        descriptionIcon.color = color;

        descriptionName.text = data.materialName;
        descriptionDetails.text = data.materialDescription;
        descriptionPanel.SetActive(true);
    }

    private void OnMaterialChanged(MaterialType materialType, int amount)
    {
        UpdateResourceSlot(materialType, amount);
    }

    private void UpdateResourceSlot(MaterialType materialType, int amount)
    {
        if (resourceToSlot.TryGetValue(materialType, out InventorySlotUI slot))
        {
            if (amount <= 0)
            {
                slot.Clear();
                resourceToSlot.Remove(materialType);
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
                Sprite resourceIcon = GetResourceIcon(materialType);
                freeSlot.Setup(materialType, amount, resourceIcon);
                resourceToSlot[materialType] = freeSlot;
            }
        }
    }

    public void UpdateAllSlots()
    {
        foreach (InventorySlotUI slot in uiSlots) slot.Clear();
        resourceToSlot.Clear();

        if (InventoryManager.Instance != null)
        {
            List<MaterialItem> allMaterials = InventoryManager.Instance.GetAllMaterials();
            int slotIndex = 0;
            foreach (MaterialItem material in allMaterials)
            {
                if (slotIndex < uiSlots.Count)
                {
                    InventorySlotUI slot = uiSlots[slotIndex];
                    slot.Setup(material.type, material.amount, material.icon);
                    resourceToSlot[material.type] = slot;
                    slotIndex++;
                }
                else break;
            }
        }
    }

    private InventorySlotUI FindFreeSlot()
    {
        foreach (InventorySlotUI slot in uiSlots)
            if (!slot.IsOccupied()) return slot;
        return null;
    }

    private Sprite GetResourceIcon(MaterialType materialType)
    {
        if (InventoryManager.Instance != null)
        {
            List<MaterialItem> allResources = InventoryManager.Instance.GetAllMaterials();
            MaterialItem resource = allResources.Find(r => r.type == materialType);
            if (resource != null && resource.icon != null) return resource.icon;
        }
        return null;
    }

    public void ShowInventory()
    {
        if (inventoryPanel == null) return;

        inventoryPanel.SetActive(true);
        UpdateAllSlots();
        ClearDescriptionPanel();
        descriptionPanel.SetActive(false);
        
    }

    public void HideInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
        ClearDescriptionPanel();
    }

    public void ForceRefresh() => StartCoroutine(DelayedRefresh());
    private IEnumerator DelayedRefresh() { yield return null; UpdateAllSlots(); }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);

        if (newState)
        {
            UpdateAllSlots();
            ClearDescriptionPanel();
        }
    }

    private void UpdateGoldDisplay(int newAmount)
    {
        if (goldText != null) goldText.text = $"GOLD: " + newAmount.ToString();
    }

    public void ClearDescriptionPanel()
    {
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
        if (descriptionIcon != null) { descriptionIcon.sprite = null; var color = descriptionIcon.color; color.a = 0f; descriptionIcon.color = color; }
        if (descriptionName != null) descriptionName.text = "";
        if (descriptionDetails != null) descriptionDetails.text = "";
    }

    
    public void OpenOptionsTab()
    {
        ShowInventory();
        StartCoroutine(SelectTabNextFrame(optionsTabIndex));
    }

    private IEnumerator SelectTabNextFrame(int index)
    {
        yield return null;
        SelectTab(index);
    }

    
    public void SelectTab(int index)
    {
        if (tabPanels == null || tabPanels.Count == 0)
        {
            if (index == optionsTabIndex && optionsPanelFallback != null)
            {
                descriptionPanel.SetActive(true);

            }
            return;
        }

        for (int i = 0; i < tabPanels.Count; i++)
        {
            var panel = tabPanels[i];
            if (panel != null) panel.SetActive(i == index);
        }

        if (descriptionPanel != null) descriptionPanel.SetActive(true);
    }
}
