using UnityEngine;

public class InstructionsPanel : UIControllerBase
{
    [Header("Instructions Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

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
            _goBackButton.OnClick.AddListener(CloseInstructions);
        }
    }

    private void CloseInstructions()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseInstructions();
        }
        else
        {
            Hide();
        }
    }
}
