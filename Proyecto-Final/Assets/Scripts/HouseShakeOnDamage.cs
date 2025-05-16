using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LifeController))]
public class HouseShakeOnDamage : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Duración total del temblor en segundos")]
    [SerializeField] private float duration = 0.3f;
    [Tooltip("Magnitud máxima de desplazamiento")]
    [SerializeField] private float magnitude = 0.2f;

    private LifeController life;
    private Vector3 originalPos;
    private float lastHealth;

    private void Awake()
    {
        life = GetComponent<LifeController>();
        originalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        // arrancamos con full vida
        lastHealth = life.currentHealth;
        life.onHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnDisable()
    {
        life.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void OnHealthChanged(float current, float max)
    {
        // si perdió salud, sacudimos
        if (current < lastHealth)
        {
            StopAllCoroutines();
            StartCoroutine(Shake());
        }
        lastHealth = current;
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // desplazamiento aleatorio en X/Y
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // restaurar
        transform.localPosition = originalPos;
    }
}
