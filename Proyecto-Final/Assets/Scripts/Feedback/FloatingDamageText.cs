using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1f, 0);
    public float floatSpeed = 1f;
    public float fadeInDuration = 0.2f;
    public float duration = 1.5f;
    public float fadeOutDuration = 0.5f;
    public Color startColor = new Color(1f, 0.5f, 0f, 1f);
    public Color endColor = Color.red;
    public float maxDamageForColor = 50f;

    public float minScale = 0.5f;
    public float maxScale = 1f;

    private TextMeshProUGUI textMesh;
    private Transform target;
    private float totalDamage = 0f;
    private float lastHitTime;
    private float spawnTime;
    private bool isFadingOut = false;

    private Vector3 initialWorldPosition;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(Transform target)
    {
        this.target = target;
        transform.SetParent(target, worldPositionStays: true);
        transform.localPosition = offset;

        initialWorldPosition = transform.position;

        spawnTime = Time.time;
        lastHitTime = Time.time;
        totalDamage = 0f;

        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        transform.localScale = Vector3.one * minScale;
    }

    void Update()
    {
        float now = Time.time;
        float sinceSpawn = now - spawnTime;
        float sinceLastHit = now - lastHitTime;

        float tDamage = Mathf.Clamp01(totalDamage / maxDamageForColor);

        Color dynamicColor = Color.Lerp(startColor, endColor, tDamage);

        float currentScale = Mathf.Lerp(minScale, maxScale, tDamage);
        transform.localScale = Vector3.one * currentScale;

        float totalFloatDistance = sinceSpawn * floatSpeed;

        if (!isFadingOut && target != null)
        {
            transform.position = target.position + offset + Vector3.up * totalFloatDistance;
        }
        else
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        }

        if (sinceSpawn <= fadeInDuration)
        {
            float aIn = sinceSpawn / fadeInDuration;
            textMesh.color = new Color(dynamicColor.r, dynamicColor.g, dynamicColor.b, aIn);
        }
        else if (!isFadingOut && sinceLastHit < duration)
        {
            textMesh.color = new Color(dynamicColor.r, dynamicColor.g, dynamicColor.b, 1f);
        }

        if (!isFadingOut && sinceLastHit >= duration)
        {
            isFadingOut = true;
            transform.SetParent(null, worldPositionStays: true);
        }

        if (isFadingOut)
        {
            float fadeElapsed = sinceLastHit - duration;
            float tOut = Mathf.Clamp01(fadeElapsed / fadeOutDuration);
            float aOut = Mathf.Lerp(1f, 0f, tOut);

            textMesh.color = new Color(dynamicColor.r, dynamicColor.g, dynamicColor.b, aOut);

            if (fadeElapsed >= fadeOutDuration)
                Destroy(gameObject);
        }
    }

    public void AddDamage(float dmg)
    {
        totalDamage += dmg;
        textMesh.text = totalDamage.ToString("F0");
        lastHitTime = Time.time;

        if (isFadingOut)
        {
            isFadingOut = false;
            if (target != null)
                transform.SetParent(target, worldPositionStays: true);
        }
    }
}