using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class LootItem
{
    public string name;
    public GameObject itemPrefab;
    [Range(0f, 100f)] public float dropChance;
}

public class LifeController : MonoBehaviour
{
    public enum DamageType
    {
        SingleTick,
        DamageOverTime
    }

    public enum DamageElement
    {
        Normal,
        Fire,
    }

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
    public UnityEvent<float, DamageType> onDamaged;

    [Header("MANA DROP")]
    [SerializeField] private GameObject manaPickupPrefab;
    [SerializeField] public float manaDropChance = 1f;

    [Header("OBJECT DROP SYSTEM")]
    [SerializeField] private List<LootItem> lootTable;
    [SerializeField] private float dropScatterDistance = 0.5f;
    [SerializeField] private float explosionForce = 3f;

    [Header("AUDIO SETTINGS")]
    [SerializeField] private float dotSoundCooldown = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    public bool isRespawning = false;
    [SerializeField] private bool isEnemy;
    [SerializeField] private bool isPlayer;
    [SerializeField] private bool isPlant;
    private Animator animator;
    [SerializeField] private bool hasDeathAnimation;

    private float lastDotSoundTime = 0f;

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

        isPlayer = GetComponent<PlayerController>() != null;
    }

    public void TakeDamage(float damage, DamageType damageType = DamageType.SingleTick, DamageElement damageElement = DamageElement.Normal)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        if (damage > 0)
        {
            onDamaged?.Invoke(damage, damageType);

            if (isPlayer)
            {
                if (damageType == DamageType.SingleTick)
                {
                    switch (damageElement)
                    {
                        case DamageElement.Normal:
                            SoundManager.Instance.Play("PlayerHit");
                            break;
                        
                        case DamageElement.Fire:
                            SoundManager.Instance.Play("PlayerHitBurn");
                            break;
                        
                        default:
                            SoundManager.Instance.Play("PlayerHit");
                            break;
                    }
                }
                
                else if (damageType == DamageType.DamageOverTime)
                {
                    if (Time.time - lastDotSoundTime >= dotSoundCooldown)
                    {
                        SoundManager.Instance.Play("PlayerBurn");
                        lastDotSoundTime = Time.time;
                    }
                }
            }
            
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
        if (isDead) return;

        isDead = true;

        if (isPlayer)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
                playerController.SetCanAct(false);
            }
        }

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        onDeath?.Invoke();

        if (isEnemy)
        {
            GetComponent<IEnemy>()?.MarkAsDead();

            if (hasDeathAnimation && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");
            }
            else
            {
                EnemyDeath();
            }
        }
        else if (isPlayer)
        {
            GetComponent<ManaSystem>()?.SetMana(0f);
            LevelManager.Instance?.uiManager?.UpdateManaUI();

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                LevelManager.Instance?.uiManager?.SetGrayscaleGhostEffect(true);
                animator.SetTrigger("Death");
                animator.SetBool("IsDead", true);

                var playerController = GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetMovementEnabled(false);
                    playerController.SetCanAct(false);

                    var rb = GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                    }
                }
            }
            else
            {
                var playerController = GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetMovementEnabled(false);
                    playerController.SetCanAct(false);
                }
            }
        }
        else if (isPlant)
        {
            if (hasDeathAnimation && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");
            }
            else
            {
                PlantDeath();
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
            EnemyDeath();
        }
        else if (isPlayer)
        {
            StartCoroutine(DelayedRevive());
        }
        else if (isPlant)
        {
            PlantDeath();
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }

    private void PlantDeath()
    {
        Destroy(gameObject);
    }

    public void ConfigureAsPlant(bool hasAnimation)
    {
        isPlant = true;
        hasDeathAnimation = hasAnimation;
        isEnemy = false;
        isPlayer = false;
    }

    private void EnemyDeath()
    {
        Drop();
        Destroy(gameObject);
    }

    private IEnumerator DelayedRevive()
    {
        float delay = LevelManager.Instance != null ?
            LevelManager.Instance.playerRespawnTime : 2f;

        LevelManager.Instance?.uiManager?.AnimateRespawnRecovery(delay);

        yield return new WaitForSeconds(0.5f);

        isRespawning = true;
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetCanAct(false);
        }

        yield return new WaitForSeconds(delay - 0.5f);

        ResetLife();
        GetComponent<PlayerController>()?.SetMovementEnabled(true);
        GetComponent<PlayerController>()?.SetCanAct(true);
    }

    public void Drop()
    {
        if (lootTable != null && lootTable.Count > 0)
        {
            foreach (var loot in lootTable)
            {
                if (UnityEngine.Random.Range(0f, 100f) <= loot.dropChance)
                {
                    if (loot.itemPrefab != null)
                    {
                        SpawnAndExplode(loot.itemPrefab);
                    }
                }
            }
        }

        if (manaPickupPrefab != null && UnityEngine.Random.value < manaDropChance)
        {
            SpawnAndExplode(manaPickupPrefab);
        }
    }

    private void SpawnAndExplode(GameObject prefabToSpawn)
    {
        GameObject newItem = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

        Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

            rb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);
        }
    }

    public void ResetLife()
    {
        isRespawning = false;
        isDead = false;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = true;

        animator.SetBool("IsDead", false);
        animator.ResetTrigger("Death");
        animator.SetTrigger("Revive");
        LevelManager.Instance?.uiManager?.SetGrayscaleGhostEffect(false);

        RefreshPlayerUI();
    }

    private void RefreshPlayerUI()
    {
        if (LevelManager.Instance?.uiManager != null)
        {
            LevelManager.Instance.uiManager.UpdateHealthBar(currentHealth, maxHealth);

            var manaSystem = GetComponent<ManaSystem>();
            if (manaSystem != null)
            {
                LevelManager.Instance.uiManager.UpdateManaUI();
            }
        }

        UICursor cursorController = FindObjectOfType<UICursor>();
        if (cursorController != null)
        {
            cursorController.SetCursorForGameState(LevelManager.Instance.currentGameState);
        }
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

    public void OnReviveAnimationEnd()
    {
        var pc = GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetCanAct(true);
            pc.RefreshHandNightness();
        }
    }
}