using System.Collections.Generic;
using UnityEngine;

public class PiercingSpell : Spell
{
    [Header("SETTINGS")]
    [SerializeField] private float speed = 10f;

    [Header("PIERCING DAMAGE")]
    [SerializeField] private float firstHitDamage = 30f;
    [SerializeField] private float followingHitDamage = 15f;

    private Vector2 direction;
    private bool isInitialized = false;

    private readonly HashSet<LifeController> hitEnemies = new HashSet<LifeController>();

    private bool hasHitFirstEnemy = false;

    public override void Cast(Vector2 castDirection, Vector3 spawnPosition)
    {
        direction = castDirection.normalized;
        transform.position = spawnPosition;

        hitEnemies.Clear();
        hasHitFirstEnemy = false;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        LifeController life = collision.GetComponent<LifeController>();

        if (life == null || !life.IsAlive())
            return;

        if (hitEnemies.Contains(life))
            return;

        hitEnemies.Add(life);

        float currentDamage;

        if (!hasHitFirstEnemy)
        {
            currentDamage = firstHitDamage;
            hasHitFirstEnemy = true;
        }
        else
        {
            currentDamage = followingHitDamage;
        }

        if (RitualBuffManager.Instance != null)
        {
            currentDamage *= RitualBuffManager.Instance.GetDamageMultiplier();
        }

        life.TakeDamage(currentDamage);

        ShowDamageText(collision.transform, currentDamage);
        PlayImpactEffects(collision.transform.position);
        ApplyKnockback(collision);

    }

    public void SetDirection(Vector2 newDirection)
    {
        Cast(newDirection, transform.position);
    }
}