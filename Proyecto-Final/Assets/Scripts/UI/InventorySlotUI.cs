using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
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
        if (iconImage == null || amountText == null || materialNameText == null || backgroundImage == null)
        {
            Debug.LogError($"Faltan referencias UI en {gameObject.name}");
        }
        Clear();
    }

    /// <summary>
    /// Inicializa el slot con un material (incluida la poción de curar la casa).
    /// </summary>
    public void Setup(MaterialType type, int amount, Sprite icon)
    {
        typeHeld = type;
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

    /// <summary>
    /// Actualiza solo la cantidad mostrada.
    /// </summary>
    public void UpdateAmount(int newAmount)
    {
        resourceAmount = newAmount;
        if (amountText != null)
        {
            amountText.text = newAmount.ToString();
        }
    }

    /// <summary>
    /// Limpia el slot dejándolo vacío.
    /// </summary>
    public void Clear()
    {
        typeHeld = MaterialType.None;
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

    /// <summary>
    /// Detecta clicks en el slot. Click derecho usa poción, click izquierdo loggea.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOccupied) return;

        // Click derecho: si es poción de curar la casa, úsala
        if (eventData.button == PointerEventData.InputButton.Right
         && typeHeld == MaterialType.HouseHealingPotion)
        {
            bool used = InventoryManager.Instance.UseMaterial(MaterialType.HouseHealingPotion, 1);
            if (used)
            {
                // Cura la casa: invierte daño
                var homeLife = GameManager.Instance.home.GetComponent<LifeController>();
                if (homeLife != null)
                {
                    homeLife.TakeDamage(-50f); // ajusta el valor de curación aquí
                    GameManager.Instance.uiManager.UpdateHomeHealthBar(
                        homeLife.currentHealth, homeLife.maxHealth);
                }
                UpdateAmount(InventoryManager.Instance.GetMaterialAmount(MaterialType.HouseHealingPotion));
                if (resourceAmount <= 0)
                {
                    Clear();
                }
            }
        }
        // Click izquierdo: comportamiento genérico
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"Slot clicked: {resourceName} x{resourceAmount}");
        }
    }
}
