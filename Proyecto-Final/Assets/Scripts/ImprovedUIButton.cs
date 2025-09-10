using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Advanced UI Button component with comprehensive event handling and audio feedback.
/// Integrates with SoundManager for advanced audio features like randomization and cooldowns.
/// </summary>
[RequireComponent(typeof(Button))] [DisallowMultipleComponent]
public class ImprovedUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Audio Configuration
    [Header("Audio Clips Hover/Click")]
    [SerializeField] private bool _enableHoverSound = true;
    [SerializeField] private SoundClipData _onHoverSound;
    [SerializeField] private bool _enableClickSound = true;
    [SerializeField] private SoundClipData _onClickSound;
    
    [Header("Audio Clips Disabled Hover/Click")]
    [SerializeField] private bool _enableDisabledClickSound = true;
    [SerializeField] private SoundClipData _onDisabledClickSound;
    [SerializeField] private bool _enableDisabledHoverSound = true;
    [SerializeField] private SoundClipData _onDisabledHoverSound;
    #endregion

    #region Events
    [Header("Button Events")]
    [Space(5)]
    public UnityEvent OnHover = new();
    public UnityEvent OnClick = new();
    public UnityEvent OnDisabledClick = new();
    public UnityEvent OnDisabledHover = new();
    private UnityEvent OnHoverExit = new();
    private UnityEvent OnDisabledHoverExit = new();
    #endregion

    #region Advanced Settings
    [Header("Advanced Settings")]
    [SerializeField] private bool _enableDebugLogging;
    #endregion

    #region Private Fields
    private Button buttonComponent;
    private ButtonState currentState;
    private bool isHovering;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the current state of the button
    /// </summary>
    public ButtonState CurrentState => currentState;
    
    /// <summary>
    /// Gets whether the button is currently being hovered
    /// </summary>
    public bool IsHovering => isHovering;
    
    /// <summary>
    /// Gets or sets the button's interactable state
    /// </summary>
    public bool Interactable
    {
        get => buttonComponent != null && buttonComponent.interactable;
        set
        {
            if (buttonComponent != null)
            {
                buttonComponent.interactable = value;
                UpdateButtonState();
            }
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateButtonState();
    }

    private void OnValidate()
    {
        // Validate SoundClipData settings
        ValidateSoundClipData(_onHoverSound);
        ValidateSoundClipData(_onClickSound);
        ValidateSoundClipData(_onDisabledClickSound);
        ValidateSoundClipData(_onDisabledHoverSound);
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        buttonComponent = GetComponent<Button>();
    }

    private void ValidateSoundClipData(SoundClipData soundData)
    {
        if (soundData == null) return;
        
        // Ensure volume and pitch values are within valid ranges
        soundData.volume = Mathf.Clamp01(soundData.volume);
        soundData.pitch = Mathf.Clamp(soundData.pitch, 0.1f, 3f);
        soundData.pitchVariation = Mathf.Clamp01(soundData.pitchVariation);
    }
    #endregion

    #region Event Handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState == ButtonState.Disabled)
        {
            HandleDisabledClick();
        }
        else
        {
            HandleClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        if (currentState == ButtonState.Disabled)
        {
            HandleDisabledHover();
        }
        else
        {
            HandleHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        isHovering = false;
            
        if (currentState == ButtonState.Disabled)
        {
            HandleDisabledHoverExit();
        }
        else
        {
            HandleHoverExit();
        }
    }
    #endregion

    #region Event Handling Methods
    private void HandleClick()
    {
        PlaySound(_onClickSound, _enableClickSound);
        OnClick?.Invoke();
    }

    private void HandleHover()
    {
        PlaySound(_onHoverSound, _enableHoverSound);
        OnHover?.Invoke();
    }

    private void HandleHoverExit()
    {
        OnHoverExit?.Invoke();
    }

    private void HandleDisabledClick()
    {
        PlaySound(_onDisabledClickSound, _enableDisabledClickSound);
        OnDisabledClick?.Invoke();
    }

    private void HandleDisabledHover()
    {
        PlaySound(_onDisabledHoverSound, _enableDisabledHoverSound);
        OnDisabledHover?.Invoke();
    }

    private void HandleDisabledHoverExit()
    {
        OnDisabledHoverExit?.Invoke();
    }
    #endregion

    #region Audio Management
    private void PlaySound(SoundClipData soundData, bool enabledFlag = true)
    {
        if (!enabledFlag || soundData == null) return;

        // Check if SoundManager exists
        if (SoundManager.Instance == null)
        {
            return;
        }

        // Check cooldown before playing
        if (!soundData.CanPlay())
        {
            return;
        }
        
        SoundManager.Instance.PlayClip(soundData);
        
        soundData.SetLastPlayTime();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Manually triggers the hover event (useful for testing or external control)
    /// </summary>
    public void TriggerHover()
    {
        if (currentState == ButtonState.Disabled)
            HandleDisabledHover();
        else
            HandleHover();
    }

    /// <summary>
    /// Manually triggers the click event (useful for testing or external control)
    /// </summary>
    public void TriggerClick()
    {
        if (currentState == ButtonState.Disabled)
            HandleDisabledClick();
        else
            HandleClick();
    }

    /// <summary>
    /// Gets a specific sound clip data for external modification
    /// </summary>
    /// <param name="soundType">The type of sound to get</param>
    /// <returns>The SoundClipData for the specified type</returns>
    public SoundClipData GetSoundData(ButtonSoundType soundType)
    {
        return soundType switch
        {
            ButtonSoundType.Hover => _onHoverSound,
            ButtonSoundType.Click => _onClickSound,
            ButtonSoundType.DisabledClick => _onDisabledClickSound,
            ButtonSoundType.DisabledHover => _onDisabledHoverSound,
            _ => null
        };
    }

    /// <summary>
    /// Sets a specific sound clip data
    /// </summary>
    /// <param name="soundType">The type of sound to set</param>
    /// <param name="soundData">The new SoundClipData</param>
    public void SetSoundData(ButtonSoundType soundType, SoundClipData soundData)
    {
        switch (soundType)
        {
            case ButtonSoundType.Hover:
                _onHoverSound = soundData;
                break;
            case ButtonSoundType.Click:
                _onClickSound = soundData;
                break;
            case ButtonSoundType.DisabledClick:
                _onDisabledClickSound = soundData;
                break;
            case ButtonSoundType.DisabledHover:
                _onDisabledHoverSound = soundData;
                break;
        }
    }
    #endregion

    #region State Management
    private void UpdateButtonState()
    {
        if (buttonComponent == null) return;
        
        var newState = buttonComponent.interactable ? ButtonState.Normal : ButtonState.Disabled;
        
        if (newState != currentState)
        {
            currentState = newState;
        }
    }
    #endregion
}

#region Supporting Enums
/// <summary>
/// Represents the possible states of a button
/// </summary>
public enum ButtonState
{
    Normal,
    Disabled
}

/// <summary>
/// Represents the different types of button sounds
/// </summary>
public enum ButtonSoundType
{
    Hover,
    Click,
    DisabledClick,
    DisabledHover,
    HoverExit,
    DisabledHoverExit
}
#endregion
