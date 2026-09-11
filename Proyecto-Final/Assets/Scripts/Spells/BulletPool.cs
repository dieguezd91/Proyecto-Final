using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;

    [Header("Fire Bullet")]
    [SerializeField] private FireBullet bulletPrefab;
    [SerializeField] private int initialFireBulletSize = 10;

    [Header("Ice Ball")]
    [SerializeField] private FireBullet iceBallPrefab;
    [SerializeField] private int initialIceBallSize = 10;

    private Queue<FireBullet> fireBulletPool = new Queue<FireBullet>();
    private Queue<FireBullet> iceBallPool = new Queue<FireBullet>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inicializar Fire Bullets
        for (int i = 0; i < initialFireBulletSize; i++)
        {
            var b = Instantiate(bulletPrefab, transform);
            b.gameObject.SetActive(false);
            fireBulletPool.Enqueue(b);
        }

        // Inicializar Ice Balls
        for (int i = 0; i < initialIceBallSize; i++)
        {
            var ice = Instantiate(iceBallPrefab, transform);
            ice.gameObject.SetActive(false);
            iceBallPool.Enqueue(ice);
        }
    }

    public FireBullet GetBullet()
    {
        FireBullet b;

        if (fireBulletPool.Count > 0)
        {
            b = fireBulletPool.Dequeue();
        }
        else
        {
            b = Instantiate(bulletPrefab, transform);
        }

        b.gameObject.SetActive(true);
        return b;
    }

    public void ReturnBullet(FireBullet b)
    {
        if (b == null)
            return;

        b.gameObject.SetActive(false);
        fireBulletPool.Enqueue(b);
    }

    public FireBullet GetIceBall()
    {
        FireBullet ice;

        if (iceBallPool.Count > 0)
        {
            ice = iceBallPool.Dequeue();
        }
        else
        {
            ice = Instantiate(iceBallPrefab, transform);
        }

        ice.gameObject.SetActive(true);
        return ice;
    }

    public void ReturnIceBall(FireBullet ice)
    {
        if (ice == null)
            return;

        ice.gameObject.SetActive(false);
        iceBallPool.Enqueue(ice);
    }
}