using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static UpgradeUIArtTests;

// Read-only prefab and measured Play Mode contracts; never authors or generates production assets.
public class UpgradeUICompactLayoutTests
{
    internal const string Output = "Library/MagicGardenCompactUIValidation/";
    private static Rect InSpace(RectTransform child, RectTransform space)
    {
        var points = new Vector3[4]; child.GetWorldCorners(points);
        var local = points.Select(space.InverseTransformPoint).ToArray();
        return Rect.MinMaxRect(local.Min(p => p.x), local.Min(p => p.y), local.Max(p => p.x), local.Max(p => p.y));
    }
    private static void Contains(Rect outer, Rect inner, string name, float padding = 0)
    {
        Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin + padding - .1f), name + " left");
        Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax - padding + .1f), name + " right");
        Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin + padding - .1f), name + " bottom");
        Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax - padding + .1f), name + " top");
    }
    private static void UnitScale(GameObject panel)
    {
        foreach (var t in panel.GetComponentsInChildren<RectTransform>(true))
            Assert.That(t.localScale, Is.EqualTo(Vector3.one), t.name + " must not shrink via localScale");
    }
    private static Vector2 Goal(GameObject panel) => panel.name.StartsWith("LevelUpPanel") ? new Vector2(1100, 680) : new Vector2(1150, 750);
    internal static void AuthoredWindow(GameObject panel)
    {
        UnitScale(panel);
        var root = (RectTransform)panel.transform;
        Assert.That(root.anchorMin, Is.EqualTo(Vector2.zero)); Assert.That(root.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(root.sizeDelta, Is.EqualTo(Vector2.zero));
        var window = (RectTransform)root.Find("Window"); Assert.That(window, Is.Not.Null);
        Assert.That(window.anchorMin, Is.EqualTo(new Vector2(.5f, .5f))); Assert.That(window.anchorMax, Is.EqualTo(window.anchorMin));
        Assert.That(window.pivot, Is.EqualTo(new Vector2(.5f, .5f))); Assert.That(window.anchoredPosition, Is.EqualTo(Vector2.zero));
        Assert.That(window.rect.size.x, Is.EqualTo(Goal(panel).x).Within(.1));
        Assert.That(window.rect.size.y, Is.EqualTo(Goal(panel).y).Within(.1));
        foreach (RectTransform child in window)
            Contains(window.rect, InSpace(child, window), child.name, 20);
        foreach (Component card in panel.GetComponentsInChildren(TypeOf("LevelUpCardView"), true))
        {
            var rect = (RectTransform)card.transform;
            Assert.That(rect.rect.size, Is.EqualTo(new Vector2(300, 420)));
            Contains(window.rect, InSpace(rect, window), "card", 20);
            RowOrCard(card.gameObject);
        }
        var cards = panel.GetComponentsInChildren(TypeOf("LevelUpCardView"), true).Cast<Component>().ToArray();
        for (int i = 1; i < cards.Length; i++)
        {
            var left = InSpace((RectTransform)cards[i - 1].transform, window);
            var right = InSpace((RectTransform)cards[i].transform, window);
            Assert.That(right.xMin - left.xMax, Is.EqualTo(20).Within(.1), "Compact horizontal gap");
        }
        var viewport = window.Find("Specials scroll/Viewport") as RectTransform;
        if (viewport != null)
        {
            Assert.That(viewport.rect.size, Is.EqualTo(new Vector2(950, 540)));
            Contains(window.rect, InSpace(viewport, window), "viewport", 20);
        }
        foreach (Component row in panel.GetComponentsInChildren(TypeOf("SpecialUpgradeItemView"), true)) RowOrCard(row.gameObject);
        foreach (var button in Components(panel, "UnityEngine.UI.Button"))
        {
            var graphic = (Component)Property(button, "targetGraphic");
            Assert.That(((RectTransform)graphic.transform).rect.size, Is.EqualTo(new Vector2(164.5f, 31.5f)));
        }
    }
    private static void RowOrCard(GameObject root)
    {
        UnitScale(root);
        var rect = (RectTransform)root.transform;
        bool card = root.name.StartsWith("LevelUpCard");
        Assert.That(rect.rect.width, Is.EqualTo(card ? 300 : 950).Within(.1));
        Assert.That(rect.rect.height, Is.EqualTo(card ? 420 : 200).Within(.1));
        var visual = root.transform.Find("Action visual") as RectTransform;
        Assert.That(visual, Is.Not.Null); Contains(rect.rect, InSpace(visual, rect), "action", 12);
        foreach (RectTransform child in rect) Contains(rect.rect, InSpace(child, rect), child.name, card ? 12 : 8);
        var labels = Components(root, "TMPro.TextMeshProUGUI").Where(c => c.transform.parent == rect).ToArray();
        var icon = root.transform.Find("Target icon") as RectTransform;
        foreach (var label in labels)
        {
            var bounds = InSpace((RectTransform)label.transform, rect);
            Assert.That(bounds.Overlaps(InSpace(visual, rect)), Is.False, label.name + " / action overlap");
            Assert.That(bounds.Overlaps(InSpace(icon, rect)), Is.False, label.name + " / icon overlap");
        }
        for (int i = 0; i < labels.Length; i++)
            for (int j = i + 1; j < labels.Length; j++)
                Assert.That(InSpace((RectTransform)labels[i].transform, rect).Overlaps(InSpace((RectTransform)labels[j].transform, rect)), Is.False, labels[i].name + " / " + labels[j].name);
    }
    internal static void MeasuredCapture(Canvas canvas, GameObject panel, string evidence, int width, int height)
    {
        AuthoredWindow(panel);
        var window = (RectTransform)panel.transform.Find("Window");
        var corners = new Vector3[4]; window.GetWorldCorners(corners);
        var screen = corners.Select(p => canvas.worldCamera.WorldToScreenPoint(p)).ToArray();
        var frame = Rect.MinMaxRect(screen.Min(p => p.x), screen.Min(p => p.y), screen.Max(p => p.x), screen.Max(p => p.y));
        float scale = Mathf.Sqrt(width / 1920f * height / 1080f);
        Assert.That(frame.center.x, Is.EqualTo(width / 2f).Within(1)); Assert.That(frame.center.y, Is.EqualTo(height / 2f).Within(1));
        Assert.That(frame.width, Is.EqualTo(Goal(panel).x * scale).Within(1)); Assert.That(frame.height, Is.EqualTo(Goal(panel).y * scale).Within(1));
        Contains(new Rect(0, 0, width, height), frame, "rendered frame", 40);
        Assert.That(frame.width / width, Is.LessThan(.75)); Assert.That(frame.height / height, Is.LessThan(.75));
        // The root dimmer intentionally covers the full target, including Game Canvas nested overrides.
        ((RectTransform)panel.transform).GetWorldCorners(corners);
        var dimmer = corners.Select(p => canvas.worldCamera.WorldToScreenPoint(p)).ToArray();
        Assert.That(dimmer.Max(p => p.x) - dimmer.Min(p => p.x), Is.EqualTo(width).Within(1));
        Assert.That(dimmer.Max(p => p.y) - dimmer.Min(p => p.y), Is.EqualTo(height).Within(1));
        var measurements = new List<string> {
            "capture=" + width + "x" + height + " scale=" + scale + " simulated CanvasScaler match=.5 capture only",
            "screen frame=" + frame + " authored=" + window.rect.size
        };
        foreach (string type in new[] { "LevelUpCardView", "SpecialUpgradeItemView", "UnityEngine.UI.Button" })
        {
            var components = type.StartsWith("UnityEngine") ? Components(panel, type) : panel.GetComponentsInChildren(TypeOf(type), false).Cast<Component>().ToArray();
            foreach (var component in components)
            {
                var measured = type.EndsWith("Button") ? ((Component)Property(component, "targetGraphic")).transform : component.transform;
                var rect = (RectTransform)measured;
                measurements.Add(type + " " + measured.name + " actual local=" + rect.rect.size + " inWindow=" + InSpace(rect, window));
            }
        }
        var viewport = window.Find("Specials scroll/Viewport") as RectTransform;
        if (viewport != null) measurements.Add("viewport actual=" + viewport.rect.size + " inWindow=" + InSpace(viewport, window));
        File.WriteAllLines(Output + evidence + "-bounds.txt", measurements);
    }
    [TestCase("LevelUpPanel")]
    [TestCase("SpecialUpgradePanel")]
    public void SourceAndProductionCanvasHaveCompactCenteredWindows(string name)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
        var canvas = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Canvas/Canvas Game UI.prefab");
        // Imported nested prefab transforms retain stale local positions until Unity resolves their anchors.
        // Clone both the source and the inherited production instance without mutating either asset.
        foreach (var asset in new[] { source, canvas.transform.Find(name).gameObject })
        {
            var host = new GameObject("Compact layout fixture", typeof(RectTransform), typeof(Canvas));
            var clone = UnityEngine.Object.Instantiate(asset, host.transform, false);
            try
            {
                host.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                clone.SetActive(true); Canvas.ForceUpdateCanvases();
                AuthoredWindow(clone);
                AssertFrame(clone, clone.GetComponent(TypeOf(name + "View")));
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }
    }
    [TestCase("LevelUpCard", 1148178998566213947L, 8156434278158443870L, 9110852116597401L, 4846185045621668382L)]
    [TestCase("SpecialUpgradeItem", 6332052736431292355L, 9210391740693434405L, 8548105710899072688L, 4566027624550477145L)]
    public void SourceDimensionsAndOriginalActionBindingsSurvive(string name, long rootId, long viewId, long buttonId, long actionId)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
        var clone = UnityEngine.Object.Instantiate(root);
        try { RowOrCard(clone); }
        finally { UnityEngine.Object.DestroyImmediate(clone); }
        var view = root.GetComponent(TypeOf(name + "View"));
        var button = (Component)Field(view, name == "LevelUpCard" ? "selectButton" : "purchaseButton");
        var action = (Component)Property(button, "targetGraphic");
        foreach (var pair in new[] { Tuple.Create((UnityEngine.Object)root, rootId), Tuple.Create((UnityEngine.Object)view, viewId), Tuple.Create((UnityEngine.Object)button, buttonId), Tuple.Create((UnityEngine.Object)action, actionId) })
        {
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(pair.Item1, out string guid, out long id), Is.True);
            Assert.That(id, Is.EqualTo(pair.Item2)); Assert.That(guid, Is.EqualTo(AssetDatabase.AssetPathToGUID(Folder + name + ".prefab")));
        }
        Assert.That(action.transform.parent, Is.SameAs(root.transform)); Assert.That(button.gameObject, Is.SameAs(root));
        Assert.That(new SerializedObject(button).FindProperty("m_OnClick.m_PersistentCalls.m_Calls").arraySize, Is.Zero);
        AssertFrame(root, view);
    }
}
