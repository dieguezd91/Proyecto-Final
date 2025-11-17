using System.Collections.Generic;
using UnityEngine;

public class Skeleton : EnemyBase
{
    [Header("Melee Data")]
    [SerializeField] private MeleeEnemyDataSO meleeData;

    [Header("Combat References")]
    public Transform attackPoint;
    public LayerMask attackableLayers;

    private float attackDistance;
    public float minDamage;
    public float maxDamage;
    public float attackRange;

    

    protected override EnemyDataSO GetEnemyData() => meleeData;

    protected override void Awake()
    {
        base.Awake();
        LoadEnemyData();
    }

    protected override void LoadEnemyData()
    {
        base.LoadEnemyData();

        if (meleeData != null)
        {
            attackDistance = meleeData.AttackDistance;
            minDamage = meleeData.MinDamage;
            maxDamage = meleeData.MaxDamage;
            attackCooldown = meleeData.AttackCooldown;
            attackRange = meleeData.AttackRange;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No MeleeEnemyDataSO assigned!");
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isDead)
        {
           return;
        }

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        StateMachine?.CurrentState?.OnFixedUpdate();
    }

    

    protected override void ProcessMovement()
    {
    }


    public bool CanAttack()
    {
        return !isCurrentlyAttacking && Time.time >= nextAttackTime;
    }

    public void StartAttack()
    {
        if (isCurrentlyAttacking) return;

        isCurrentlyAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        animator?.SetBool("isAttacking", true);
    }

    public void PerformSwordHit()
    {
        // Play attack sound at the start of the swing, even if no target is hit
        PlayEnemySound(EnemySoundType.Attack, SoundSourceType.Localized, transform);
        Debug.Log($"[{gameObject.name}] Skeleton: Playing swing attack sound at {Time.time:F2}s.");

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

        if (target.TryGetComponent(out LifeController life))
        {
            life.TakeDamage(damage);

            if (currentTargetType == "player")
            {
                CameraShaker.Instance?.Shake(0.3f, 0.3f);
            }
        }
        else if (target.TryGetComponent(out HouseLifeController houseLife))
        {
            houseLife.TakeDamage(damage);
        }
    }

    public void OnAttackAnimationEnd()
    {
        isCurrentlyAttacking = false;
        animator?.SetBool("isAttacking", false);

        if (isDead)
        {
            StateMachine.ChangeState<EnemyDeadState>();
        }
        else if (currentTarget != null && GetDistanceToTarget() <= detectionRange)
        {
            StateMachine.ChangeState<EnemyChaseState>();
        }
        else
        {
            StateMachine.ChangeState<EnemyIdleState>();
        }
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
