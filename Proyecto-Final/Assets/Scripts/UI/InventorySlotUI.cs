using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public event Action<MaterialType> onLeftClick;

    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI materialNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 1f);

    private MaterialType typeHeld;
    private string resourceName;
    private int resourceAmount;
    private bool isOccupied;

    private void Start()
    {
        Clear();
    }

    /// <summary>
    /// Inicializa el slot con un material.
    /// </summary>
    public void Setup(MaterialType type, int amount, Sprite icon)
    {
        typeHeld = type;
        resourceName = InventoryManager.Instance.GetMaterialName(type);
        resourceAmount = amount;

        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
        amountText.text = amount.ToString();
        amountText.enabled = true;
        materialNameText.text = resourceName;
        materialNameText.enabled = true;

        isOccupied = true;
        UpdateVisualState();
    }

    /// <summary>
    /// Actualiza solo la cantidad mostrada.
    /// </summary>
    public void UpdateAmount(int newAmount)
    {
        resourceAmount = newAmount;
        amountText.text = newAmount.ToString();
    }

    /// <summary>
    /// Limpia el slot dejándolo vacío.
    /// </summary>
    public void Clear()
    {
        typeHeld = MaterialType.None;
        resourceName = "";
        resourceAmount = 0;

        iconImage.enabled = false;
        amountText.enabled = false;
        materialNameText.enabled = false;

        isOccupied = false;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        backgroundImage.color = isOccupied ? occupiedColor : emptyColor;
    }

    public bool IsOccupied() => isOccupied;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOccupied) return;

        if (eventData.button == PointerEventData.InputButton.Right
         && typeHeld == MaterialType.HouseHealingPotion)
        {
            // lógica para usar poción...
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClick?.Invoke(typeHeld);
        }
    }
}
