using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GardenGnomeController : MonoBehaviour, IEnemy
{
    [Header("MOVEMENT")]
    public float maxSpeed = 3f;
    public float acceleration = 2f;
    public float stopDistance = 0.1f;

    [Tooltip("Cuánto hacia abajo persigue el gnomo respecto al centro del jugador")]
    [SerializeField] private float chaseYOffset = 0.5f;

    [Header("EXPLOSION")]
    public float clingDuration = 2f;
    public float minExplosionDamage = 25f;
    public float maxExplosionDamage = 35f;
    public LayerMask playerLayerMask;

    [Header("LOOK")]
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D _rb;
    private Transform _player;
    private LifeController _playerLife;
    private Vector2 _velocity;
    private bool _isClinging = false;

    private Animator animator;
    private LifeController targetLife;
    private bool isDead = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.drag = 0.5f;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            var life = pObj.GetComponent<LifeController>();
            if (life != null && life.IsTargetable())
            {
                _player = pObj.transform;
                _playerLife = life;
            }
            else
            {
                _player = null;
                _playerLife = null;
            }
        }
        else
        {
            _player = null;
            _playerLife = null;
        }
    }

    void FixedUpdate()
    {
        if (_isClinging || _player == null) return;

        if (GetComponent<KnockbackReceiver>()?.IsBeingKnockedBack() == true)
            return;

        Vector2 targetPos = (Vector2)_player.position + Vector2.down * chaseYOffset;
        Vector2 disp = targetPos - (Vector2)transform.position;
        float dist = disp.magnitude;
        Vector2 desired = dist > stopDistance
            ? disp.normalized * maxSpeed
            : Vector2.zero;

        _velocity = Vector2.Lerp(_velocity, desired, acceleration * Time.fixedDeltaTime);
        _rb.velocity = _velocity;

        LookDir(targetPos, transform.position);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_isClinging || _player == null) return;

        if ((playerLayerMask.value & (1 << col.gameObject.layer)) != 0)
        {
            var life = col.GetComponent<LifeController>();
            if (life != null && life.IsTargetable())
                StartCoroutine(ClingAndExplode(col.attachedRigidbody));
        }
    }

    private IEnumerator ClingAndExplode(Rigidbody2D player)
    {
        _isClinging = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;

        Transform gripPoint = player.transform.Find("GnomeGripPoint");
        if (gripPoint != null)
        {
            transform.SetParent(gripPoint, true);
            transform.position = gripPoint.position;
        }
        else
        {
            transform.SetParent(player.transform, true);
        }

        float dirX = player.transform.position.x - transform.position.x;
        spriteRenderer.flipX = dirX > 0f;

        if (animator != null)
            animator.SetBool("IsClinging", true);

        targetLife = player.GetComponent<LifeController>();

        yield return new WaitForSeconds(1.2f);

        Explode();
    }


    public void Explode()
    {
        if (targetLife != null)
        {
            float dmg = Random.Range(minExplosionDamage, maxExplosionDamage);
            targetLife.TakeDamage(dmg);
            CameraShaker.Instance?.Shake(0.3f, 0.3f);
        }

        var selfLife = GetComponent<LifeController>();
        if (selfLife != null) selfLife.Die();

        Destroy(gameObject);
    }


    private void LookDir(Vector2 targetPos, Vector2 currentPos)
    {
        Vector2 lookDir = targetPos - currentPos;
        LookAtDirection(lookDir);
    }

    private void LookAtDirection(Vector2 direction)
    {
        if (direction.x > 0.1f)
        {
            spriteRenderer.flipX = true;
            
        }
        else if (direction.x < -0.1f)
        {
            spriteRenderer.flipX = false;
            
        }
    }

    public void MarkAsDead()
    {
        isDead = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
    }
}
