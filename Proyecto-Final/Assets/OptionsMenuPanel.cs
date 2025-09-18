using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuPanel : ImprovedUIPanel
{
    [Header("Options Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

    // Start is called before the first frame update
    private void Start()
    {
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(() => {
                Debug.Log("[OptionsMenuPanel] Go Back button clicked, hiding options panel");
                Hide();
            });
        }
    }
}
