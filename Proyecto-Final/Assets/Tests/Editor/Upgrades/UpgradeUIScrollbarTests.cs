using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static UpgradeUIArtTests;

// Reflection retains the existing test assembly boundary; production assets are read-only.
public class UpgradeUIScrollbarTests
{
    internal static void Set(object target, string name, object value) => target.GetType().GetProperty(name).SetValue(target, value);
    internal static void SetField(object target, string name, object value) => target.GetType().GetField(name).SetValue(target, value);
    internal static IList Items(int count)
    {
        var type = TypeOf("SpecialUpgradeItemData");
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
        for (int i = 0; i < count; i++)
        {
            var data = Activator.CreateInstance(type);
            foreach (var field in type.GetFields())
                if (field.FieldType == typeof(string)) field.SetValue(data, field.Name + " " + i);
            SetField(data, "CanPurchase", i % 2 == 0);
            list.Add(data);
        }
        return list;
    }
    internal static object EventSystem => TypeOf("UnityEngine.EventSystems.EventSystem", "UnityEngine.UI").GetProperty("current").GetValue(null);
    internal static object Pointer(Vector2 position)
    {
        var data = Activator.CreateInstance(TypeOf("UnityEngine.EventSystems.PointerEventData", "UnityEngine.UI"), EventSystem);
        Set(data, "position", position); Set(data, "pressPosition", position);
        return data;
    }
    internal static bool Dispatch(Component target, string handler, object data)
    {
        var execute = TypeOf("UnityEngine.EventSystems.ExecuteEvents", "UnityEngine.UI");
        var callback = execute.GetProperty(handler == "initializePotentialDrag" ? handler : handler + "Handler").GetValue(null);
        var contract = TypeOf("UnityEngine.EventSystems.I" + char.ToUpperInvariant(handler[0]) + handler.Substring(1) + "Handler", "UnityEngine.UI");
        return (bool)execute.GetMethods().Single(m => m.Name == "Execute" && m.IsGenericMethod)
            .MakeGenericMethod(contract).Invoke(null, new[] { (object)target.gameObject, data, callback });
    }
    internal static void Move(Component target, string direction)
    {
        var data = Activator.CreateInstance(TypeOf("UnityEngine.EventSystems.AxisEventData", "UnityEngine.UI"), EventSystem);
        Set(data, "moveDir", Enum.Parse(TypeOf("UnityEngine.EventSystems.MoveDirection", "UnityEngine.UI"), direction));
        Assert.That(Dispatch(target, "move", data), Is.True);
    }
    internal static void Select(Component target) => EventSystem.GetType().GetMethod("SetSelectedGameObject", new[] { typeof(GameObject) })
        .Invoke(EventSystem, new object[] { target.gameObject });
    internal static GameObject Selected => (GameObject)Property(EventSystem, "currentSelectedGameObject");
    internal static void Click(Component target)
    {
        var data = Pointer(RectTransformUtility.WorldToScreenPoint(null, target.transform.position));
        Dispatch(target, "pointerDown", data); Dispatch(target, "pointerUp", data); Dispatch(target, "pointerClick", data);
    }
    internal static float Offset(object scroll) => ((RectTransform)Property(scroll, "content")).anchoredPosition.y;
    internal static void Offset(object scroll, float y)
    {
        Call(scroll, "StopMovement"); var content = (RectTransform)Property(scroll, "content");
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
        Canvas.ForceUpdateCanvases();
    }
    [Test] public void AuthoredNativeScrollbarHasExactSpriteIdentityGutterAndStableBindings()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "SpecialUpgradePanel.prefab");
        var view = root.GetComponent(TypeOf("SpecialUpgradePanelView")); var scroll = Field(view, "scroll");
        Assert.That(Components(root, "UnityEngine.UI.ScrollRect").Length, Is.EqualTo(1));
        var bar = (Component)Property(scroll, "verticalScrollbar"); Assert.That(bar, Is.Not.Null);
        Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(bar, out string guid, out long id), Is.True);
        Assert.That(id, Is.EqualTo(910000000000000005L)); Assert.That(guid, Is.EqualTo(AssetDatabase.AssetPathToGUID(Folder + "SpecialUpgradePanel.prefab")));
        Assert.That(new SerializedObject((UnityEngine.Object)scroll).FindProperty("m_VerticalScrollbar").objectReferenceValue, Is.SameAs(bar));
        Assert.That(Property(bar, "direction").ToString(), Is.EqualTo("BottomToTop"));
        Assert.That(Property(scroll, "verticalScrollbarVisibility").ToString(), Is.EqualTo("AutoHide"));
        Assert.That(Property(scroll, "scrollSensitivity"), Is.EqualTo(35f));
        var handle = (RectTransform)Property(bar, "handleRect"); var image = (Component)Property(bar, "targetGraphic");
        Assert.That(image.transform, Is.SameAs(handle)); Assert.That(Property(image, "type").ToString(), Is.EqualTo("Simple"));
        Assert.That(Property(image, "preserveAspect"), Is.False, "Native resizing must retain a full clickable thumb");
        var sprite = (Sprite)Property(image, "sprite"); Assert.That(sprite, Is.SameAs(Sprite("ScrollBar")));
        Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string sheetGuid, out long spriteId), Is.True);
        Assert.That(sheetGuid, Is.EqualTo("6b876b38e4597704b84b5e76e54d27a1")); Assert.That(spriteId, Is.EqualTo(-1066148658L));
        Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(9, 25))); Assert.That(sprite.border, Is.EqualTo(Vector4.zero));
        Assert.That(sprite.pixelsPerUnit, Is.EqualTo(16)); Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
        Assert.That(((RectTransform)bar.transform).sizeDelta.x, Is.EqualTo(24));
        var viewport = (RectTransform)Property(scroll, "viewport"); Assert.That(viewport.sizeDelta.x, Is.EqualTo(-32));
        Assert.That(viewport.GetComponent(TypeOf("UnityEngine.UI.RectMask2D", "UnityEngine.UI")), Is.Not.Null);
        Assert.That(((RectTransform)root.transform.Find("Window")).sizeDelta, Is.EqualTo(new Vector2(1150, 750)));
        AssertFrame(root, view);
    }
    [Test] public void RefreshPreservesPixelsAcrossPoolGrowthAndClampsShrinkWithoutVelocity()
    {
        var host = new GameObject("Scrollbar offset fixture", typeof(RectTransform), typeof(Canvas));
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "SpecialUpgradePanel.prefab"), host.transform, false);
        try
        {
            host.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var view = root.GetComponent(TypeOf("SpecialUpgradePanelView")); var scroll = Field(view, "scroll");
            Call(view, "Show", Items(6)); Offset(scroll, 321.25f); Set(scroll, "velocity", new Vector2(0, 250));
            Call(view, "Refresh", Items(9)); Assert.That(Offset(scroll), Is.EqualTo(321.25f).Within(.01));
            Assert.That(Property(scroll, "velocity"), Is.EqualTo(Vector2.zero));
            Call(view, "Refresh", Items(3)); Assert.That(Offset(scroll), Is.EqualTo(88).Within(.01));
            Call(view, "Refresh", Items(1)); Assert.That(Offset(scroll), Is.Zero.Within(.01));
            Call(view, "Show", Items(9)); Offset(scroll, 250); Call(view, "Hide"); Call(view, "Refresh", Items(9));
            Assert.That(Offset(scroll), Is.Zero.Within(.01), "Refresh on a hidden view is a new opening");
            Offset(scroll, 250); Call(view, "Show", Items(9)); Assert.That(Offset(scroll), Is.Zero.Within(.01));
        }
        finally { UnityEngine.Object.DestroyImmediate(host); }
    }
    [Test] public void NativeArrowsWheelAndEligibleRowNavigationActuallyMoveContent()
    {
        var host = new GameObject("Scrollbar navigation fixture", typeof(RectTransform), typeof(Canvas));
        var events = new GameObject("Scrollbar fixture EventSystem");
        var eventType = TypeOf("UnityEngine.EventSystems.EventSystem", "UnityEngine.UI");
        var previousEvents = eventType.GetProperty("current").GetValue(null);
        var fixtureEvents = events.AddComponent(eventType);
        // Edit Mode does not run EventSystem.OnEnable, so activate its native registration explicitly.
        eventType.GetMethod("OnEnable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(fixtureEvents, null);
        eventType.GetProperty("current").SetValue(null, fixtureEvents);
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "SpecialUpgradePanel.prefab"), host.transform, false);
        try
        {
            host.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var view = root.GetComponent(TypeOf("SpecialUpgradePanelView")); Call(view, "Show", Items(8));
            var scroll = (Component)Field(view, "scroll"); var bar = (Component)Property(scroll, "verticalScrollbar");
            var close = (Component)Field(view, "closeButton"); Select(close); Move(close, "Right"); Assert.That(Selected, Is.SameAs(bar.gameObject));
            float before = Offset(scroll); Move(bar, "Down"); Assert.That(Offset(scroll), Is.GreaterThan(before));
            Assert.That((float)Property(bar, "value"), Is.LessThan(1)); Move(bar, "Up"); Assert.That(Offset(scroll), Is.EqualTo(before).Within(.1));
            var wheel = Pointer(Vector2.zero); Set(wheel, "scrollDelta", new Vector2(0, -3)); Assert.That(Dispatch(scroll, "scroll", wheel), Is.True);
            Assert.That(Offset(scroll), Is.GreaterThan(before)); Move(bar, "Left"); Assert.That(Selected, Is.SameAs(close.gameObject));
            Move(close, "Up"); var rows = (IList)Field(view, "rows"); Assert.That(Selected, Is.SameAs(((Component)rows[6]).gameObject));
            Assert.That(Offset(scroll), Is.GreaterThan(500), "Selecting a lower row must reveal it");
            Move((Component)Field(rows[6], "purchaseButton"), "Up"); Assert.That(Selected, Is.SameAs(((Component)rows[4]).gameObject), "Disabled rows are skipped");
            Call(view, "Refresh", Items(2)); Assert.That(Selected, Is.SameAs(close.gameObject), "Pooled hidden selection returns to Close");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
            eventType.GetMethod("OnDisable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(fixtureEvents, null);
            UnityEngine.Object.DestroyImmediate(events);
            if (previousEvents != null) eventType.GetProperty("current").SetValue(null, previousEvents);
        }
    }
}
