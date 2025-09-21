using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImprovedUIPanel : MonoBehaviour
{
    public enum PanelState { Hidden, Shown }

    [SerializeField] private PanelState _defaultState = PanelState.Hidden;
    [SerializeField] private PanelState _currentState = PanelState.Hidden;

    public void Initialize()
    {
        Debug.Log($"[ImprovedUIPanel] Initialize called on {gameObject.name} with DefaultState={_defaultState}");
        if (_defaultState == PanelState.Shown)
            Show();
        else
            Hide();
    }

    public PanelState CurrentState => _currentState;

    public virtual void Show()
    {
        if (_currentState == PanelState.Shown) return;
        Debug.Log($"[ImprovedUIPanel] Show called on {gameObject.name}");
        gameObject.SetActive(true);
        _currentState = PanelState.Shown;
        OnShowAnimation();
    }

    public virtual void Hide()
    {
        if (_currentState == PanelState.Hidden) return;
        Debug.Log($"[ImprovedUIPanel] Hide called on {gameObject.name}");
        OnHideAnimation();
        gameObject.SetActive(false);
        _currentState = PanelState.Hidden;
    }

    protected virtual void OnShowAnimation()
    {
        Debug.Log($"[ImprovedUIPanel] OnShowAnimation called on {gameObject.name}");
        // Override in derived classes for custom show animation
    }

    protected virtual void OnHideAnimation()
    {
        Debug.Log($"[ImprovedUIPanel] OnHideAnimation called on {gameObject.name}");
        // Override in derived classes for custom hide animation
    }
}
