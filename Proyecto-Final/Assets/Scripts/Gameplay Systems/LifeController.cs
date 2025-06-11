using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
            }
            else
            {
                Drop();
                Destroy(gameObject);
            }
        }
        else
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");
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
        Drop();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeathAnimationComplete();
        }

        gameObject.SetActive(false);
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

        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = true;
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public IEnumerator StartInvulnerability(float duration)
    {
        isRespawning = true;
        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = toggle ? 0.3f : 1f;
                spriteRenderer.color = color;
                toggle = !toggle;
            }

            elapsed += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        if (spriteRenderer != null)
        {
            Color finalColor = spriteRenderer.color;
            finalColor.a = 1f;
            spriteRenderer.color = finalColor;
        }

        isRespawning = false;
    }
}