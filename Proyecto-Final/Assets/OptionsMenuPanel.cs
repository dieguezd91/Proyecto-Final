using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionsMenuPanel : UIControllerBase
{
    [Header("Options Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

    [Header("Panel Events")]
    [HideInInspector] public UnityEvent OnGoBackClicked = new UnityEvent();

    private void Start()
    {
        // Ensure the controller is properly initialized
        Initialize();
        Setup();
    }
    
    protected override void CacheReferences()
    {
        // Cache any references needed
    }

    protected override void SetupEventListeners()
    {
        Debug.Log("[OptionsMenuPanel] Setting up event listeners");
        
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(() => {
                Debug.Log("[OptionsMenuPanel] Go Back button clicked, notifying controller");
                OnGoBackClicked.Invoke();
            });
            Debug.Log("[OptionsMenuPanel] Go Back button listener added");
        }
        else
        {
            Debug.LogWarning("[OptionsMenuPanel] Go Back button is null!");
        }
    }
}
