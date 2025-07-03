using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private TextMeshProUGUI bubbleText;

    [Header("Animation")]
    [SerializeField] private float fadeInTime = 0.2f;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float wobbleSpeed = 1.5f;
    [SerializeField] private float wobbleAmount = 0.05f;
    [SerializeField] private Animator animator;

    private float timer = 0f;

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                Hide();
        }
    }

    public void ShowMessage(string msg, float duration = -1)
    {
        bubbleText.text = msg;
        bubbleText.ForceMeshUpdate();

        float maxWidth = 5f;
        float minWidth = 2f;
        float aspect = 0.4f;
        float paddingX = 2f, paddingY = 1f;

        var preferred = bubbleText.GetPreferredValues(msg, maxWidth, Mathf.Infinity);

        float width = Mathf.Clamp(preferred.x, minWidth, maxWidth);

        float proportionalHeight = width * aspect;
        float height = Mathf.Max(preferred.y, proportionalHeight);

        bubbleText.enableWordWrapping = true;
        bubbleText.rectTransform.sizeDelta = new Vector2(width, height);

        bubbleRect.sizeDelta = new Vector2(width + paddingX, height + paddingY);

        animator.SetTrigger("Open");
        timer = duration > 0 ? duration : showDuration;
    }


    public void Hide()
    {
        timer = 0f;
        animator.SetTrigger("Close");
        bubbleText.text = "";
    }
}
