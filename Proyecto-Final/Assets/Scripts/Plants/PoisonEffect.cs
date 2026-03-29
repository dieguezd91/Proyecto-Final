using System.Collections;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    private LifeController lifeController;

    private Coroutine poisonRoutine;
    private float totalDuration;
    private float tickInterval;
    private float damagePerTick;

    private bool isPoisoned = false;

    private void Awake()
    {
        lifeController = GetComponent<LifeController>();
    }

    public void ApplyPoison(float duration, float tickTime, float damage)
    {
        totalDuration = duration;
        tickInterval = tickTime;
        damagePerTick = damage;

        if (poisonRoutine != null)
        {
            StopCoroutine(poisonRoutine);
        }

        poisonRoutine = StartCoroutine(PoisonRoutine());
    }

    private IEnumerator PoisonRoutine()
    {
        isPoisoned = true;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            if (lifeController != null)
            {
                lifeController.TakeDamage(damagePerTick);
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        isPoisoned = false;
        poisonRoutine = null;

        //Destroy(this);
    }

    public bool IsPoisoned() => isPoisoned;
}