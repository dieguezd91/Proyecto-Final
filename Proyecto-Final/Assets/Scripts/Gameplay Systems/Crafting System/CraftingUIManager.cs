using UnityEngine;

// Compatibility shell for existing serialized scene/prefab references. The cauldron
// now renders composed special upgrades in UpgradePanel, never the old seed recipes.
public class CraftingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject craftingUIPanel;
    private void Awake()
    {
        if (craftingUIPanel != null) craftingUIPanel.SetActive(false);
    }
    public void CloseCraftingUI()
    {
        var runtime = UpgradeRuntime.Current;
        if (runtime != null) runtime.CloseShop();
    }
    public void ShowSelectedRecipe(PlantDataSO plantData) { }
}
