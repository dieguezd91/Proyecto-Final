using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensePlant : Plant
{
    [Header("Defense Settings")]
    public float attractionRadius = 10f;
    public float reflectRadius = 2f;
    public float reflectDamage = 15f;
    public float reflectInterval = 0.5f;
    public LayerMask enemyLayer;
    public Color attractionColor = new Color(0.5f, 1f, 0.5f, 0.3f);
    public Color reflectColor = new Color(1f, 0.5f, 0.5f, 0.5f);

    private float reflectTimer = 0f;
    private bool canReflect = false;
    private List<Enemy> enemiesAttracted = new List<Enemy>();

    protected override void Start()
    {
        base.Start();

        LifeController lifeController = GetComponent<LifeController>();
        if (lifeController != null)
        {
            lifeController.maxHealth = 150f;
            lifeController.currentHealth = lifeController.maxHealth;

            lifeController.onDamaged.AddListener(OnDamageTaken);
        }
    }

    private void OnDamageTaken(float damage, LifeController.DamageType damageType)
    {
        if (canReflect && damage > 0 && animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void Update()
    {
        canReflect = IsFullyGrown();

        if (canReflect)
        {
            AttractEnemies();
            reflectTimer += Time.deltaTime;
            if (reflectTimer >= reflectInterval)
            {
                ReflectDamage();
                reflectTimer = 0f;
            }
        }
    }

    void AttractEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attractionRadius, enemyLayer);
        enemiesAttracted.Clear();

        foreach (Collider2D enemigo in enemiesInRange)
        {
            Enemy enemy = enemigo.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (enemy.player != transform)
                {
                    Transform player = enemy.player;
                    enemy.player = transform;
                    enemiesAttracted.Add(enemy);
                    StartCoroutine(RestoreTarget(enemy, player, 10f));
                }
            }
        }
    }

    IEnumerator RestoreTarget(Enemy enemy, Transform originalTarget, float time)
    {
        yield return new WaitForSeconds(time);
        if (enemy != null && enemy.gameObject.activeInHierarchy)
        {
            enemy.player = originalTarget;
        }
    }

    void ReflectDamage()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, reflectRadius, enemyLayer);
        foreach (Collider2D enemy in enemiesInRange)
        {
            LifeController enemyHealth = enemy.GetComponent<LifeController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(reflectDamage);
            }
        }
    }

    protected override void OnMature()
    {
        base.OnMature();

        LifeController lifeController = GetComponent<LifeController>();
        if (lifeController != null)
        {
            lifeController.maxHealth *= 2f;
            lifeController.currentHealth = lifeController.maxHealth;
            lifeController.onHealthChanged?.Invoke(lifeController.currentHealth, lifeController.maxHealth);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        LifeController lifeController = GetComponent<LifeController>();
        if (lifeController != null)
        {
            lifeController.onDamaged.RemoveListener(OnDamageTaken);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = attractionColor;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);

        Gizmos.color = reflectColor;
        Gizmos.DrawWireSphere(transform.position, reflectRadius);
    }
}