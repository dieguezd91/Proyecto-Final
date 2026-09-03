using UnityEngine;
using UnityEngine.Events;

public class InstructionsPanel : UIControllerBase
{
    [Header("Instructions Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

    [Header("Panel Events")]
    [HideInInspector] public UnityEvent OnGoBackClicked = new();

    private void Start()
    {
        // Ensure the controller is properly initialized
        Initialize();
        Setup();
    }
    
    protected override void CacheReferences()
    {
        if (_goBackButton == null)
        {
            _goBackButton = GetComponentInChildren<ImprovedUIButton>(true);
        }
    }

    protected override void SetupEventListeners()
    {
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(Hide);
            _goBackButton.OnClick.AddListener(() => { OnGoBackClicked.Invoke(); });
        }
    }
}
