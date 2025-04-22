using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    public float knockbackResistance = 0f;
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    [SerializeField] private float knockbackDuration = 0.2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isKnockedBack) return;

        Vector2 knockback = direction.normalized * force * (1f - knockbackResistance);
        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);
        StartCoroutine(RecoverFromKnockback());
    }

    private System.Collections.IEnumerator RecoverFromKnockback()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(knockbackDuration);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    public bool IsBeingKnockedBack()
    {
        return isKnockedBack;
    }
}
