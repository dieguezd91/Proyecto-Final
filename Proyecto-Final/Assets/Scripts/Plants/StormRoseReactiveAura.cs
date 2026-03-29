using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormRoseReactiveAura : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float initialDamage = 2f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float areaDuration = 2.5f;
    [SerializeField] private float areaCheckInterval = 0.1f;

    [Header("Poison (Damage Over Time)")]
    [SerializeField] private float poisonDamagePerTick = 1f;
    [SerializeField] private float poisonDuration = 3f;
    [SerializeField] private float poisonTickRate = 1f;

    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX & Feedback")]
    [SerializeField] private GameObject floatingDamagePrefab;


    private LifeController lifeController;
    private float cooldownTimer = 0f;
    private bool areaActive = false;

    private HashSet<LifeController> damagedTargets =
        new HashSet<LifeController>();

    private void Awake()
    {
        lifeController = GetComponent<LifeController>();
    }

    private void OnEnable()
    {
        if (lifeController != null)
            lifeController.onDamaged.AddListener(OnDamaged);
    }

    private void OnDisable()
    {
        if (lifeController != null)
            lifeController.onDamaged.RemoveListener(OnDamaged);
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnDamaged(float damage, LifeController.DamageType damageType)
    {
        if (cooldownTimer > 0f || areaActive)
            return;

        StartCoroutine(AreaRoutine());
        cooldownTimer = cooldown;
    }

    private IEnumerator AreaRoutine()
    {
        areaActive = true;
        damagedTargets.Clear();

        float elapsed = 0f;

        while (elapsed < areaDuration)
        {
            ApplyAreaEffects();
            elapsed += areaCheckInterval;
            Debug.Log("Area activada");
            yield return new WaitForSeconds(areaCheckInterval);
        }

        areaActive = false;
    }

    private void ApplyAreaEffects()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            LifeController enemyLife = hit.GetComponent<LifeController>();
            if (enemyLife == null || !enemyLife.IsAlive())
                continue;

            if (!damagedTargets.Contains(enemyLife))
            {
                enemyLife.TakeDamage(initialDamage);
                ShowDamageText(enemyLife.transform, initialDamage);

                damagedTargets.Add(enemyLife);
            }

            PoisonEffect poison = enemyLife.GetComponent<PoisonEffect>();

            if (poison == null)
            {
                poison = enemyLife.gameObject.AddComponent<PoisonEffect>();
            }

            poison.ApplyPoison(poisonDuration, poisonTickRate, poisonDamagePerTick);
        }
    }

    private void ShowDamageText(Transform target, float dmg)
    {
        if (floatingDamagePrefab == null) return;

        var existing = target.GetComponentInChildren<FloatingDamageText>();

        if (existing != null)
        {
            existing.AddDamage(dmg);
        }
        else
        {
            Vector3 spawnPos = target.position + Vector3.up * 1f;
            var go = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
            var fdt = go.GetComponent<FloatingDamageText>();
            fdt?.Initialize(target);
            fdt?.AddDamage(dmg);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = areaActive
            ? new Color(0.2f, 1f, 0.4f, 0.8f)
            : new Color(0.4f, 0.8f, 1f, 0.4f);

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
