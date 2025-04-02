using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    public bool isOccupied = false;
    private GameObject currentPlant;
    [SerializeField] private GameObject harvestIndicator;

    private void Update()
    {
        UpdateHarvestIndicator();
    }

    private void UpdateHarvestIndicator()
    {
        if (harvestIndicator != null)
        {
            bool shouldShowIndicator = false;
            if (currentPlant != null)
            {
                ResourcePlant resourcePlant = currentPlant.GetComponent<ResourcePlant>();
                if (resourcePlant != null && resourcePlant.IsReadyToHarvest())
                {
                    shouldShowIndicator = true;
                }
            }
            harvestIndicator.SetActive(shouldShowIndicator);
        }
    }

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

        Plant plantComponent = currentPlant.GetComponent<Plant>();
        if (plantComponent != null && PlantManager.Instance != null)
        {
            PlantManager.Instance.RegisterPlant(plantComponent);
        }

        Debug.Log("Plant placed successfully");
    }

    public void RemovePlant()
    {
        if (!isOccupied || currentPlant == null)
            return;

        Plant plantComponent = currentPlant.GetComponent<Plant>();
        if (plantComponent != null && PlantManager.Instance != null)
        {
            PlantManager.Instance.UnregisterPlant(plantComponent);
        }

        Destroy(currentPlant);
        isOccupied = false;
        currentPlant = null;

        if (harvestIndicator != null)
        {
            harvestIndicator.SetActive(false);
        }
    }

    public GameObject GetCurrentPlant()
    {
        return currentPlant;
    }

    public T GetPlantComponent<T>() where T : Component
    {
        if (currentPlant != null)
        {
            return currentPlant.GetComponent<T>();
        }
        return null;
    }

    private void OnMouseDown()
    {
        if (currentPlant != null)
        {
            ResourcePlant resourcePlant = currentPlant.GetComponent<ResourcePlant>();
            if (resourcePlant != null && resourcePlant.IsReadyToHarvest())
            {
                resourcePlant.StartHarvest();
                return;
            }
        }

        if (!isOccupied && PlantInventory.Instance != null)
        {
            GameObject selectedPlantPrefab = PlantInventory.Instance.GetSelectedPlantPrefab();
            if (selectedPlantPrefab != null)
            {
                Plant(selectedPlantPrefab);
            }
        }
    }

    public void RegisterPlant(GameObject plant)
    {
        if (plant != null && !isOccupied)
        {
            currentPlant = plant;
            isOccupied = true;

            Plant plantComponent = currentPlant.GetComponent<Plant>();
            if (plantComponent != null && PlantManager.Instance != null)
            {
                PlantManager.Instance.RegisterPlant(plantComponent);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}