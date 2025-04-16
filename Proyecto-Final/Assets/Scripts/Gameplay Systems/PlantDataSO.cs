using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Scriptable Object/Plant Data")]
public class PlantDataSO : ScriptableObject
{
    public string plantName;
    public GameObject plantPrefab;
    public Sprite plantIcon;
    public int daysToGrow;
    public string description;
    public SeedsEnum seedType;
    public int slotIndex = 0;
}
