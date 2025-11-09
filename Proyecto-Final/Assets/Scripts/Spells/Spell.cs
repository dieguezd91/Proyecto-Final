using UnityEngine;

public abstract class Spell : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] protected float damage;
    [SerializeField] protected float lifeTime;

    [Header("VFX & FEEDBACK")]
    [SerializeField] protected GameObject floatingDamagePrefab;
    [SerializeField] protected GameObject impactParticlesPrefab;

    protected virtual void Awake()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public abstract void Cast(Vector2 direction, Vector3 spawnPosition);

    protected virtual void ApplyDamage(Collider2D target)
    {
        float dmg = Random.Range(damage, damage + 5f);

        var life = target.GetComponent<LifeController>();
        life?.TakeDamage(dmg);

        ShowDamageText(target.transform, dmg);
        PlayImpactEffects(target.transform.position);
        ApplyKnockback(target);
    }

    protected virtual void ShowDamageText(Transform target, float dmg)
    {
        if (floatingDamagePrefab == null) return;

        var existing = target.GetComponentInChildren<FloatingDamageText>();

        if (existing != null)
        {
            existing.AddDamage(dmg);
        }
        else
        {
            Vector3 spawnPos = target.position + Vector3.up * 1f;
            var go = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
            var fdt = go.GetComponent<FloatingDamageText>();
            fdt?.Initialize(target);
            fdt?.AddDamage(dmg);
        }
    }

    protected virtual void PlayImpactEffects(Vector3 position)
    {
        if (impactParticlesPrefab == null) return;

        var particles = Instantiate(impactParticlesPrefab, position, Quaternion.identity);
        var ps = particles.GetComponent<ParticleSystem>();

        if (ps != null)
            Destroy(particles, ps.main.duration);
        else
            Destroy(particles, 1.5f);

        if (Time.timeScale > 0f && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(0.3f, 0.25f);
        }
    }

    protected virtual void ApplyKnockback(Collider2D target)
    {
        var knockback = target.GetComponent<KnockbackReceiver>();
        if (knockback != null)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            knockback.ApplyKnockback(direction, 8f);
        }
    }
}