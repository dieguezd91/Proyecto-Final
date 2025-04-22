using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    [SerializeField] private CameraOffsetController cameraOffsetController;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shake(float intensity, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float currentIntensity = intensity * shakeCurve.Evaluate(normalizedTime);

            Vector3 offset = (Vector3)Random.insideUnitCircle * currentIntensity;
            cameraOffsetController?.SetOffset(offset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraOffsetController?.ResetOffset();
    }
}