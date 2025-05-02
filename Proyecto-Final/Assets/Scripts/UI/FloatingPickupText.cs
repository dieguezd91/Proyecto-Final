using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class FloatingPickupText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pickupText;
    [SerializeField] private float displayTime = 1.5f;

    private float timer;
    private Dictionary<string, int> pickups = new();

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

        if (pickups.ContainsKey(materialName))
        {
            pickups[materialName] += amount;
        }
        else
        {
            pickups[materialName] = amount;
        }

        timer = displayTime;

        UpdateText();
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                ClearText();
            }
        }
    }

    private void UpdateText()
    {
        System.Text.StringBuilder sb = new();

        foreach (var kvp in pickups)
        {
            sb.AppendLine($"+{kvp.Value} {kvp.Key}");
        }

        pickupText.text = sb.ToString();
        pickupText.alpha = 1f;
    }

    private void ClearText()
    {
        pickupText.text = "";
        pickups.Clear();
    }
}
