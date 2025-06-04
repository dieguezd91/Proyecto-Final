using TMPro;
using UnityEngine;

public class FloatingWarningUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float floatSpeed = 0.5f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    public void Show(string message)
    {
        if (text != null)
            text.text = message;
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        transform.rotation = Quaternion.identity;
    }
}
