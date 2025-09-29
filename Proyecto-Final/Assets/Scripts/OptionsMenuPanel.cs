using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionsMenuPanel : UIControllerBase
{
    [Header("Options Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

    [Header("Panel Events")]
    [HideInInspector] public UnityEvent OnGoBackClicked = new ();

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
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(() => { OnGoBackClicked.Invoke(); });
        }
    }
}
