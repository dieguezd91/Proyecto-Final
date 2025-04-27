using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagedUpdateBehavior : MonoBehaviour
{
    protected virtual void Start()
    {
        // Usar la instancia Singleton del CustomUpdateManager
        if (CustomUpdateManager.Instance != null)
        {
            CustomUpdateManager.Instance.Register(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // Desregistrar el comportamiento cuando se destruye
        if (CustomUpdateManager.Instance != null)
        {
            CustomUpdateManager.Instance.Unregister(this);
        }
    }

    public virtual void UpdateMe()
    {
        // Implementación del comportamiento específico en las clases derivadas
    }
}
