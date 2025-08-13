using UnityEngine;

public class SpriteChangeOnProximity : MonoBehaviour
{
    [SerializeField] private float changeDistance = 2.5f;
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite nearSprite;
    //[SerializeField] private Color gizmoColor = Color.yellow;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && defaultSprite != null)
            spriteRenderer.sprite = defaultSprite;
    }

    private void Update()
    {
        if (player == null || spriteRenderer == null)
            return;

        float dist = Vector2.Distance(player.position, transform.position);

        if (dist <= changeDistance)
        {
            spriteRenderer.sprite = nearSprite;
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }


    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = gizmoColor;
    //    Gizmos.DrawWireSphere(transform.position, changeDistance);
    //}
}
