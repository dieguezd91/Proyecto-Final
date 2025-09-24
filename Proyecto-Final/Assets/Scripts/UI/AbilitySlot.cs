using UnityEngine;
using UnityEngine.UI;
using System;

public class AbilitySlot : ImprovedUIButton
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Ability Configuration")]
    [SerializeField] private PlayerAbility abilityType;
    
    public PlayerAbility AbilityType => abilityType;
    
    public event Action<PlayerAbility> OnAbilitySelected;

    public void Initialize(PlayerAbility abilityType)
    {
        this.abilityType = abilityType;
        
        CacheReferences();
        SetupButtonEvents();
    }

    private void CacheReferences()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    private void SetupButtonEvents()
    {
        // Subscribe to ImprovedUIButton events
        OnClick.RemoveAllListeners();
        OnClick.AddListener(() => OnAbilitySelected?.Invoke(AbilityType));
    }

    public void SetSelected(bool selected)
    {
        if (buttonImage == null) return;

        buttonImage.color = selected ? selectedColor : normalColor;
    }

    public void SetInteractable(bool interactable)
    {
        // Use ImprovedUIButton's Interactable property
        Interactable = interactable;
        
        if (buttonImage != null)
        {
            // Only apply disabled color if not selected
            bool isSelected = buttonImage.color == selectedColor;
            if (!interactable && !isSelected)
            {
                buttonImage.color = disabledColor;
            }
            else if (interactable && !isSelected)
            {
                buttonImage.color = normalColor;
            }
        }
    }

    public bool IsSelected()
    {
        return buttonImage != null && buttonImage.color == selectedColor;
    }

    public void RefreshVisualState(PlayerAbility currentAbility, bool isInteractable)
    {
        bool isSelected = currentAbility == AbilityType;
        SetSelected(isSelected);
        SetInteractable(isInteractable);
    }
}
