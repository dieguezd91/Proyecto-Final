using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimator : MonoBehaviour
{
    [Header("Sprites to Animate")]
    public List<Sprite> sprites = new List<Sprite>();

    [Header("Animation Settings")]
    public float frameDelay = 0.1f;
    public bool enableColliderToggle = false;
    public Collider2D targetCollider;

    private SpriteRenderer spriteRenderer;
    private bool isAnimating = false;
    private bool isForward = true;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (sprites.Count > 0)
            spriteRenderer.sprite = sprites[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TriggerAnimation();
        }
    }

    public void TriggerAnimation()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        isAnimating = true;

        int startIndex = isForward ? 0 : sprites.Count - 1;
        int endIndex = isForward ? sprites.Count - 1 : 0;
        int direction = isForward ? 1 : -1;

        if (enableColliderToggle && targetCollider != null)
            targetCollider.enabled = !targetCollider.enabled;

        for (int i = startIndex; isForward ? i <= endIndex : i >= endIndex; i += direction)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(frameDelay);
        }

        isForward = !isForward;
        isAnimating = false;
    }
}

