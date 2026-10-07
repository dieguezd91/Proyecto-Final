using System.Collections;
using UnityEngine;

public class FireSpell : Spell
{
    [Header("FLAMETHROWER SETTINGS")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float fireRate = 0.05f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 0.5f;

    [Header("DAMAGE SETTINGS")]
    [SerializeField] private float damagePerHit = 5f;

    public override MagicGarden.Target DescribeUpgradeTarget(string id, string displayName)
    {
        var target = new MagicGarden.Target { Id = id, Name = displayName, Capability = GetType().Name };
        target.Bases[MagicGarden.Stat.Damage] = damagePerHit;
        target.Bases[MagicGarden.Stat.Range] = projectileSpeed * projectileLifeTime;
        return target;
    }
    private bool isActive = false;
    private int spellSlotIndex = -1;
    private PlayerSpellController playerSpellController;
    private Rigidbody2D rb;

    [HideInInspector] public bool isProjectile = false;

    protected override void Awake()
    {
        // Intentionally do not call base.Awake: FireRoutine and ProjectileLifetime
        // own flame lifetimes; the generic lifetime could destroy the controller early.
        rb = GetComponent<Rigidbody2D>();
    }


    public override void Cast(Vector2 direction, Vector3 spawnPosition)
    {
        transform.position = spawnPosition;

        if (isActive)
            return;

        isActive = true;

        if (!isProjectile)
        {
            
            if (playerSpellController == null)
            {
                playerSpellController = FindObjectOfType<PlayerSpellController>();
            }

            if (playerSpellController != null)
            {
                playerSpellController.SetFireSpellActive(true);

                transform.SetParent(playerSpellController.transform, true); 
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }

            StartCoroutine(FireRoutine());
        }
        else
        {

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = direction.normalized * projectileSpeed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            StartCoroutine(ProjectileLifetime());
        }
    }

    private IEnumerator ProjectileLifetime()
    {
        float elapsed = 0;
        while (elapsed < Upgraded(MagicGarden.Stat.Range, projectileLifeTime))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void SetSpellSlotIndex(int slotIndex)
    {
        spellSlotIndex = slotIndex;
    }

    private IEnumerator FireRoutine()
    {
        float timer = 0f;
        Camera mainCam = Camera.main;

        while (timer < duration)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector2 currentMouseDirection = (mousePos - transform.position).normalized;

            GameObject flameProj = Instantiate(gameObject, transform.position, Quaternion.identity);
            flameProj.transform.SetParent(null);

            FireSpell flameScript = flameProj.GetComponent<FireSpell>();
            flameScript.SetUpgradeTarget(UpgradeTargetId, UpgradeCapability);
            flameScript.isProjectile = true;
            flameScript.isActive = false;

            flameScript.Cast(currentMouseDirection, transform.position);

            yield return new WaitForSeconds(fireRate);
            timer += fireRate;
        }

        if (playerSpellController != null)
            playerSpellController.SetFireSpellActive(false);

        StartCooldown();
        Destroy(gameObject);
    }

    private void StartCooldown()
    {
        if (SpellInventory.Instance == null || spellSlotIndex < 0 || isProjectile)
            return;

        SpellInventory.Instance.StartCooldown(spellSlotIndex);
    }

    private void OnDestroy()
    {
        if (!isProjectile && playerSpellController != null)
            playerSpellController.SetFireSpellActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive || !isProjectile)
            return;

        if (collision.CompareTag("Enemy"))
        {
            LifeController life = collision.GetComponent<LifeController>();

            if (life != null && life.IsAlive())
            {
                float damage = Upgraded(MagicGarden.Stat.Damage, damagePerHit);

                if (RitualBuffManager.Instance != null)
                {
                    damage *= RitualBuffManager.Instance.GetDamageMultiplier();
                }

                life.TakeDamage(damage);
            }
        }
    }
}