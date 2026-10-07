using UnityEngine;

public class BasicRangeSpell : Spell
{
    [Header("SETTINGS")]
    [SerializeField] private float speed = 10f;

    public override MagicGarden.Target DescribeUpgradeTarget(string id, string displayName)
    {
        var target = base.DescribeUpgradeTarget(id, displayName);
        target.Bases[MagicGarden.Stat.Range] = speed * lifeTime;
        target.Bases[MagicGarden.Stat.Quantity] = 1;
        return target;
    }
    private Vector2 direction;
    private bool isInitialized = false;

    public override void Cast(Vector2 castDirection, Vector3 spawnPosition)
    {
        direction = castDirection.normalized;
        transform.position = spawnPosition;
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
        if (!collision.CompareTag("Enemy")) return;

        ApplyDamage(collision);
        Destroy(gameObject);
    }

    public void SetDirection(Vector2 newDirection)
    {
        Cast(newDirection, transform.position);
    }
}