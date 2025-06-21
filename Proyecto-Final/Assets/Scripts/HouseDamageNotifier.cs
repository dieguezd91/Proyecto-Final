using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseDamageNotifier : MonoBehaviour
{
    [Header("Referencias")]
    public LifeController houseLife;
    public DangerIndicator dangerIndicator;
    public Transform player;

    [Header("Ajustes")]
    [Tooltip("Distancia mínima a la casa para ignorar la notificación")]
    public float ignoreRadius = 5f;

    void Start()
    {
        if (houseLife != null)
            houseLife.onDamaged.AddListener(OnHouseDamaged);
    }

    void OnDestroy()
    {
        if (houseLife != null)
            houseLife.onDamaged.RemoveListener(OnHouseDamaged);
    }

    private void OnHouseDamaged(float damage)
    {
        if (player == null || dangerIndicator == null)
            return;

        float dist = Vector3.Distance(player.position, houseLife.transform.position);
        if (dist <= ignoreRadius)
            return;

        dangerIndicator.Activate();

        CancelInvoke(nameof(DeactivateIndicator));
        Invoke(nameof(DeactivateIndicator), 2f);
    }

    private void DeactivateIndicator()
    {
        dangerIndicator.Deactivate();
    }

    private void OnDrawGizmosSelected()
    {
        if (houseLife == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(houseLife.transform.position, ignoreRadius);
    }
}
