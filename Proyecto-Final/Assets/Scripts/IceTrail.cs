using System.Collections;
using UnityEngine;

public class IceTrail : MonoBehaviour
{
    public float duration = 2f;
    public float damagePerSecond = 10f;

    [Header("Ice Slow")]
    [SerializeField, Range(0f, 1f)] private float slowAmount = 0.25f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovementController player = collision.GetComponent<PlayerMovementController>();

            if (player != null)
            {
                player.ApplyIceSlow(slowAmount);
            }

            LifeController life = collision.GetComponent<LifeController>();
            if (life != null && life.IsAlive())
            {
                life.TakeDamage(
                    damagePerSecond * Time.deltaTime,
                    LifeController.DamageType.DamageOverTime
                );

                if (UIManager.Instance != null)
                {
                    CameraShaker.Instance?.Shake(0.3f, 0.3f);
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovementController player = collision.GetComponent<PlayerMovementController>();

            if (player != null)
            {
                player.RemoveIceSlow();
            }
        }
    }
}