using TMPro;
using UnityEngine;

public class FloatingPickupText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pickupText;
    [SerializeField] private float displayTime = 1.5f;

    private float timer;
    private string currentMaterialName = "";
    private int currentAmount = 0;

    void Start()
    {
        if (pickupText != null)
        {
            pickupText.text = "";
        }
    }

    public void ShowPickup(string materialName, int amount)
    {
        if (pickupText == null) return;

        if (materialName == currentMaterialName)
        {
            currentAmount += amount;
        }
        else
        {
            currentMaterialName = materialName;
            currentAmount = amount;
        }

        pickupText.text = $"+{currentAmount} {currentMaterialName}";
        pickupText.alpha = 1f;
        timer = displayTime;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                pickupText.text = "";
                currentMaterialName = "";
                currentAmount = 0;
            }
        }
    }
}