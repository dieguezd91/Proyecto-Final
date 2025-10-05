using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : IState
{
    private readonly EnemyBase enemy;

    public EnemyIdleState(EnemyBase enemy) => this.enemy = enemy;

    public void OnEnter()
    {
        enemy.StopMovement();
        Debug.Log($"{enemy.name} entra en Idle");
    }

    public void OnUpdate()
    {
        if (enemy.GetCurrentTarget() != null)
            enemy.StateMachine.ChangeState<EnemyChaseState>();
    }

    public void OnFixedUpdate() { }

    public void OnExit() { }
}

public class EnemyChaseState : IState
{
    private readonly EnemyBase enemy;
    private float attackRange = 1.5f;

    public EnemyChaseState(EnemyBase enemy)
    {
        this.enemy = enemy;
        DetectAttackRange();
    }

    private void DetectAttackRange()
    {
        if (enemy is Skeleton skeleton)
            attackRange = skeleton.attackRange;
        else if (enemy is Infernum infernum)
            attackRange = infernum.GetType().GetField("shootingRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(infernum) as float? ?? 5f;
        else if (enemy is GardenGnome gnome)
            attackRange = gnome.GetType().GetField("stopDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(gnome) as float? ?? 0.3f;
    }

    public void OnEnter()
    {
        Debug.Log($"{enemy.name} empieza a perseguir (rango de ataque = {attackRange})");
    }

    public void OnUpdate()
    {
        var target = enemy.GetCurrentTarget();
        if (target == null)
        {
            enemy.StateMachine.ChangeState<EnemyIdleState>();
            return;
        }

        if (enemy.GetDistanceToTarget() <= attackRange)
        {
            enemy.StateMachine.ChangeState<EnemyAttackState>();
        }
    }

    public void OnFixedUpdate()
    {
        var target = enemy.GetCurrentTarget();
        if (target == null) return;

        Vector2 dir = (target.position - enemy.transform.position).normalized;
        enemy.MoveTowardsTarget(dir, enemy.MoveSpeed);
    }

    public void OnExit() => enemy.StopMovement();
}


public class EnemyAttackState : IState
{
    private EnemyBase enemy;
    private Skeleton skeleton;
    private Infernum infernum;
    private GardenGnome gnome;

    private float attackRange = 1.5f;

    public EnemyAttackState(EnemyBase enemy)
    {
        this.enemy = enemy;
        DetectAttackRange();
    }

    private void DetectAttackRange()
    {
        if (enemy is Skeleton skeleton)
            attackRange = skeleton.attackRange;
        else if (enemy is Infernum infernum)
            attackRange = infernum.GetType().GetField("shootingRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(infernum) as float? ?? 5f;
        else if (enemy is GardenGnome gnome)
            attackRange = gnome.GetType().GetField("stopDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(gnome) as float? ?? 0.3f;
    }

    public void OnEnter()
    {
        enemy.StopMovement();
        enemy.isCurrentlyAttacking = true;

        enemy.nextAttackTime = Time.time + enemy.attackCooldown;
        
        if (enemy.animator != null)
        {
            enemy.animator.SetBool("isAttacking", true);
        }

        var method = enemy.GetType().GetMethod("PerformAttack",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (method != null)
        {
            method.Invoke(enemy, null);
        }
        else
        {
            Debug.LogWarning($"{enemy.name} no tiene método PerformAttack().");
        }
    }

    public void OnUpdate()
    {
        if (enemy.GetDistanceToTarget() > attackRange + 0.5f)
        {
            enemy.StateMachine.ChangeState<EnemyChaseState>();
        }


        if (enemy is Infernum infernum)
        {
            if (infernum.CanShootNow)
            {
                infernum.PerformAttack();
            }
        }


    }

    public void OnFixedUpdate() { }

    public void OnExit() { }
}




public class EnemyDeadState : IState
{
    private readonly EnemyBase enemy;
    private Coroutine destroyCoroutine;
    private const float DefaultDestroyDelay =0.1f;
    private readonly LifeController lifeController;

    public EnemyDeadState(EnemyBase enemy) => this.enemy = enemy;

    public void OnEnter()
    {
        enemy.StopMovement();

        Animator anim = enemy.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        var cols = enemy.GetComponents<Collider2D>();
        foreach (var c in cols) c.enabled = false;

        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        destroyCoroutine = enemy.StartCoroutine(DestroyAfterDelay(DefaultDestroyDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        
        if (enemy.animator != null) enemy.animator.SetTrigger("Death");
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        
    }
}
