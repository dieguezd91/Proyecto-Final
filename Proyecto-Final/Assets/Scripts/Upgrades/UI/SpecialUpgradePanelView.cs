using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SpecialUpgradePanelView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text headingLabel, emptyLabel;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform content;
    [SerializeField] private SpecialUpgradeItemView itemPrefab;
    private readonly List<SpecialUpgradeItemView> rows = new List<SpecialUpgradeItemView>();
    public event Action<int> Purchased;
    public event Action Closed;
    public event Action Hidden;
    public bool IsConfigured => root == gameObject && background != null && headingLabel != null && emptyLabel != null &&
        closeButton != null && scroll != null && content != null && scroll.content == content && scroll.viewport != null &&
        itemPrefab != null && itemPrefab.IsConfigured;
    public bool Show(IList<SpecialUpgradeItemData> items)
    {
        if (!IsConfigured || items == null) return false;
        closeButton.onClick.RemoveListener(Close);
        closeButton.onClick.AddListener(Close);
        // The only runtime visual instantiation: an already-authored shop item prefab.
        while (rows.Count < items.Count) rows.Add(Instantiate(itemPrefab, content, false));
        for (int i = 0; i < rows.Count; i++)
        {
            if (i < items.Count) rows[i].Bind(items[i], i, Purchase);
            else rows[i].Hide();
        }
        emptyLabel.gameObject.SetActive(items.Count == 0);
        root.SetActive(true);
        scroll.verticalNormalizedPosition = 1;
        EventSystem.current?.SetSelectedGameObject(closeButton.gameObject);
        return true;
    }
    private void Purchase(int index) => Purchased?.Invoke(index);
    private void Close() => Closed?.Invoke();
    public void Hide()
    {
        foreach (var row in rows) if (row != null) row.Hide();
        if (root != null) root.SetActive(false);
    }
    private void OnDisable() => Hidden?.Invoke();
    private void OnDestroy() { if (closeButton != null) closeButton.onClick.RemoveListener(Close); }
}
