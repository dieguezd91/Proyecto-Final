using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI materialNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 1f);

    private string resourceName;
    private int resourceAmount;
    private bool isOccupied;

    private void Start()
    {
        if (iconImage == null || amountText == null)
        {
            Debug.LogError($"faltan referencias en {gameObject.name}");
        }

        Clear();
    }

    public void Setup(MaterialType type, int amount, Sprite icon)
    {
        resourceName = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetMaterialName(type)
            : type.ToString();

        resourceAmount = amount;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (amountText != null)
        {
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }

        if (materialNameText != null)
        {
            materialNameText.text = resourceName;
            materialNameText.enabled = true;
        }

        isOccupied = true;
        UpdateVisualState();
    }

    public void UpdateAmount(int newAmount)
    {
        resourceAmount = newAmount;

        if (amountText != null)
        {
            amountText.text = newAmount.ToString();
        }
    }

    public void Clear()
    {
        resourceName = string.Empty;
        resourceAmount = 0;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = string.Empty;
            amountText.enabled = false;
        }

        if (materialNameText != null)
        {
            materialNameText.text = string.Empty;
            materialNameText.enabled = false;
        }

        isOccupied = false;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isOccupied ? occupiedColor : emptyColor;
        }
    }

    public bool IsOccupied() => isOccupied;
    public string GetResourceName() => resourceName;
    public int GetResourceAmount() => resourceAmount;

    public void OnSlotClicked()
    {
        if (isOccupied)
        {
            Debug.Log($"Slot clicked: {resourceName} x{resourceAmount}");
        }
    }
}