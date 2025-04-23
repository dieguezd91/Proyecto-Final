using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlantingSystem : MonoBehaviour
{
    public static TilePlantingSystem Instance { get; private set; }

    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap plantingTilemap;
    [SerializeField] private TileBase allowedPlantTile;
    public Tilemap PlantingTilemap => plantingTilemap;

    private Dictionary<Vector3Int, Plant> plantedTiles = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanPlantAt(Vector3Int cellPos)
    {
        TileBase currentTile = plantingTilemap.GetTile(cellPos);
        return currentTile == allowedPlantTile && !plantedTiles.ContainsKey(cellPos);
    }

    public bool TryPlant(Vector3Int cellPos, GameObject plantPrefab)
    {
        if (!CanPlantAt(cellPos))
            return false;

        Vector3 worldPos = plantingTilemap.GetCellCenterWorld(cellPos);
        GameObject plantGO = Instantiate(plantPrefab, worldPos, Quaternion.identity);
        Plant plant = plantGO.GetComponent<Plant>();
        plant.tilePosition = cellPos;

        plantedTiles[cellPos] = plant;
        PlantManager.Instance.RegisterPlant(plant);

        return true;
    }

    public void UnregisterPlantAt(Vector3Int cellPos)
    {
        if (plantedTiles.TryGetValue(cellPos, out Plant plant))
        {
            PlantManager.Instance.UnregisterPlant(plant);
            plantedTiles.Remove(cellPos);
        }
    }

    public Plant GetPlantAt(Vector3Int cellPos)
    {
        plantedTiles.TryGetValue(cellPos, out var plant);
        return plant;
    }

    public IEnumerable<Vector3Int> GetAllPlantedTiles() => plantedTiles.Keys;
}