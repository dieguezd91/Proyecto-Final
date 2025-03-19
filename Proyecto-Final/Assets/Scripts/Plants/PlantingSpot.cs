using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    public bool isOccupied = false; // Indica si ya hay una planta aquí
    private GameObject currentPlant; // La planta actual en esta parcela

    [SerializeField] private WaveSpawner gameStateController;

    private void Start()
    {
        if (gameStateController == null)
        {
            gameStateController = FindObjectOfType<WaveSpawner>();
        }
    }

    public void Plant(GameObject plantPrefab)
    {
        if (isOccupied)
        {
            Debug.Log("Esta parcela ya esta ocupada");
            return;
        }

        if (gameStateController != null && GameManager.Instance.currentGameState != GameState.Day)
        {
            Debug.Log("Solo puedes plantar durante el dia");
            return;
        }

        currentPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        isOccupied = true;
        Debug.Log("Planta sembrada");
    }
}