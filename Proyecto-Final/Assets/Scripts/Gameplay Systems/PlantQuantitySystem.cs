using UnityEngine;

public class PlantQuantitySystem : MonoBehaviour
{
    public static PlantQuantitySystem Instance { get; private set; }

    [Header("PLANT QUANTITY")]
    [SerializeField] private int startingQuantity = 5;

    private int currentQuantity;

    public int CurrentQuantity => currentQuantity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentQuantity = startingQuantity;
    }

    public int GetCurrentPlantCount()
    {
        Plant[] plants = FindObjectsOfType<Plant>();

        return plants.Length;
    }

    public bool CanPlant()
    {
        return GetCurrentPlantCount() < currentQuantity;
    }

    public int GetRemainingQuantity()
    {
        int currentPlantCount = GetCurrentPlantCount();

        return Mathf.Max(0, currentQuantity - currentPlantCount);
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            return;

        currentQuantity += amount;

        Debug.Log(
            $"[PlantQuantitySystem] Quantity aumentada a {currentQuantity}"
        );
    }

    public void SetQuantity(int quantity)
    {
        currentQuantity = Mathf.Max(0, quantity);
    }
}