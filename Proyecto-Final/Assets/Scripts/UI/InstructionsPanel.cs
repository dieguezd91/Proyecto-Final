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
}
