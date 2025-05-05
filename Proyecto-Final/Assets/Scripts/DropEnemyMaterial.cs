using UnityEngine;

public class DropEnemyMaterial : MonoBehaviour
{
    [SerializeField] private CraftingMaterialSO materialData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            if (materialData == null)
            {
                return;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMaterial(materialData.materialType, 1);
            }

            var player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                var pickupHandler = player.GetComponentInChildren<FloatingPickupText>();
                if (pickupHandler != null)
                {
                    pickupHandler.ShowPickup(materialData.materialName, 1);
                }
            }

            Destroy(gameObject);
        }
    }
}
