using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrail : MonoBehaviour
{
    public float duration = 2f;
    public float damagePerSecond = 10f;
    [SerializeField] private GameObject DamagedScreen;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player") || collision.CompareTag("Plant") || collision.CompareTag("Home"))
        {
            LifeController life = collision.GetComponent<LifeController>();
            if (life != null && life.IsAlive())
            {
                life.TakeDamage(damagePerSecond * Time.deltaTime);
                DamagedScreen.SetActive(true);
                StartCoroutine(DamagedScreenOff());

            }
        }
    }

    IEnumerator DamagedScreenOff()
    {
        yield return new WaitForSeconds(0.5f);
        DamagedScreen.SetActive(false);
    }
}


