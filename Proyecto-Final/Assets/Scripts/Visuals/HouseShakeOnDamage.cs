using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeController))]
public class HouseShakeOnDamage : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float magnitude = 0.2f;

    private LifeController life;
    private List<Transform> visuals = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();

    private void Awake()
    {
        life = GetComponent<LifeController>();
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            visuals.Add(sr.transform);
            originalPositions.Add(sr.transform.localPosition);
        }
    }

    private void OnEnable() => life.onDamaged.AddListener(OnDamaged);
    private void OnDisable() => life.onDamaged.RemoveListener(OnDamaged);

    private void OnDamaged(float damage)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeTremor());
    }

    private IEnumerator ShakeTremor()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - (elapsed / duration);
            float currentMag = magnitude * damper;

            Vector2 randomDir = Random.insideUnitCircle;
            Vector3 offset = new Vector3(randomDir.x, randomDir.y, 0f) * currentMag;

            for (int i = 0; i < visuals.Count; i++)
                visuals[i].localPosition = originalPositions[i] + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < visuals.Count; i++)
            visuals[i].localPosition = originalPositions[i];
    }
}
