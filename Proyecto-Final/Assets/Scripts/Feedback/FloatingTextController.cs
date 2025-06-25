using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextController : MonoBehaviour
{
    [Header("Pickups UI")]
    [SerializeField] private Transform floatingPanel;
    [SerializeField] private GameObject pickupEntryPrefab;
    [SerializeField] private float pickupDisplayTime = 1.5f;
    [SerializeField] private Color warningColor;
    [SerializeField] private Color defaultColor = Color.white;


    [Header("Damage Text")]
    [SerializeField] private GameObject floatingDamagePrefab;

    [SerializeField] private Image panelBackground;
    [SerializeField] private Vector2 backgroundPadding = new Vector2(16f, 8f);

    private float pickupTimer;
    private Dictionary<string, (int amount, GameObject entry)> pickups = new();
    private Dictionary<Transform, FloatingDamageText> damageTexts = new();

    void Start()
    {
        if (panelBackground != null)
            panelBackground.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pickupTimer > 0f)
        {
            pickupTimer -= Time.deltaTime;
            if (pickupTimer <= 0f)
                ClearPickups();
        }
    }

    public void ShowPickup(string materialName, int amount, Sprite icon)
    {
        if (floatingPanel == null || pickupEntryPrefab == null) return;

        pickupTimer = pickupDisplayTime;

        if (pickups.TryGetValue(materialName, out var data))
        {
            int newAmt = data.amount + amount;
            pickups[materialName] = (newAmt, data.entry);
            var txt = data.entry.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = $"+{newAmt} {materialName}";
        }
        else
        {
            var entry = Instantiate(pickupEntryPrefab, floatingPanel);
            var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
            var img = entry.GetComponentInChildren<Image>();
            if (txt != null) { txt.text = $"+{amount} {materialName}"; txt.color = defaultColor; }
            if (img != null) img.sprite = icon;
            pickups.Add(materialName, (amount, entry));
        }
    }


    public void ShowWarning(string message)
    {

        if (floatingPanel == null || pickupEntryPrefab == null) return;

        


        ClearPickups();

        if (panelBackground != null)
        {
            panelBackground.gameObject.SetActive(true);
            panelBackground.transform.SetAsFirstSibling();
        }


        var entry = Instantiate(pickupEntryPrefab, floatingPanel);
        var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
        var img = entry.GetComponentInChildren<Image>();
        if (txt != null) { txt.text = message; txt.color = warningColor; }
        if (img != null) img.enabled = false;
        
        AdjustBackgroundToContent();
        pickupTimer = pickupDisplayTime;

    }

    private void ClearPickups()
    {
        foreach (Transform t in floatingPanel) Destroy(t.gameObject);
        pickups.Clear();

        if (panelBackground != null)
            panelBackground.gameObject.SetActive(false);
    }

    private void AdjustBackgroundToContent()
    {
        if (panelBackground == null || floatingPanel == null) return;

        var panelRT = floatingPanel as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRT);

        float w = LayoutUtility.GetPreferredWidth(panelRT);
        float h = LayoutUtility.GetPreferredHeight(panelRT);

        var padded = new Vector2(
            w + backgroundPadding.x,
            h + backgroundPadding.y
        );

        var bgRT = panelBackground.rectTransform;
        bgRT.sizeDelta = padded;

        bgRT.pivot = panelRT.pivot;
        bgRT.anchorMin = panelRT.anchorMin;
        bgRT.anchorMax = panelRT.anchorMax;
        bgRT.anchoredPosition = panelRT.anchoredPosition;

        panelBackground.transform.SetAsFirstSibling();
        panelBackground.gameObject.SetActive(true);
    }


    public void ShowDamage(float dmg, Transform target)
    {
        if (floatingDamagePrefab == null || target == null) return;

        if (damageTexts.TryGetValue(target, out var existing))
        {
            if (existing != null)
            {
                existing.AddDamage(dmg);
                return;
            }
            else
            {
                damageTexts.Remove(target);
            }
        }

        var go = Instantiate(floatingDamagePrefab, target.position, Quaternion.identity);
        var fdt = go.GetComponent<FloatingDamageText>();
        fdt.Initialize(target);
        fdt.AddDamage(dmg);
        damageTexts[target] = fdt;
    }



}
