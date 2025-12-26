using System.Collections;
using UnityEngine;

public class HouseHealthHeartAnimator : MonoBehaviour
{
    [SerializeField] private Animator heartAnimator;

    private const string HEALTH_PARAM = "Health";

    private void Awake()
    {
        if (heartAnimator == null)
            heartAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (heartAnimator != null && !heartAnimator.enabled)
            heartAnimator.enabled = true;
    }

    public void UpdateHeartVisual(float healthPercentage)
    {
        if (heartAnimator == null)
        {
            return;
        }

        float clampedHealth = Mathf.Clamp01(healthPercentage);

        heartAnimator.SetFloat(HEALTH_PARAM, clampedHealth);

        heartAnimator.Update(0f);
    }

    public void AnimateHealing(float fromHealthPercent, float toHealthPercent, float duration)
    {
        if (heartAnimator == null)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateHealingCoroutine(fromHealthPercent, toHealthPercent, duration));
    }

    private IEnumerator AnimateHealingCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        float clampedFrom = Mathf.Clamp01(from);
        float clampedTo = Mathf.Clamp01(to);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            float currentHealth = Mathf.Lerp(clampedFrom, clampedTo, smoothT);
            heartAnimator.SetFloat(HEALTH_PARAM, currentHealth);

            heartAnimator.Update(0f);

            yield return null;
        }
        heartAnimator.SetFloat(HEALTH_PARAM, clampedTo);
        heartAnimator.Update(0f);
    }
}