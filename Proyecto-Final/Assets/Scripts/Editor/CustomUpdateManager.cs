using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomUpdateManager : MonoBehaviour
{
    // Singleton instance
    public static CustomUpdateManager Instance { get; private set; }

    // Lista de comportamientos registrados
    public List<ManagedUpdateBehavior> updateBehaviors = new List<ManagedUpdateBehavior>();

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Registro de un nuevo comportamiento
    public void Register(ManagedUpdateBehavior behavior)
    {
        if (!updateBehaviors.Contains(behavior))
        {
            updateBehaviors.Add(behavior);
        }
    }

    // Eliminación del registro de un comportamiento
    public void Unregister(ManagedUpdateBehavior behavior)
    {
        updateBehaviors.Remove(behavior);
    }

    // Llamada al método UpdateMe en todos los comportamientos registrados
    private void Update()
    {
        foreach (var behavior in updateBehaviors)
        {
            behavior.UpdateMe();
        }
    }
}
