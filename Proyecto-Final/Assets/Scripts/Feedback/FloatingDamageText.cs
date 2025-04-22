using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float floatSpeed = 1f;
    public float duration = 1f;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    public void SetText(float damage)
    {
        textMesh.text = damage.ToString("F0");
    }
}
