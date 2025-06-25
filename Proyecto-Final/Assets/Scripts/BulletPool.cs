using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;

    [SerializeField] private FireBullet bulletPrefab;
    [SerializeField] private int initialSize = 10;

    private Queue<FireBullet> pool = new Queue<FireBullet>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicializar pool
        for (int i = 0; i < initialSize; i++)
        {
            var b = Instantiate(bulletPrefab, transform);
            b.gameObject.SetActive(false);
            pool.Enqueue(b);
        }
    }

    public FireBullet GetBullet()
    {
        FireBullet b;
        if (pool.Count > 0)
        {
            b = pool.Dequeue();
        }
        else
        {
            b = Instantiate(bulletPrefab, transform);
        }

        b.gameObject.SetActive(true);
        Debug.Log("Activada");
        return b;
    }


    public void ReturnBullet(FireBullet b)
    {
        b.gameObject.SetActive(false);
        pool.Enqueue(b);
    }
}

