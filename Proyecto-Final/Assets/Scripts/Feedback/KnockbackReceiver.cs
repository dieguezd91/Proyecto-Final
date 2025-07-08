using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 0f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float knockbackDecay = 8f;

    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    private PlayerController playerController;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isKnockedBack) return;

        Vector2 knockback = direction.normalized * force * (1f - knockbackResistance);

        rb.velocity = knockback;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }

        StartCoroutine(RecoverFromKnockback());
    }

    private IEnumerator RecoverFromKnockback()
    {
        isKnockedBack = true;
        float elapsedTime = 0f;
        Vector2 initialKnockbackVelocity = rb.velocity;

        while (elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float progress = elapsedTime / knockbackDuration;

            Vector2 currentKnockback = Vector2.Lerp(initialKnockbackVelocity, Vector2.zero,
                1f - Mathf.Exp(-knockbackDecay * progress));

            rb.velocity = currentKnockback;

            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        isKnockedBack = false;
    }

    public bool IsBeingKnockedBack()
    {
        return isKnockedBack;
    }

    public void CancelKnockback()
    {
        if (isKnockedBack)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }

            isKnockedBack = false;
        }
    }
}