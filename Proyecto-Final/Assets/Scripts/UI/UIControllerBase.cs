using UnityEngine;

public abstract class UIControllerBase : MonoBehaviour
{
    protected bool isInitialized = false;
    protected bool isSetup = false;

    public virtual void Initialize()
    {
        if (isInitialized) return;

        CacheReferences();
        OnInitialize();

        isInitialized = true;
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