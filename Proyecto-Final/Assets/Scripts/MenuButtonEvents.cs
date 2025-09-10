using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButtonEvents : BaseButtonEvents
{
    [Header("Menu Button Specific Configuration")]
    [SerializeField] private MenuButtonType _menuButtonType;
    
    [Header("Sound Configuration")]
    [SerializeField] private InterfaceSoundBase interfaceSoundBase;
    
    [Header("Menu Button Events")]
    public UnityEvent OnPlayButtonClicked;
    public UnityEvent OnOptionsButtonClicked;
    public UnityEvent OnControlsButtonClicked;
    public UnityEvent OnExitButtonClicked;
    
    public UnityEvent OnPlayButtonHover;
    public UnityEvent OnOptionsButtonHover;
    public UnityEvent OnControlsButtonHover;
    public UnityEvent OnExitButtonHover;
    
    [Header("Disabled Button Events")]
    public UnityEvent OnPlayButtonDisabledHover;
    public UnityEvent OnOptionsButtonDisabledHover;
    public UnityEvent OnControlsButtonDisabledHover;
    public UnityEvent OnExitButtonDisabledHover;

    private Button buttonComponent;

    protected override void Start()
    {
        base.Start();
        buttonComponent = GetComponent<Button>();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // Check if button is disabled
        if (buttonComponent != null && !buttonComponent.interactable)
        {
            return;
        }

        // Play click sound for enabled buttons
        PlayButtonSound("Click");
        
        // Handle specific menu button click events
        switch (_menuButtonType)
        {
            case MenuButtonType.Play:
                OnPlayButtonClicked?.Invoke();
                break;
            case MenuButtonType.Options:
                OnOptionsButtonClicked?.Invoke();
                break;
            case MenuButtonType.Controls:
                OnControlsButtonClicked?.Invoke();
                break;
            case MenuButtonType.Exit:
                OnExitButtonClicked?.Invoke();
                break;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        // Play hover sound (will determine enabled/disabled sound internally)
        PlayButtonSound("Enter");

        // Check if button is disabled and trigger appropriate events
        if (buttonComponent != null && !buttonComponent.interactable)
        {
            // Handle disabled hover events
            switch (_menuButtonType)
            {
                case MenuButtonType.Play:
                    OnPlayButtonDisabledHover?.Invoke();
                    break;
                case MenuButtonType.Options:
                    OnOptionsButtonDisabledHover?.Invoke();
                    break;
                case MenuButtonType.Controls:
                    OnControlsButtonDisabledHover?.Invoke();
                    break;
                case MenuButtonType.Exit:
                    OnExitButtonDisabledHover?.Invoke();
                    break;
            }
        }
        else
        {
            // Handle enabled hover events
            switch (_menuButtonType)
            {
                case MenuButtonType.Play:
                    OnPlayButtonHover?.Invoke();
                    break;
                case MenuButtonType.Options:
                    OnOptionsButtonHover?.Invoke();
                    break;
                case MenuButtonType.Controls:
                    OnControlsButtonHover?.Invoke();
                    break;
                case MenuButtonType.Exit:
                    OnExitButtonHover?.Invoke();
                    break;
            }
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        
    }

    protected override void PlayButtonSound(string actionType)
    {
        // Get the interface sound base reference
        if (interfaceSoundBase == null)
        {
            interfaceSoundBase = FindObjectOfType<InterfaceSoundBase>();
            if (interfaceSoundBase == null)
            {
                return;
            }
        }

        // Check if button is disabled to determine sound type
        bool isDisabled = buttonComponent != null && !buttonComponent.interactable;
        
        // Get the appropriate sound based on button state
        var soundToPlay = GetMenuSoundType(actionType, isDisabled);

        // Play the sound through InterfaceSoundBase
        interfaceSoundBase.PlaySound(soundToPlay);
    }

    private InterfaceSoundType GetMenuSoundType(string actionType, bool isDisabled)
    {
        // If button is disabled, use disabled sound variants
        if (isDisabled)
        {
            return actionType.ToLower() switch
            {
                "click" => InterfaceSoundType.GenericButtonClick, // Use generic disabled click
                "hover" or "enter" => InterfaceSoundType.GenericButtonHover, // Use generic disabled hover
                _ => InterfaceSoundType.GenericButtonClick
            };
        }

        // If button is enabled, use specific menu sounds
        switch (_menuButtonType)
        {
            case MenuButtonType.Play:
                return actionType.ToLower() switch
                {
                    "click" => InterfaceSoundType.MenuButtonPlay,
                    "hover" or "enter" => InterfaceSoundType.MenuButtonHover,
                    _ => InterfaceSoundType.MenuButtonClick
                };
            case MenuButtonType.Options:
                return actionType.ToLower() switch
                {
                    "click" => InterfaceSoundType.MenuButtonOptions,
                    "hover" or "enter" => InterfaceSoundType.MenuButtonHover,
                    _ => InterfaceSoundType.MenuButtonClick
                };
            case MenuButtonType.Controls:
                return actionType.ToLower() switch
                {
                    "click" => InterfaceSoundType.MenuButtonControls,
                    "hover" or "enter" => InterfaceSoundType.MenuButtonHover,
                    _ => InterfaceSoundType.MenuButtonClick
                };
            case MenuButtonType.Exit:
                return actionType.ToLower() switch
                {
                    "click" => InterfaceSoundType.MenuButtonExit,
                    "hover" or "enter" => InterfaceSoundType.MenuButtonHover,
                    _ => InterfaceSoundType.MenuButtonClick
                };
            default:
                return InterfaceSoundType.MenuButtonClick;
        }
    }
}

public enum MenuButtonType
{
    Play,
    Options,
    Controls,
    Exit
}
