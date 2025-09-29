using UnityEngine;

public abstract class UIControllerBase : MonoBehaviour
{
    public enum PanelState { Hidden, Shown }

    [SerializeField] private PanelState _defaultState = PanelState.Hidden;
    [SerializeField] protected PanelState _currentState = PanelState.Hidden;

    protected bool isInitialized = false;
    protected bool isSetup = false;

    public PanelState CurrentState => _currentState;

    public virtual void Initialize()
    {
        if (isInitialized) return;

        CacheReferences();
        OnInitialize();

        // Initialize panel state
        InitializePanelState();

        isInitialized = true;
    }

    protected virtual void InitializePanelState()
    {
        // Only apply default state if the panel hasn't been manually shown/hidden already
        if (_currentState == PanelState.Hidden && _defaultState == PanelState.Shown)
            Show();
        else if (_currentState == PanelState.Shown && _defaultState == PanelState.Hidden)
        {
            // Don't hide if the panel was just shown programmatically
        }
        else if (_currentState == PanelState.Hidden && _defaultState == PanelState.Hidden)
            Hide();
    }

    public virtual void Setup()
    {
        if (!isInitialized)
        {
            return;
        }

        if (isSetup) return;

        SetupEventListeners();
        ConfigureInitialState();
        OnSetup();

        isSetup = true;
    }

    public virtual void HandleUpdate() { }

    public virtual void Show()
    {
        if (_currentState == PanelState.Shown) return;
        gameObject.SetActive(true);
        _currentState = PanelState.Shown;
        OnShowAnimation();
    }

    public virtual void Hide()
    {
        if (_currentState == PanelState.Hidden) return;
        OnHideAnimation();
        gameObject.SetActive(false);
        _currentState = PanelState.Hidden;
    }

    protected virtual void OnShowAnimation()
    {
        // Override in derived classes for custom show animation
    }

    protected virtual void OnHideAnimation()
    {
        // Override in derived classes for custom hide animation
    }

    protected abstract void CacheReferences();
    protected virtual void OnInitialize() { }
    protected virtual void SetupEventListeners() { }
    protected virtual void ConfigureInitialState() { }
    protected virtual void OnSetup() { }

    protected virtual void OnDestroy()
    {
        CleanupEventListeners();
        OnCleanup();
    }

    protected virtual void CleanupEventListeners() { }
    protected virtual void OnCleanup() { }
}