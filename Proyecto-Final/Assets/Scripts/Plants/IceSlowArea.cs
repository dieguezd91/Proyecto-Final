using System.Collections.Generic;
using UnityEngine;

public class IceSlowArea : MonoBehaviour
{
    [Header("ICE AREA")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float radius = 2f;

    [Header("SLOW")]
    [SerializeField, Range(0f, 1f)] private float slowAmount = 0.3f;

    [SerializeField] private LayerMask enemyLayer;

    private float remainingDuration;

    private HashSet<EnemyBase> affectedEnemies = new HashSet<EnemyBase>();

    private void OnEnable()
    {
        remainingDuration = duration;
        affectedEnemies.Clear();
    }

    private void Update()
    {
        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        ApplySlow();
    }

    private void ApplySlow()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            if (enemy == null)
                continue;

            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

            if (enemyBase == null || enemyBase.IsDead)
                continue;

            enemyBase.ApplySlow(slowAmount);
            affectedEnemies.Add(enemyBase);
        }
    }

    private void OnDisable()
    {
        foreach (EnemyBase enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                enemy.RemoveSlow();
            }
        }

        affectedEnemies.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}