using System.Collections.Generic;
using MagicGarden;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Deliberately default UGUI plus existing target icons; no new textures, fonts or paid UI assets.
public sealed class UpgradePanel : MonoBehaviour
{
    private UpgradeRuntime runtime;
    private RectTransform content;
    private Font font;
    private readonly List<GameObject> generated = new List<GameObject>();
    public bool IsShopOpen { get; private set; }
    private bool shopRefresh;
    public void RequestShopRefresh() { if (IsShopOpen) shopRefresh = true; }
    public static UpgradePanel Create(UpgradeRuntime owner)
    {
        var go = new GameObject("Magic Garden upgrades", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20000;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        var panel = go.AddComponent<UpgradePanel>();
        panel.runtime = owner;
        panel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        panel.content = panel.Rect("Backdrop", go.transform, Vector2.zero, Vector2.one);
        panel.content.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.025f, 0.055f, 0.96f);
        if (EventSystem.current == null) new GameObject("Upgrade EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        panel.Hide();
        return panel;
    }
    private RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return rect;
    }
    private Text Label(Transform parent, string text, Vector2 min, Vector2 max, int size, Color color)
    {
        var rect = Rect("Label", parent, min, max);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = font; label.text = text; label.fontSize = size;
        label.alignment = TextAnchor.MiddleCenter; label.color = color;
        label.raycastTarget = false;
        return label;
    }
    private void Clear()
    {
        foreach (var go in generated) { go.SetActive(false); Destroy(go); }
        generated.Clear();
    }
    private RectTransform Element(string name, Vector2 min, Vector2 max)
    {
        var rect = Rect(name, content, min, max);
        generated.Add(rect.gameObject);
        return rect;
    }
    private Button Button(RectTransform rect, UnityEngine.Events.UnityAction clicked, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>(); image.color = color;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image; button.onClick.AddListener(clicked);
        return button;
    }
    private static Color ColorFor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary: return new Color(1, 0.76f, 0.25f);
            case Rarity.Rare: return new Color(0.72f, 0.5f, 1);
            case Rarity.Common: return new Color(0.4f, 0.8f, 1);
            default: return new Color(0.65f, 0.85f, 0.65f);
        }
    }
    private void Icon(Transform parent, Sprite sprite)
    {
        var rect = Rect("Existing target icon", parent, new Vector2(0.32f, 0.65f), new Vector2(0.68f, 0.84f));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite; image.preserveAspect = true;
        image.raycastTarget = false; image.enabled = sprite != null;
    }
    public void ShowChoices(IList<Offer> offers, int pending)
    {
        IsShopOpen = false; Clear(); gameObject.SetActive(true);
        var heading = Element("Level up", new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.97f));
        Label(heading, $"LEVEL UP — Choose a blessing ({pending} pending)", Vector2.zero, Vector2.one, 30, Color.white);
        if (offers.Count == 0)
        {
            var empty = Element("No eligible upgrades", new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.65f));
            Button(empty, () => runtime.Choose(-1), new Color(0.12f, 0.16f, 0.2f));
            Label(empty, "All owned upgrades are exhausted.\nContinue", Vector2.zero, Vector2.one, 26, Color.white);
            EventSystem.current?.SetSelectedGameObject(empty.gameObject);
            return;
        }
        for (int i = 0; i < offers.Count; i++)
        {
            int index = i;
            var offer = offers[i];
            float left = 0.07f + i * 0.30f;
            var card = Element("Upgrade card " + i, new Vector2(left, 0.17f), new Vector2(left + 0.26f, 0.80f));
            Button(card, () => runtime.Choose(index), new Color(0.09f, 0.105f, 0.16f));
            var tint = ColorFor(offer.Rarity.rarity);
            Label(card, offer.Rarity.rarity.ToString().ToUpperInvariant(), new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.98f), 21, tint);
            Icon(card, runtime.Icon(offer.Target.Id));
            Label(card, offer.Target.Name + "\n" + offer.Rule.displayName, new Vector2(0.04f, 0.51f), new Vector2(0.96f, 0.66f), 21, Color.white);
            string bonus = offer.Rule.stat == Stat.Quantity ? "+1" : $"+{offer.Rarity.bonus * offer.Target.Ratio(offer.Rule.stat) * 100:0.##}% base";
            Label(card, $"{offer.Rule.stat}\n{bonus}", new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.51f), 25, tint);
            string unit = offer.Rule.stat == Stat.AttackSpeed ? " attacks/s" : "";
            Label(card, $"{offer.Before:0.###} → {offer.After:0.###}{unit}", new Vector2(0.02f, 0.1f), new Vector2(0.98f, 0.26f), 24, Color.white);
            if (i == 0) EventSystem.current?.SetSelectedGameObject(card.gameObject);
        }
        var footer = Element("Preview help", new Vector2(0.07f, 0.03f), new Vector2(0.93f, 0.14f));
        Label(footer, "Damage: base hit • Area: radius • Plant range: detection/attachment\nApplies to this owned line, including future instances.", Vector2.zero, Vector2.one, 18, Color.gray);
    }
    public void ShowShop()
    {
        shopRefresh = false;
        IsShopOpen = true; Clear(); gameObject.SetActive(true);
        var heading = Element("Cauldron", new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f));
        Label(heading, "CAULDRON — Special upgrades (materials, not level-up choices)", Vector2.zero, Vector2.one, 26, Color.white);
        var close = Element("Close", new Vector2(0.8f, 0.02f), new Vector2(0.94f, 0.09f));
        Button(close, CloseShop, new Color(0.15f, 0.2f, 0.24f));
        Label(close, "Close", Vector2.zero, Vector2.one, 22, Color.white);
        var scrollRect = Element("Specials scroll", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.86f));
        var scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
        var viewport = Rect("Viewport", scrollRect, Vector2.zero, Vector2.one);
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        var list = Rect("Specials", viewport, new Vector2(0, 1), Vector2.one);
        list.pivot = new Vector2(0.5f, 1);
        scroll.viewport = viewport; scroll.content = list; scroll.horizontal = false;
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12; layout.childControlHeight = true; layout.childForceExpandHeight = false;
        list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        int count = 0;
        foreach (var target in runtime.Targets)
            foreach (var definition in runtime.Balance.specials)
            {
                if (definition.capability != target.Capability) continue;
                var row = Rect(definition.id, list, Vector2.zero, Vector2.one);
                var element = row.gameObject.AddComponent<LayoutElement>(); element.preferredHeight = 150;
                string targetId = target.Id;
                var button = Button(row, () => { runtime.Purchase(definition, targetId); runtime.RefreshTargets(); ShowShop(); }, new Color(0.10f, 0.13f, 0.19f));
                button.interactable = runtime.CanPurchase(definition, target);
                string costs = "";
                foreach (var cost in definition.costs)
                {
                    var inventory = InventoryManager.Instance;
                    string name = inventory != null ? inventory.GetMaterialName((MaterialType)cost.material) : ((MaterialType)cost.material).ToString();
                    int owned = inventory != null ? inventory.GetMaterialAmount((MaterialType)cost.material) : 0;
                    costs += $"{name} {owned}/{cost.amount}  ";
                }
                string prerequisites = definition.prerequisites != null && definition.prerequisites.Length > 0
                    ? "Requires: " + string.Join(", ", definition.prerequisites) : "";
                Label(row, $"{target.Name} — {definition.displayName} ({runtime.State.SpecialStacks(definition.id, target.Id)}/{definition.maxStacks})\n{definition.description}\n{costs}\n{prerequisites}",
                    new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), 20, button.interactable ? Color.white : Color.gray);
                count++;
            }
        if (count == 0)
        {
            var empty = Element("No owned specials", new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.65f));
            Label(empty, "No owned compatible targets yet.", Vector2.zero, Vector2.one, 22, Color.white);
        }
        EventSystem.current?.SetSelectedGameObject(close.gameObject);
    }
    public void CloseShop()
    {
        if (!IsShopOpen) return;
        IsShopOpen = false;
        UIManager.Instance?.Flow?.Close(UIModal.Crafting);
        Hide();
        UIEvents.TriggerCraftingUIClosed();
        TutorialEvents.InvokeCraftingClosed();
    }
    public void Hide() { gameObject.SetActive(false); }
    private void OnDisable()
    {
        runtime?.PanelDisabled();
        if (IsShopOpen) CloseShop();
    }
    private void Update()
    {
        if (IsShopOpen && shopRefresh) { runtime.RefreshTargets(); ShowShop(); }
        if (IsShopOpen && Input.GetKeyDown(KeyCode.Escape) && !InputConsumptionManager.IsEscapeConsumed)
        {
            InputConsumptionManager.ConsumeEscape(); CloseShop();
        }
    }
}
