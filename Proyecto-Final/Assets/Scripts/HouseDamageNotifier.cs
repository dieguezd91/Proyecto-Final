using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseDamageNotifier : MonoBehaviour
{
    public LifeController houseLife;         // Referencia al LifeController de la casa
    public DangerIndicator dangerIndicator;  
    private float lastHealth;

    void Start()
    {
        if (houseLife != null)
        {
            houseLife.onDamaged.AddListener(OnHouseDamaged);
        }
    }

    void OnDestroy()
    {
        if (houseLife != null)
        {
            houseLife.onDamaged.RemoveListener(OnHouseDamaged);
        }
    }

    void OnHouseDamaged(float damage)
    {
        dangerIndicator?.Activate();

        CancelInvoke(nameof(DeactivateIndicator));
        Invoke(nameof(DeactivateIndicator), 3f);
    }

    void DeactivateIndicator()
    {
        dangerIndicator?.Deactivate();
    }
}
