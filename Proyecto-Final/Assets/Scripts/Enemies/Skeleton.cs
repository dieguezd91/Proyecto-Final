using UnityEngine;
using System.Collections.Generic;

public class Skeleton : EnemyBase
{
    [Header("Melee Combat")]
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float minDamage = 8f;
    [SerializeField] private float maxDamage = 12f;
    [SerializeField] private float attackCooldown = 0.75f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private LayerMask attackableLayers;

    private bool isCurrentlyAttacking = false;
    private bool chasingTarget = false;
    private Vector2 moveDirection;

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        UpdateCombatState();
    }

    protected override void ProcessMovement()
    {
        float distanceToTarget = GetDistanceToTarget();

        if (chasingTarget && distanceToTarget > attackDistance)
        {
            MoveTowardsTarget(moveDirection.normalized, moveSpeed);
        }
        else
        {
            StopMovement();
        }
    }

    private void UpdateCombatState()
    {
        if (currentTarget == null)
        {
            chasingTarget = false;
            moveDirection = Vector2.zero;
            return;
        }

        float distanceToTarget = GetDistanceToTarget();

        if (distanceToTarget <= detectionRange)
        {
            chasingTarget = true;
            moveDirection = (currentTarget.position - transform.position).normalized;

            if (distanceToTarget <= attackDistance && !isCurrentlyAttacking)
            {
                StartAttack();
            }
        }
        else
        {
            chasingTarget = false;
            moveDirection = Vector2.zero;
        }
    }

    private void StartAttack()
    {
        animator.SetBool("isAttacking", true);
        isCurrentlyAttacking = true;
    }

    public void PerformSwordHit()
    {
        soundBase?.PlaySound(EnemySoundType.Attack, EnemySoundBase.SoundSourceType.Localized, transform);

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, attackableLayers);
        HashSet<GameObject> damagedTargets = new HashSet<GameObject>();

        foreach (Collider2D target in hitTargets)
        {
            GameObject obj = target.gameObject;
            if (damagedTargets.Contains(obj)) continue;

            damagedTargets.Add(obj);
            DealDamageTo(obj);
        }
    }

    private void DealDamageTo(GameObject target)
    {
        float damage = Random.Range(minDamage, maxDamage);

        LifeController life = target.GetComponent<LifeController>();
        if (life != null)
        {
            life.TakeDamage(damage);

            if (currentTargetType == "player")
            {
                CameraShaker.Instance?.Shake(0.3f, 0.3f);
            }
            return;
        }

        HouseLifeController houseLife = target.GetComponent<HouseLifeController>();
        if (houseLife != null)
        {
            houseLife.TakeDamage(damage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        animator.SetBool("isAttacking", false);
        isCurrentlyAttacking = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}