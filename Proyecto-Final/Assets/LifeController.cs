using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LifeController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Visual Feedback")]
    public bool flashOnDamage = true;
    public float flashDuration = 0.1f;
    public int numberOfFlashes = 3;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float, float> onHealthChanged;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;

    void Start()
    {
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

        if (flashOnDamage && spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        currentHealth = Mathf.Min(currentHealth, maxHealth);

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} healed for {amount}. Current health: {currentHealth}/{maxHealth}");
    }

    public virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} died!");

        onDeath?.Invoke();
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
}