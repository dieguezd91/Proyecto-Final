using System.Collections;
using UnityEngine;

public class FireTrail : MonoBehaviour
{
    public float duration = 2f;
    public float damagePerSecond = 10f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            LifeController life = collision.GetComponent<LifeController>();
            if (life != null && life.IsAlive())
            {
                life.TakeDamage(damagePerSecond * Time.deltaTime);
                if (GameManager.Instance.uiManager != null)
                {
                    CameraShaker.Instance?.Shake(0.2f, 0.2f);
                }
            }
            else
            {
                var houseLife = collision.GetComponent<HouseLifeController>();
                if (houseLife != null)
                    houseLife.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
        else if (collision.CompareTag("Plant") || collision.CompareTag("Home"))
        {
            var life = collision.GetComponent<LifeController>();
            if (life != null && life.IsAlive())
            {
                life.TakeDamage(damagePerSecond * Time.deltaTime);
            }
            else
            {
                var houseLife = collision.GetComponent<HouseLifeController>();
                if (houseLife != null)
                    houseLife.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}