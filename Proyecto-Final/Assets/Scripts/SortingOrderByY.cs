using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortingOrderByY : MonoBehaviour
{
    [SerializeField] private float referenceY = 0f;

    [SerializeField] private int sortingOffset = 0;

    [SerializeField] private int immaturePlantSortingOrder = -500;

    private SpriteRenderer spriteRenderer;
    private Plant plant;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        plant = GetComponent<Plant>();
    }

    private void LateUpdate()
    {
        if (plant != null && !plant.IsFullyGrown())
        {
            spriteRenderer.sortingOrder = immaturePlantSortingOrder;
            return;
        }

        int order = Mathf.RoundToInt((referenceY - transform.position.y) * 100) + sortingOffset;
        spriteRenderer.sortingOrder = order;
    }
}
