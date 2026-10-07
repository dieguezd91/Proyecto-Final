using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Crafting/Plant Data")]
public class PlantDataSO : ScriptableObject
{
    [Tooltip("Optional stable plant-line id; defaults to plant:<seedType>, independent of inventory slot.")]
    public string upgradeLineId;
    public string UpgradeLineId => string.IsNullOrEmpty(upgradeLineId) ? "plant:" + seedType : upgradeLineId;
    public string plantName;
    public GameObject plantPrefab;
    public Sprite plantIcon;
    public Sprite fullyGrownSprite;
    public int daysToGrow;
    public string description;
    public SeedsEnum seedType;
    public int slotIndex = 0;
    public bool hasDeathAnimation = true;
    public bool spriteFacesLeft = false;
}