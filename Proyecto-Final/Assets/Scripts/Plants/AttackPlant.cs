using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlant : Plant
{
    [Header("Configuración de Ataque")]
    public GameObject projectile;
    public Transform firePoint;
    public float cooldown = 2f;
    public float detectionRange = 8f;
    public LayerMask enemyLayer;

    private float attackTimer = 0f;
    private bool canShoot = false;
    private Transform target;

    protected override void Start()
    {
        base.Start();

        if (firePoint == null)
        {
            GameObject point = new GameObject("FirePoint");
            point.transform.parent = transform;
            point.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = point.transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        canShoot = timer >= (growthTime * 0.5f);

        if (canShoot)
        {
            DetectEnemies();

            if (target != null)
            {
                attackTimer += Time.deltaTime;

                if (attackTimer >= cooldown)
                {
                    Shoot();
                    attackTimer = 0f;
                }

                Vector3 dir = target.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                firePoint.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    void DetectEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemiesInRange)
        {
            if (enemy.GetComponent<Enemy>() != null)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        target = closestEnemy;
    }

    void Shoot()
    {
        if (projectile != null && target != null)
        {
            GameObject projectileObj = Instantiate(this.projectile, firePoint.position, firePoint.rotation);
            Spell projectile = projectileObj.GetComponent<Spell>();

            if (projectile != null)
            {
                Vector2 direccion = (target.position - firePoint.position).normalized;
                projectile.SetDirection(direccion);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
