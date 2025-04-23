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
                    GameManager.Instance.uiManager.ShowDamagedScreen();
                }
            }
        }
        
        
        if (collision.CompareTag("Plant") || collision.CompareTag("Home"))
        {
            LifeController life = collision.GetComponent<LifeController>();
            if (life != null && life.IsAlive())
            {
                life.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}