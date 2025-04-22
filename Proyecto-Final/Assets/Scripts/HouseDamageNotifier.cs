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
            lastHealth = houseLife.currentHealth;
            houseLife.onHealthChanged.AddListener(OnHouseHealthChanged);
        }
    }

    void OnHouseHealthChanged(float currentHealth, float maxHealth)
    {
        if (currentHealth < lastHealth)
        {
            // La casa recibió daño
            dangerIndicator?.Activate();

            // Opcional: desactivar después de unos segundos
            CancelInvoke(nameof(DeactivateIndicator));
            Invoke(nameof(DeactivateIndicator), 3f);
        }

        lastHealth = currentHealth;
    }

    void DeactivateIndicator()
    {
        dangerIndicator?.Deactivate();
    }
}
