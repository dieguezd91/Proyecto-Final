using System.Collections;
using UnityEngine;

public class HouseController : MonoBehaviour
{
    [Header("REFERENCES")]
    public SpriteRenderer roofSprite;

    [Header("SETTINGS")]
    public float fadeSpeed = 2.0f;
    public float delayBeforeFade = 0.2f;

    private bool isInside = false;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (roofSprite == null)
        {
            Debug.LogError("No se asigno el renderer del techo");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInside)
        {
            isInside = true;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeRoof(0.0f));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInside)
        {
            isInside = false;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeRoof(1.0f));
        }
    }

    private IEnumerator FadeRoof(float targetAlpha)
    {
        yield return new WaitForSeconds(delayBeforeFade);

        if (roofSprite == null)
        {
            yield break;
        }

        float currentAlpha = roofSprite.color.a;

        float elapsedTime = 0;
        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            float t = Mathf.Clamp01(elapsedTime);

            Color color = roofSprite.color;
            color.a = Mathf.Lerp(currentAlpha, targetAlpha, t);
            roofSprite.color = color;

            yield return null;
        }
    }
}