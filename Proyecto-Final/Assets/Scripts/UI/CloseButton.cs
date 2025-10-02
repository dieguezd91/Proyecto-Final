using UnityEngine;

public class CloseButton : ImprovedUIButton
{
    [Header("Close Button Settings")]
    [SerializeField] private bool closeOnEscape = true;

    private void Update()
    {
        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerClick();
        }
    }
}