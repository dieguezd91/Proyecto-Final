using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextController : MonoBehaviour
{
    [Header("Shared UI Panel")]
    [SerializeField] private Transform pickupPanel;
    [SerializeField] private GameObject pickupEntryPrefab;
    [SerializeField] private TMP_SpriteAsset itemSpriteAsset;
    [SerializeField] private float pickupDisplayTime = 1.5f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Color warningColor;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("Sprite Name Mapping")]
    [SerializeField] private List<SpriteMapping> spriteMappings = new List<SpriteMapping>();

    [Header("Gold Feedback")]
    [SerializeField] private Transform goldRewardPanel;
    [SerializeField] private GameObject goldEntryPrefab;
    [SerializeField] private float goldFeedbackDisplayTime = 6f;
    [SerializeField] private Color goldRewardColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color goldTotalColor = new Color(1f, 0.9f, 0.3f);

    public static FloatingTextController Instance { get; private set; }

    private float panelTimer;
    private Dictionary<string, (int amount, GameObject entry)> pickups = new();
    private Dictionary<Transform, FloatingDamageText> damageTexts = new();
    private CanvasGroup pickupPanelCanvasGroup;
    private Dictionary<string, string> spriteNameMap;

    private List<GameObject> goldFeedbackEntries = new List<GameObject>();
    private bool isShowingGoldFeedback = false;

    [Serializable]
    public class SpriteMapping
    {
        public string originalSpriteName;
        public string tmpSpriteName;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pickupPanel != null)
            pickupPanelCanvasGroup = pickupPanel.GetComponent<CanvasGroup>();

        SetPanelAlpha(pickupPanelCanvasGroup, 0f);

        InitializeSpriteMapping();
    }

    private void InitializeSpriteMapping()
    {
        spriteNameMap = new Dictionary<string, string>();

        foreach (var mapping in spriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.originalSpriteName) &&
                !string.IsNullOrEmpty(mapping.tmpSpriteName))
            {
                spriteNameMap[mapping.originalSpriteName] = mapping.tmpSpriteName;
            }
        }
    }

    void Update()
    {
        if (panelTimer > 0f)
        {
            panelTimer -= Time.deltaTime;
            if (panelTimer <= 0f)
            {
                if (isShowingGoldFeedback)
                {
                    StartCoroutine(FadeOutPanel(pickupPanelCanvasGroup, ClearGoldFeedback));
                }
                else
                {
                    StartCoroutine(FadeOutPanel(pickupPanelCanvasGroup, ClearPickups));
                }
            }
        }
    }

    public void ShowPickup(string materialName, int amount, Sprite icon)
    {
        if (isShowingGoldFeedback) return;
        if (pickupPanel == null || pickupEntryPrefab == null) return;

        StartCoroutine(FadeInPanel(pickupPanelCanvasGroup));
        panelTimer = pickupDisplayTime;

        string tmpSpriteName = GetTMPSpriteName(icon);
        string spriteTag = !string.IsNullOrEmpty(tmpSpriteName) ? $"<sprite name=\"{tmpSpriteName}\"> " : "";

        if (pickups.TryGetValue(materialName, out var data))
        {
            int newAmt = data.amount + amount;
            pickups[materialName] = (newAmt, data.entry);

            var txt = data.entry.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = $"{spriteTag}+{newAmt} {materialName}";
            }
        }
        else
        {
            var entry = Instantiate(pickupEntryPrefab, pickupPanel);
            var txt = entry.GetComponent<TextMeshProUGUI>();

            if (txt != null)
            {
                txt.color = warningColor;
                txt.text = $"{spriteTag}+{amount} {materialName}";

                if (txt.spriteAsset == null && itemSpriteAsset != null)
                {
                    txt.spriteAsset = itemSpriteAsset;
                }
            }

            pickups.Add(materialName, (amount, entry));
        }
    }

    private string GetTMPSpriteName(Sprite originalSprite)
    {
        if (originalSprite == null || itemSpriteAsset == null)
            return null;

        if (spriteNameMap.TryGetValue(originalSprite.name, out string tmpName))
        {
            return tmpName;
        }

        for (int i = 0; i < itemSpriteAsset.spriteInfoList.Count; i++)
        {
            if (itemSpriteAsset.spriteInfoList[i].name == originalSprite.name)
            {
                return originalSprite.name;
            }
        }

        return null;
    }

    [Serializable]
    public struct GoldReward
    {
        public int goldAmount;
        public string message;
        public Color textColor;
    }

    public void ShowGoldFeedbackSequence(List<GoldReward> rewards, int totalGold)
    {
        if (goldRewardPanel == null || goldEntryPrefab == null)
        {
            return;
        }

        ClearPickups();

        isShowingGoldFeedback = true;
        StartCoroutine(FadeInPanel(pickupPanelCanvasGroup));
        panelTimer = goldFeedbackDisplayTime;

        if (rewards.Count == 0)
        {
            ShowGoldFeedbackEntry("The shadows watch in silence...", 0, new Color(0.6f, 0.2f, 0.2f));
        }
        else
        {
            foreach (var reward in rewards)
            {
                ShowGoldFeedbackEntry(reward.message, reward.goldAmount, reward.textColor);
            }
        }
    }

    private void ShowGoldFeedbackEntry(string message, int goldAmount, Color textColor)
    {
        var entry = Instantiate(goldEntryPrefab, goldRewardPanel);
        var txt = entry.GetComponentInChildren<TextMeshProUGUI>();
        var img = entry.GetComponentInChildren<Image>();

        if (txt != null)
        {
            string displayText = goldAmount > 0
                ? $"{message}\n+{goldAmount} gold"
                : message;
            txt.text = displayText;
            txt.color = textColor;

            if (goldAmount > 0)
            {
                txt.fontSize *= 1.1f;
            }
        }

        if (img != null)
        {
            if (goldAmount > 0)
            {
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.5f, 0.1f, 0.1f);
            }
        }

        goldFeedbackEntries.Add(entry);
    }

    private void ClearGoldFeedback()
    {
        foreach (var entry in goldFeedbackEntries)
        {
            if (entry != null) Destroy(entry);
        }
        goldFeedbackEntries.Clear();
        isShowingGoldFeedback = false;
        SetPanelAlpha(pickupPanelCanvasGroup, 0f);
    }

    private void ClearPickups()
    {
        foreach (Transform t in pickupPanel)
        {
            if (t.gameObject != null) Destroy(t.gameObject);
        }
        pickups.Clear();

        if (!isShowingGoldFeedback)
        {
            SetPanelAlpha(pickupPanelCanvasGroup, 0f);
        }
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