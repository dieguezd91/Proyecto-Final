using UnityEngine;
using UnityEngine.UI;

public class PlantGrowthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private Plant plant;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        plant = GetComponentInParent<Plant>();
        UpdateGrowth();
    }

    private void Update()
    {
        UpdateGrowth();
    }

    private void UpdateGrowth()
    {
        if (plant == null || fillImage == null) return;

        float growthPercent = plant.GetGrowthPercentage();
        fillImage.fillAmount = growthPercent;

        fillImage.enabled = growthPercent < 1f;
    }
}
