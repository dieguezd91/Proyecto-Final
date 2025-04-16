using UnityEngine;

public class CraftingSystemTester : MonoBehaviour
{
    [SerializeField] private CraftingSystem craftingSystem;
    [SerializeField] private SeedsEnum seedToCraft;

    [ContextMenu("Craft Selected Seed")]
    public void CraftTestSeed()
    {
        craftingSystem.CraftSeed(seedToCraft);
    }
}