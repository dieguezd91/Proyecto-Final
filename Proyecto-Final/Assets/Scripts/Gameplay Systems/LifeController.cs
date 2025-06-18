using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class LifeController : MonoBehaviour
{
    [Header("HEALTH SETTINGS")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("VISUAL FEEDBACK")]
    public bool flashOnDamage = true;
    public float flashDuration = 0.1f;
    public int numberOfFlashes = 3;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("EVENTS")]
    public UnityEvent onDeath;
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent<float> onDamaged;

    [Header("OBJECT DROP")]
    [SerializeField] private GameObject objetDrop;

    [Header("MANA DROP")]
    [SerializeField] private GameObject manaPickupPrefab;
    [SerializeField] private float manaDropChance = 1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    public bool isRespawning = false;
    [SerializeField] private bool isEnemy;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        if (damage > 0)
        {
            onDamaged?.Invoke(damage);

            if (flashOnDamage && spriteRenderer != null)
            {
                StartCoroutine(FlashRoutine());
            }
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        isDead = true;

        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        onDeath?.Invoke();

        if (isEnemy)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");

                StartCoroutine(DestroyAfterDelay(1f));
            }
            else
            {
                Drop();
                Destroy(gameObject);
            }
        }
        else
        {
            GetComponent<ManaSystem>()?.SetMana(0f);
            GameManager.Instance?.uiManager?.UpdateManaUI();

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");
                animator.SetBool("IsDead", true);
                GetComponent<PlayerController>()?.SetMovementEnabled(false);
                GetComponent<PlayerController>()?.SetCanAct(false);
            }
            else
            {
                Drop();
                gameObject.SetActive(false);
            }
        }
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;

            if (i < numberOfFlashes - 1)
                yield return new WaitForSeconds(flashDuration);
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    public void Kill()
    {
        currentHealth = 0;
        TakeDamage(float.MaxValue);
    }

    public void OnDeathAnimationEnd()
    {
        if (isEnemy)
        {
            Drop();
            Destroy(gameObject);
        }
    }

    public void Drop()
    {
        // 50% de probabilidad de dropear el objeto(Abierto a ser modificado)
        if (objetDrop != null && Random.value < 0.5f)
        {
            Instantiate(objetDrop, transform.position, Quaternion.identity);
        }

        if (manaPickupPrefab != null && Random.value < manaDropChance)
        {
            Instantiate(manaPickupPrefab, transform.position, Quaternion.identity);
        }
    }

    public void ResetLife()
    {
        isDead = false;
        currentHealth = maxHealth;

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = true;

        animator.SetBool("IsDead", false);
        animator.ResetTrigger("Death");
        animator.SetTrigger("Revive");
    }

    public IEnumerator StartInvulnerability(float duration)
    {
        isRespawning = true;

        yield return new WaitForSeconds(duration);

        isRespawning = false;
    }

    public bool IsTargetable()
    {
        return !isRespawning && !isDead;
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this != null && gameObject != null)
        {
            Drop();
            Destroy(gameObject);
        }
    }

    public void OnReviveAnimationEnd()
    {
        GetComponent<PlayerController>()?.SetCanAct(true);
    }
}