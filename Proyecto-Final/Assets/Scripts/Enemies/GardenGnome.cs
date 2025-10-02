using UnityEngine;
using System.Collections;

public class GardenGnome : EnemyBase
{
    [Header("Kamikaze Data")]
    [SerializeField] private GardenGnomeEnemyDataSO kamikazeData;

    [Header("Combat References")]
    [SerializeField] private LayerMask playerLayerMask;

    private float acceleration;
    private float stopDistance;
    private float chaseYOffset;
    private float clingDuration;
    private float minExplosionDamage;
    private float maxExplosionDamage;

    private Vector2 velocity;
    private bool isClinging = false;
    private Transform player;
    private LifeController playerLife;
    private LifeController targetLife;

    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        rb.drag = 0.5f;
    }

    protected override void LoadEnemyData()
    {
        base.LoadEnemyData();

        if (kamikazeData != null)
        {
            acceleration = kamikazeData.Acceleration;
            stopDistance = kamikazeData.StopDistance;
            chaseYOffset = kamikazeData.ChaseYOffset;
            clingDuration = kamikazeData.ClingDuration;
            minExplosionDamage = kamikazeData.MinExplosionDamage;
            maxExplosionDamage = kamikazeData.MaxExplosionDamage;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No GnomeEnemyDataSO assigned!");
        }
    }

    protected override void Start()
    {
        base.Start();
        CachePlayerReference();
    }

    protected override void Update()
    {
        base.Update();

        if (player == null || playerLife == null)
        {
            CachePlayerReference();
        }
        else if (!playerLife.IsTargetable())
        {
            player = null;
            playerLife = null;
        }
    }

    protected override void ProcessMovement()
    {
        if (isClinging || player == null || playerLife == null || !playerLife.IsTargetable())
            return;

        Vector2 targetPos = (Vector2)player.position + Vector2.down * chaseYOffset;
        Vector2 displacement = targetPos - (Vector2)transform.position;
        float distance = displacement.magnitude;

        Vector2 desiredVelocity = distance > stopDistance
            ? displacement.normalized * moveSpeed
            : Vector2.zero;

        velocity = Vector2.Lerp(velocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        rb.velocity = velocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            PlayFootstepSound();
            UpdateSpriteDirection(velocity);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isClinging || player == null || playerLife == null) return;

        if ((playerLayerMask.value & (1 << col.gameObject.layer)) != 0)
        {
            LifeController life = col.GetComponent<LifeController>();
            if (life != null && life.IsTargetable())
            {
                StartCoroutine(ClingAndExplode(col.attachedRigidbody));
            }
        }
    }

    private IEnumerator ClingAndExplode(Rigidbody2D playerRb)
    {
        isClinging = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        Transform gripPoint = playerRb.transform.Find("GnomeGripPoint");
        if (gripPoint != null)
        {
            transform.SetParent(gripPoint, true);
            transform.position = gripPoint.position;
        }
        else
        {
            transform.SetParent(playerRb.transform, true);
        }

        float dirX = playerRb.transform.position.x - transform.position.x;
        spriteRenderer.flipX = dirX > 0f;

        if (animator != null)
            animator.SetBool("IsClinging", true);

        targetLife = playerRb.GetComponent<LifeController>();

        yield return new WaitForSeconds(clingDuration);

        Explode();
    }

    public void Explode()
    {
        if (targetLife != null && targetLife.IsTargetable())
        {
            float damage = Random.Range(minExplosionDamage, maxExplosionDamage);
            targetLife.TakeDamage(damage);
            CameraShaker.Instance?.Shake(0.3f, 0.3f);
        }

        LifeController selfLife = GetComponent<LifeController>();
        if (selfLife != null) selfLife.Die();

        Destroy(gameObject);
    }

    private void CachePlayerReference()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        LifeController life = playerObj.GetComponent<LifeController>();
        if (life != null && life.IsTargetable())
        {
            player = playerObj.transform;
            playerLife = life;
        }
    }
}