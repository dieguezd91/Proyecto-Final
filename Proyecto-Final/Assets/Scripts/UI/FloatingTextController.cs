using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private Transform floatingPanel;
    [SerializeField] private GameObject pickupEntryPrefab;
    [SerializeField] private float displayTime = 1.5f;
    [SerializeField] private Color warningColor;
    [SerializeField] private Color defaultColor = Color.white;

    [SerializeField] private ProgressBar progressBar;

    private float timer;
    private Dictionary<string, (int, GameObject)> pickups = new();

    void Start()
    {
        ClearText();
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

    public void ShowPickup(string materialName, int amount, Sprite icon)
    {
        if (floatingPanel == null || pickupEntryPrefab == null) return;

        timer = displayTime;

        if (pickups.TryGetValue(materialName, out var entryData))
        {
            int newAmount = entryData.Item1 + amount;
            pickups[materialName] = (newAmount, entryData.Item2);

            var text = entryData.Item2.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"+{newAmount} {materialName}";
        }
        else
        {
            GameObject entry = Instantiate(pickupEntryPrefab, floatingPanel);
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            var image = entry.GetComponentInChildren<Image>();

            if (text != null) text.text = $"+{amount} {materialName}";
            if (image != null) image.sprite = icon;

            text.color = defaultColor;

            pickups.Add(materialName, (amount, entry));
        }

    }

    public void ShowWarning(string message)
    {
        ClearText();

        GameObject entry = Instantiate(pickupEntryPrefab, floatingPanel);
        var text = entry.GetComponentInChildren<TextMeshProUGUI>();
        var image = entry.GetComponentInChildren<Image>();

        if (text != null) text.text = message;
        if (image != null) image.enabled = false;

        text.color = warningColor;

        timer = displayTime;
    }

    private void ClearText()
    {
        foreach (Transform child in floatingPanel)
        {
            Destroy(child.gameObject);
        }
        pickups.Clear();
    }
}
