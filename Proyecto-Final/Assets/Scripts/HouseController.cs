using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public class HouseController : MonoBehaviour
{
    [Header("REFERENCES")]
    public SpriteRenderer roofSprite;
    public Light2D[] nightLights;
    public ShadowCaster2D[] shadowCastersToDisable;

    [Header("DOOR")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private float doorAnimDuration = 0.5f;

    [Header("FADE SETTINGS")]
    [SerializeField] private float roofFadeDelay = 0.2f;
    [SerializeField] private float roofFadeSpeed = 2f;
    [SerializeField] private float doorFadeDelay = 0f;
    [SerializeField] private float doorFadeSpeed = 2f;

    [Header("NIGHT")]
    [SerializeField] private float nightIntensity = 1.5f;

    private bool isInside = false;
    private Coroutine roofFadeCoroutine;
    private Coroutine doorFadeCoroutine;
    private Coroutine doorSequenceCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isInside) return;
        isInside = true;
        if (doorSequenceCoroutine != null) StopCoroutine(doorSequenceCoroutine);
        doorSequenceCoroutine = StartCoroutine(OpenDoorThenFade());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isInside) return;
        isInside = false;
        if (doorSequenceCoroutine != null) StopCoroutine(doorSequenceCoroutine);
        doorSequenceCoroutine = StartCoroutine(CloseDoorThenUnfade());
    }

    private IEnumerator OpenDoorThenFade()
    {
        doorAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(doorAnimDuration);
        if (roofFadeCoroutine != null) StopCoroutine(roofFadeCoroutine);
        roofFadeCoroutine = StartCoroutine(FadeSprite(roofSprite, 0f, roofFadeDelay, roofFadeSpeed));

        if (doorFadeCoroutine != null) StopCoroutine(doorFadeCoroutine);
        doorFadeCoroutine = StartCoroutine(FadeSprite(doorSprite, 0f, doorFadeDelay, doorFadeSpeed));
    }

    private IEnumerator CloseDoorThenUnfade()
    {
        doorAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(doorAnimDuration);
        if (roofFadeCoroutine != null) StopCoroutine(roofFadeCoroutine);
        roofFadeCoroutine = StartCoroutine(FadeSprite(roofSprite, 1f, roofFadeDelay, roofFadeSpeed));

        if (doorFadeCoroutine != null) StopCoroutine(doorFadeCoroutine);
        doorFadeCoroutine = StartCoroutine(FadeSprite(doorSprite, 1f, doorFadeDelay, doorFadeSpeed));
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha, float delay, float speed)
    {
        if (sr == null) yield break;
        yield return new WaitForSeconds(delay);
        float start = sr.color.a;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float a = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t));
            var c = sr.color;
            c.a = a;
            sr.color = c;
            yield return null;
        }
    }

    public void SetNightMode(bool isNight)
    {
        foreach (var light in nightLights)
            light.intensity = isNight ? nightIntensity : 0f;
        foreach (var sc in shadowCastersToDisable)
            sc.enabled = !isNight;
    }
}
