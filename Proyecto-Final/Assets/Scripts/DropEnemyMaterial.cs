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
                Debug.Log($"Jugador recogio: {materialData.materialName})");
            }

            Destroy(gameObject);
        }
    }
}
