using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HouseHealingPotionPickup : MonoBehaviour
{
    [Tooltip("Cuántas pociones añade al inventario")]
    [SerializeField] private int amount = 1;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        InventoryManager.Instance.AddMaterial(MaterialType.HouseHealingPotion, amount);
        Destroy(gameObject);
    }
}
