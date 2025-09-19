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
        Debug.Log("[InstructionsPanel] Setting up event listeners");
        
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(CloseInstructions);
            Debug.Log("[InstructionsPanel] Go Back button listener added");
        }
        else
        {
            Debug.LogWarning("[InstructionsPanel] Go Back button is null!");
        }
    }

    private void CloseInstructions()
    {
        Debug.Log("[InstructionsPanel] Go Back button clicked, closing instructions");
        
        // Use UIManager to properly close instructions since it handles the game state logic
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseInstructions();
        }
        else
        {
            Debug.LogWarning("[InstructionsPanel] UIManager.Instance is null, falling back to Hide()");
            Hide();
        }
    }
}
