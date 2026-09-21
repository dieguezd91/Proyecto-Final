using System.Collections;
using UnityEngine;

public class ElectricPlantAura : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float damagePerSecond = 2f;
    [SerializeField] private float areaCheckInterval = 0.1f;

    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX & Feedback")]
    [SerializeField] private GameObject floatingDamagePrefab;

    private LifeController lifeController;

    private void Awake()
    {
        lifeController = GetComponent<LifeController>();
    }

    private void OnEnable()
    {
        StartCoroutine(AreaRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AreaRoutine()
    {
        while (true)
        {
            ApplyAreaEffects();

            yield return new WaitForSeconds(areaCheckInterval);
        }
    }

    private void ApplyAreaEffects()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyLayer
        );

        float damage = damagePerSecond * areaCheckInterval;

        foreach (var hit in hits)
        {
            LifeController enemyLife = hit.GetComponent<LifeController>();

            if (enemyLife == null || !enemyLife.IsAlive())
                continue;

            enemyLife.TakeDamage(damage);

            ShowDamageText(enemyLife.transform, damage);
        }
    }

    private void ShowDamageText(Transform target, float dmg)
    {
        if (floatingDamagePrefab == null)
            return;

        var existing = target.GetComponentInChildren<FloatingDamageText>();

        if (existing != null)
        {
            existing.AddDamage(dmg);
        }
        else
        {
            Vector3 spawnPos = target.position + Vector3.up * 1f;

            var go = Instantiate(
                floatingDamagePrefab,
                spawnPos,
                Quaternion.identity
            );

            var fdt = go.GetComponent<FloatingDamageText>();

            fdt?.Initialize(target);
            fdt?.AddDamage(dmg);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}