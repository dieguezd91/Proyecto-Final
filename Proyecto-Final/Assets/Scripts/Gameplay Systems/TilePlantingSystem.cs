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

    public bool TryPlant(Vector3Int cellPos, GameObject plantPrefab, out string failureReason)
    {
        failureReason = "";

        TileBase currentTile = plantingTilemap.GetTile(cellPos);

        if (currentTile != allowedPlantTile)
        {
            failureReason = "The soil is not tilled.";
            return false;
        }

        if (plantedTiles.ContainsKey(cellPos))
        {
            failureReason = "There's already a plant here.";
            return false;
        }

        Vector3 worldPos = plantingTilemap.GetCellCenterWorld(cellPos);
        GameObject plantGO = Instantiate(plantPrefab, worldPos, Quaternion.identity);
        Plant plant = plantGO.GetComponent<Plant>();
        plant.tilePosition = cellPos;

        plantedTiles[cellPos] = plant;
        PlantManager.Instance.RegisterPlant(plant);

        InvokePlantEventByType(plant);

        TutorialEvents.InvokeSeedPlanted();

        return true;
    }

    private void InvokePlantEventByType(Plant plant)
    {
        if (plant == null) return;

        PlantType plantType = DeterminePlantType(plant);

        switch (plantType)
        {
            case PlantType.Production:
                TutorialEvents.InvokeProductionPlantPlanted();
                Debug.Log($"[Tutorial] Planta de Producción plantada: {plant.GetType().Name}");
                break;

            case PlantType.Defensive:
                TutorialEvents.InvokeDefensivePlantPlanted();
                Debug.Log($"[Tutorial] Planta Defensiva plantada: {plant.GetType().Name}");
                break;
        }
    }

    private PlantType DeterminePlantType(Plant plant)
    {
        if (plant is ResourcePlant)
        {
            return PlantType.Production;
        }

        if (plant is AttackPlant || plant is DefensePlant)
        {
            return PlantType.Defensive;
        }

        return PlantType.Production;
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

public enum PlantType
{
    Production,
    Defensive,
}