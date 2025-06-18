using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GardenGnomeController : MonoBehaviour
{
    [Header("MOVEMENT")]
    public float maxSpeed = 3f;
    public float acceleration = 2f;
    public float stopDistance = 0.1f;

    [Tooltip("Cuánto hacia abajo persigue el gnomo respecto al centro del jugador")]
    [SerializeField] private float chaseYOffset = 0.5f;

    [Header("EXPLOSION")]
    public float clingDuration = 2f;
    public float explosionRadius = 2f;
    public float explosionDamage = 30f;
    public LayerMask playerLayerMask;

    [Header("LOOK")]
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D _rb;
    private Transform _player;
    private Vector2 _velocity;
    private bool _isClinging = false;

    private Animator animator;
    private LifeController targetLife;

    void Awake()
    {
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.drag = 0.5f;
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator != null)
            animator.Play("GnomeWalk");
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void FixedUpdate()
    {
        if (_isClinging || _player == null) return;

        
        Vector2 targetPos = (Vector2)_player.position + Vector2.down * chaseYOffset;
        Vector2 displacement = targetPos - (Vector2)transform.position;
        float dist = displacement.magnitude;

        Vector2 desiredVel = dist > stopDistance
            ? displacement.normalized * maxSpeed
            : Vector2.zero;

        _velocity = Vector2.Lerp(_velocity, desiredVel, acceleration * Time.fixedDeltaTime);
        _rb.velocity = _velocity;

        LookDir(targetPos, transform.position);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (_isClinging) return;
        if ((playerLayerMask.value & (1 << col.gameObject.layer)) != 0)
            StartCoroutine(ClingAndExplode(col.collider));
    }

    private IEnumerator ClingAndExplode(Collider2D playerCollider)
    {
        _isClinging = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        transform.SetParent(playerCollider.transform, true);

        if (animator != null)
            animator.Play("GnomeExplode");

        targetLife = playerCollider.GetComponent<LifeController>();

        yield return null;
    }

    public void Explode()
    {
        if (targetLife != null)
        {
            targetLife.TakeDamage(explosionDamage);
        }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
