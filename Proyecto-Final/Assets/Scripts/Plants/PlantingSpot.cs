using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentPlant;

    public void Plant(GameObject plantPrefab)
    {
        if (isOccupied)
        {
            Debug.Log("This spot is already occupied");
            return;
        }

        if (GameManager.Instance.currentGameState != GameState.Day)
        {
            Debug.Log("You can only plant during the day");
            return;
        }

        currentPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        isOccupied = true;

        Debug.Log("Plant placed successfully");
    }

    public void RemovePlant()
    {
        if (!isOccupied || currentPlant == null)
            return;

        Destroy(currentPlant);
        isOccupied = false;
        currentPlant = null;
    }

    public GameObject GetCurrentPlant()
    {
        return currentPlant;
    }
}