using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextController : MonoBehaviour
{
    [Header("Pickups UI")]
    [SerializeField] private Transform pickupPanel;
    [SerializeField] private Transform warningPanel;
    [SerializeField] private GameObject pickupEntryPrefab;
    [SerializeField] private GameObject warningEntryPrefab;
    [SerializeField] private float pickupDisplayTime = 1.5f;
    [SerializeField] private float fadeDuration = 0.25f; // <- Duración del fade
    [SerializeField] private Color warningColor;
    [SerializeField] private Color defaultColor = Color.white;

    private float pickupTimer;
    private float warningTimer;

    private Dictionary<string, (int amount, GameObject entry)> pickups = new();
    private Dictionary<Transform, FloatingDamageText> damageTexts = new();

    private CanvasGroup pickupPanelCanvasGroup;
    private CanvasGroup warningPanelCanvasGroup;

    void Awake()
    {
        if (pickupPanel != null)
            pickupPanelCanvasGroup = pickupPanel.GetComponent<CanvasGroup>();
        if (warningPanel != null)
            warningPanelCanvasGroup = warningPanel.GetComponent<CanvasGroup>();

        SetPanelAlpha(pickupPanelCanvasGroup, 0f);
        SetPanelAlpha(warningPanelCanvasGroup, 0f);
    }

    void Update()
    {
        if (pickupTimer > 0f)
        {
            pickupTimer -= Time.deltaTime;
            if (pickupTimer <= 0f)
                StartCoroutine(FadeOutPanel(pickupPanelCanvasGroup, ClearPickups));
        }
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
                StartCoroutine(FadeOutPanel(warningPanelCanvasGroup, ClearWarnings));
        }
    }

    public void ShowPickup(string materialName, int amount, Sprite icon)
    {
        if (pickupPanel == null || pickupEntryPrefab == null) return;
        StartCoroutine(FadeInPanel(pickupPanelCanvasGroup));
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
            var entry = Instantiate(pickupEntryPrefab, pickupPanel);
            var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
            var img = entry.GetComponentInChildren<Image>();
            if (txt != null) { txt.text = $"+{amount} {materialName}"; txt.color = warningColor; }
            if (img != null) img.sprite = icon;
            pickups.Add(materialName, (amount, entry));
        }
    }

    public void ShowWarning(string message)
    {
        if (warningPanel == null || warningEntryPrefab == null) return;
        ClearWarnings();
        StartCoroutine(FadeInPanel(warningPanelCanvasGroup));
        var entry = Instantiate(warningEntryPrefab, warningPanel);
        var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
        var img = entry.GetComponentInChildren<Image>();
        if (txt != null) { txt.text = message; txt.color = warningColor; }
        if (img != null) img.enabled = false;
        warningTimer = pickupDisplayTime;
    }

    private void ClearPickups()
    {
        foreach (Transform t in pickupPanel) Destroy(t.gameObject);
        pickups.Clear();
        SetPanelAlpha(pickupPanelCanvasGroup, 0f);
    }

    private void ClearWarnings()
    {
        foreach (Transform t in warningPanel) Destroy(t.gameObject);
        SetPanelAlpha(warningPanelCanvasGroup, 0f);
    }

    private void SetPanelAlpha(CanvasGroup cg, float alpha)
    {
        if (cg != null)
            cg.alpha = alpha;
    }

    private IEnumerator FadeInPanel(CanvasGroup cg)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(cg.alpha, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOutPanel(CanvasGroup cg, System.Action onComplete)
    {
        if (cg == null) yield break;
        float startAlpha = cg.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        onComplete?.Invoke();
    }
}
