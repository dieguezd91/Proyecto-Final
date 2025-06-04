using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private float displayTime = 1.5f;

    private float timer;
    private Dictionary<string, int> pickups = new();
    [SerializeField] private Color warningColor;
    [SerializeField] private Color defaultColor = Color.white;


    [SerializeField] private ProgressBar progressBar;

    void Start()
    {
        if (floatingText != null)
        {
            floatingText.text = "";
        }
    }

    void Update()
    {
        if (progressBar != null && progressBar.IsShowing())
        {
            ClearText();
            return;
        }

        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                ClearText();
            }
        }
    }

    public void ShowPickup(string materialName, int amount)
    {
        if (floatingText == null) return;

        if (pickups.ContainsKey(materialName))
        {
            pickups[materialName] += amount;
        }
        else
        {
            pickups[materialName] = amount;
        }

        timer = displayTime;

        floatingText.color = defaultColor;

        UpdateText();
    }

    private void UpdateText()
    {
        System.Text.StringBuilder sb = new();

        foreach (var kvp in pickups)
        {
            sb.AppendLine($"+{kvp.Value} {kvp.Key}");
        }

        floatingText.text = sb.ToString();
        floatingText.alpha = 1f;
    }

    private void ClearText()
    {
        floatingText.text = "";
        pickups.Clear();
    }

    public void ShowWarning(string message)
    {
        if (floatingText == null) return;

        floatingText.color = warningColor;
        floatingText.text = message;
        floatingText.alpha = 1f;
        timer = displayTime;
    }
}
