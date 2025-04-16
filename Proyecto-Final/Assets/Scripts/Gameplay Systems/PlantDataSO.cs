using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Crafting/Plant Data")]
public class PlantDataSO : ScriptableObject
{
    public string plantName;
    public GameObject plantPrefab;
    public Sprite plantIcon;
    public Sprite startingDaySprite;
    public Sprite middleDaySprite;
    public Sprite lastDaySprite;
    public Sprite fullyGrownSprite;
    public int daysToGrow;
    public string description;
    public SeedsEnum seedType;
    public int slotIndex = 0;
}
