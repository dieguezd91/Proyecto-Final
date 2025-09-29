using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class AbilitySlot : ImprovedUIButton, IPointerExitHandler
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
    public event Action<PlayerAbility> OnAbilityHovered;
    public event Action<PlayerAbility> OnAbilityUnhovered;

    public void Initialize()
    {
        // Don't overwrite the abilityType - it's already set in the Inspector
        
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
            
        SetupButtonEvents();
    }

    private void SetupButtonEvents()
    {
        OnClick.RemoveAllListeners();
        OnClick.AddListener(() => OnAbilitySelected?.Invoke(AbilityType));
        
        OnHover.RemoveAllListeners();
        OnHover.AddListener(() => OnAbilityHovered?.Invoke(AbilityType));
    }

    public new void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnAbilityUnhovered?.Invoke(AbilityType);
    }

    public void SetSelected(bool selected)
    {
        if (buttonImage == null) return;
        buttonImage.color = selected ? selectedColor : normalColor;
    }

    public void SetInteractable(bool interactable)
    {
        Interactable = interactable;
        
        // Don't change color if the slot is actually selected - let SetSelected handle that
        // Only apply interactable colors when the slot is not selected
    }

    public void RefreshVisualState(PlayerAbility currentAbility, bool isInteractable)
    {
        bool isSelected = currentAbility == AbilityType;
        
        // Set interactability first
        Interactable = isInteractable;
        
        // Then set the visual state based on selection and interactability
        if (buttonImage == null) return;
        
        if (isSelected)
        {
            buttonImage.color = selectedColor;
        }
        else if (isInteractable)
        {
            buttonImage.color = normalColor;
        }
        else
        {
            buttonImage.color = disabledColor;
        }
    }
}
