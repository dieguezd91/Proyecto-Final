using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss : EnemyBase
{
    [Header("Boss Data")]
    [SerializeField] private BossEnemyDataSO bossData;

    [Header("Combat References")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform[] minionSpawnPoints;

    private float minAttackDistance;
    private float meleeRadius;
    private float specialRadius;
    private int meleeDamage;
    private int specialDamage;
    private float attackCooldown;
    private float meleeDelay;
    private float specialDelay;
    private GameObject minionPrefab;
    private float spawnDelayAfterMinionsDie;

    private List<GameObject> currentMinions = new List<GameObject>();
    private bool isSpawningMinions = false;
    private bool isAttacking = false;
    private float spawnCooldownTimer = 0f;

    private HashSet<GameObject> meleeDamagedObjects = new HashSet<GameObject>();
    private HashSet<GameObject> specialDamagedObjects = new HashSet<GameObject>();

    #region Initialization
    protected override EnemyDataSO GetEnemyData() => bossData;

    protected override void LoadEnemyData()
    {
        base.LoadEnemyData();

        if (bossData != null)
        {
            minAttackDistance = bossData.MinAttackDistance;
            meleeRadius = bossData.MeleeRadius;
            specialRadius = bossData.SpecialRadius;
            meleeDamage = bossData.MeleeDamage;
            specialDamage = bossData.SpecialDamage;
            attackCooldown = bossData.AttackCooldown;
            meleeDelay = bossData.MeleeDelay;
            specialDelay = bossData.SpecialDelay;
            minionPrefab = bossData.MinionPrefab;
            spawnDelayAfterMinionsDie = bossData.SpawnDelayAfterMinionsDie;

            if (lifeController != null)
            {
                lifeController.maxHealth = bossData.BossMaxHealth;
                lifeController.currentHealth = bossData.BossMaxHealth;
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No BossEnemyDataSO assigned!");
        }
    }

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();

        if (lifeController != null)
        {
            lifeController.onDeath.AddListener(OnBossDeath);
        }
    }
    #endregion

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        UpdateMinionManagement();
        UpdateBossCombat();
    }

    private void UpdateMinionManagement()
    {
        currentMinions.RemoveAll(m => m == null);

        if (!isAttacking && !isSpawningMinions && currentMinions.Count == 0)
        {
            spawnCooldownTimer += Time.deltaTime;
            if (spawnCooldownTimer >= spawnDelayAfterMinionsDie)
            {
                StartCoroutine(SpawnMinionsRoutine());
            }
        }
    }

    private void UpdateBossCombat()
    {
        if (isAttacking || isSpawningMinions || currentTarget == null) return;

        float distanceToTarget = GetDistanceToTarget();

        if (IsTargetValid())
        {
            if (distanceToTarget <= detectionRange)
            {
                if (distanceToTarget > minAttackDistance)
                {
                    Vector2 direction = (currentTarget.position - transform.position).normalized;
                    MoveTowardsTarget(direction, moveSpeed);
                }
                else
                {
                    StopMovement();
                    TriggerNextAttack();
                }
            }
            else
            {
                StopMovement();
            }
        }

        if (currentTarget != null)
        {
            Vector2 lookDirection = (currentTarget.position - transform.position).normalized;
            UpdateSpriteDirection(lookDirection);
        }
    }

    private bool IsTargetValid()
    {
        if (currentTarget == null) return false;

        if (currentTarget.CompareTag("Player") || currentTarget.CompareTag("Plant"))
        {
            LifeController life = currentTarget.GetComponent<LifeController>();
            return life != null && life.IsTargetable();
        }

        if (currentTarget.CompareTag("Home"))
        {
            HouseLifeController home = currentTarget.GetComponent<HouseLifeController>();
            return home != null;
        }

        return false;
    }

    protected override void ProcessMovement() { }

    protected override void SetMovementAnimation(bool isMoving)
    {
        if (animator != null)
            animator.SetBool("IsMoving", isMoving);
    }

    private void TriggerNextAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        SetMovementAnimation(false);

        int randomAttack = Random.Range(1, 4);

        if (randomAttack == 1)
            StartCoroutine(PerformMeleeAttack());
        else
            StartCoroutine(PerformSpecialAttack());
    }

    private IEnumerator PerformMeleeAttack()
    {
        rb.velocity = Vector2.zero;
        animator.SetTrigger("attack");
        soundBase?.PlaySound(EnemySoundType.Attack);

        yield return new WaitForSeconds(meleeDelay);

        meleeDamagedObjects.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius);

        foreach (Collider2D hit in hits)
        {
            if (!meleeDamagedObjects.Contains(hit.gameObject))
            {
                DealDamageTo(hit.gameObject, meleeDamage);
                meleeDamagedObjects.Add(hit.gameObject);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private IEnumerator PerformSpecialAttack()
    {
        rb.velocity = Vector2.zero;
        soundBase?.PlaySound(EnemySoundType.Special);

        yield return new WaitForSeconds(specialDelay);

        specialDamagedObjects.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, specialRadius);

        foreach (Collider2D hit in hits)
        {
            if (!specialDamagedObjects.Contains(hit.gameObject))
            {
                DealDamageTo(hit.gameObject, specialDamage, true);
                specialDamagedObjects.Add(hit.gameObject);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void DealDamageTo(GameObject target, int damage, bool isSpecialAttack = false)
    {
        if (target.CompareTag("Player") || target.CompareTag("Plant"))
        {
            LifeController life = target.GetComponent<LifeController>();
            if (life != null)
            {
                life.TakeDamage(damage);

                if (target.CompareTag("Player"))
                {
                    float shakeIntensity = isSpecialAttack ? 0.5f : 0.3f;
                    CameraShaker.Instance?.Shake(shakeIntensity, shakeIntensity);
                }
            }
        }
        else if (target.CompareTag("Home"))
        {
            HouseLifeController home = target.GetComponent<HouseLifeController>();
            if (home != null)
            {
                home.TakeDamage(damage);
            }
        }
    }

    private IEnumerator SpawnMinionsRoutine()
    {
        isSpawningMinions = true;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(1f);

        foreach (Transform spawnPoint in minionSpawnPoints)
        {
            GameObject minion = Instantiate(minionPrefab, spawnPoint.position, Quaternion.identity);
            currentMinions.Add(minion);
        }

        spawnCooldownTimer = 0f;
        isSpawningMinions = false;
    }

    private void KillAllMinions()
    {
        foreach (GameObject minion in currentMinions)
        {
            if (minion == null) continue;

            LifeController life = minion.GetComponent<LifeController>();
            if (life != null)
            {
                life.Die();
            }
            else
            {
                Destroy(minion);
            }
        }
        currentMinions.Clear();
    }

    private void OnBossDeath()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;
        isAttacking = true;

        soundBase?.PlaySound(EnemySoundType.Die);

        KillAllMinions();
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        CameraShaker.Instance?.Shake(1f, 0.8f);

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    public override void MarkAsDead()
    {
        if (!isDead)
        {
            OnBossDeath();
        }
    }

    #region Public API
    public float GetCurrentHealth()
    {
        return lifeController != null ? lifeController.currentHealth : 0f;
    }

    public float GetMaxHealth()
    {
        return lifeController != null ? lifeController.maxHealth : 0f;
    }
    #endregion

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (attackPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, meleeRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, specialRadius);

        if (minionSpawnPoints != null && minionSpawnPoints.Length > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform spawnPoint in minionSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                }
            }
        }
    }
}