using UnityEngine;

public class AttackPlant : Plant
{
    [Header("Attack Configuration")]
    public GameObject projectile;
    public Transform firePoint;
    public float cooldown = 2f;
    public float detectionRange = 8f;
    public LayerMask enemyLayer;

    private float attackTimer = 0f;
    private bool canShoot = false;
    private Transform target;

    private bool isPerformingAttack = false;
    private Transform queuedTarget;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalFirePointLocalPos;
    private bool isFlipped = false;
    private bool spriteFacesLeft = false;

    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (plantData != null)
        {
            spriteFacesLeft = plantData.spriteFacesLeft;
        }

        if (firePoint == null)
        {
            GameObject point = new GameObject("FirePoint");
            point.transform.parent = transform;
            point.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = point.transform;
        }

        originalFirePointLocalPos = firePoint.localPosition;

        LifeController lifeController = GetComponent<LifeController>();
        if (lifeController != null)
        {
            lifeController.maxHealth = 100f;
            lifeController.currentHealth = lifeController.maxHealth;
        }
    }

    protected override void Update()
    {
        base.Update();
        canShoot = IsFullyGrown();

        if (canShoot && !isPerformingAttack)
        {
            DetectEnemies();

            if (target != null)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= cooldown)
                {
                    StartAttackSequence();
                    attackTimer = 0f;
                }

                Vector3 direction = target.position - transform.position;

                UpdateSpriteFlipAndFirePoint(direction);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                firePoint.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void UpdateSpriteFlipAndFirePoint(Vector3 direction)
    {
        if (spriteRenderer == null) return;

        bool shouldFlip;
        if (spriteFacesLeft)
        {
            shouldFlip = direction.x > 0;
        }
        else
        {
            shouldFlip = direction.x < 0;
        }

        if (shouldFlip != isFlipped)
        {
            isFlipped = shouldFlip;
            spriteRenderer.flipX = isFlipped;

            Vector3 adjustedPos = originalFirePointLocalPos;
            adjustedPos.x *= isFlipped ? -1 : 1;
            firePoint.localPosition = adjustedPos;
        }
    }

    void DetectEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemiesInRange)
        {
            if (enemy.GetComponent<GardenGnome>() != null)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        target = closestEnemy;
    }

    void StartAttackSequence()
    {
        if (target != null && animator != null)
        {
            isPerformingAttack = true;
            queuedTarget = target;
            animator.SetTrigger("Attack");
        }
    }

    public void OnShootAnimationEvent()
    {
        if (projectile != null && queuedTarget != null)
        {
            if (queuedTarget.gameObject.activeInHierarchy)
            {
                GameObject projectileObj = Instantiate(this.projectile, firePoint.position, firePoint.rotation);
                BasicRangeSpell projectileComponent = projectileObj.GetComponent<BasicRangeSpell>();

                if (projectileComponent != null)
                {
                    Vector2 direction = (queuedTarget.position - firePoint.position).normalized;
                    projectileComponent.SetDirection(direction);
                }
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        isPerformingAttack = false;
        queuedTarget = null;
    }

    protected override void OnMature()
    {
        base.OnMature();

        cooldown *= 0.7f;
        detectionRange *= 1.2f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}