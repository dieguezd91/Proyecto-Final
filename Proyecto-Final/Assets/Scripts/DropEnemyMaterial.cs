using System.Collections;
using UnityEngine;

public class DropEnemyMaterial : MonoBehaviour
{
    [SerializeField] private CraftingMaterialSO materialData;
    [SerializeField] private float attractionSpeed = 8f;
    private Transform player;
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        if (collision.gameObject.layer != 7) return;

        player = collision.transform;
        isCollected = true;
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(MoveToPlayerAndCollect());
    }

    private IEnumerator MoveToPlayerAndCollect()
    {
        while (Vector2.Distance(transform.position, player.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractionSpeed * Time.deltaTime
            );
            yield return null;
        }

        SoundManager.Instance.PlayOneShot("PickUp");
        if (materialData != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddMaterial(materialData.materialType, 1);
        }
        var pickupHandler = player.GetComponentInChildren<FloatingTextController>();
        if (pickupHandler != null && materialData != null)
        {
            pickupHandler.ShowPickup(materialData.materialName, 1, materialData.materialIcon);
        }

        Destroy(gameObject);
    }
}
