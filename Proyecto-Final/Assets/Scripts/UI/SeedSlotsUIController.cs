using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SeedSlotsUIController : UIControllerBase
{
    [Header("Seed Slots")]
    [SerializeField] private SeedSlot[] seedSlots = new SeedSlot[9];
    [SerializeField] private CanvasGroup seedSlotsCanvasGroup;

    [Header("Drag & Drop Settings")]
    [SerializeField] private float doublePressThreshold = 0.3f;

    private int lastPressSlot = -1;
    private float lastPressTime = 0f;
    private int pendingSwapSlot = -1;
    private int hoveredSlotIndex = -1;
    private Coroutine fadeCoroutine;

    private GameObject dragIcon;
    private int dragSourceIndex = -1;

    protected override void SetupEventListeners()
    {
        if (SeedInventory.Instance != null)
            SeedInventory.Instance.onSlotSelected += UpdateSelectedSlotUI;

        if (FindObjectOfType<PlayerAbilitySystem>() is PlayerAbilitySystem abilitySystem)
            abilitySystem.OnAbilityChanged += OnAbilityChanged;

        UIEvents.OnSlotSelected += UpdateSelectedSlotUI;
        UIEvents.OnSeedCountsUpdated += UpdateSeedCounts;

        SetupSlotEventListeners();
    }

    protected override void ConfigureInitialState()
    {
        InitializeSlots();

        var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        if (abilitySystem?.CurrentAbility != PlayerAbility.Planting && seedSlotsCanvasGroup != null)
        {
            seedSlotsCanvasGroup.alpha = 0.1f;
        }
    }

    public override void HandleUpdate()
    {
        HandleSeedSlotInput();
    }

    private void SetupSlotEventListeners()
    {
        for (int i = 0; i < seedSlots.Length; i++)
        {
            if (seedSlots[i] == null) continue;

            var slot = seedSlots[i];
            slot.OnSlotClicked += OnSlotClicked;
            slot.OnDragStarted += BeginDragIcon;
            slot.OnSlotDragged += OnDragIcon;
            slot.OnDragEnded += EndDragIcon;
            slot.OnSlotHovered += OnSlotHovered;
            slot.OnSlotUnhovered += OnSlotUnhovered;
        }
    }

    public void InitializeSlots()
    {
        for (int i = 0; i < seedSlots.Length; i++)
        {
            if (seedSlots[i] == null) continue;

            seedSlots[i].Initialize(i);
            UpdateSlotDisplay(i);
        }

        if (SeedInventory.Instance != null)
            UpdateSelectedSlotUI(SeedInventory.Instance.GetSelectedSlotIndex());
    }

    private void UpdateSlotDisplay(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= seedSlots.Length || seedSlots[slotIndex] == null) return;

        PlantSlot plantSlot = SeedInventory.Instance?.GetPlantSlot(slotIndex);
        seedSlots[slotIndex].UpdateSlotDisplay(plantSlot);
    }

    public void UpdateSeedCounts()
    {
        for (int i = 0; i < seedSlots.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    private void UpdateSelectedSlotUI(int selectedIndex)
    {
        for (int i = 0; i < seedSlots.Length; i++)
        {
            if (seedSlots[i] == null) continue;

            bool isSelected = (i == selectedIndex);
            seedSlots[i].SetSelected(isSelected);
        }
    }

    private void OnAbilityChanged(PlayerAbility newAbility)
    {
        if (seedSlotsCanvasGroup == null || !isActiveAndEnabled) return;

        bool shouldShow = newAbility == PlayerAbility.Planting;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePlantSlots(shouldShow));
    }

    private IEnumerator FadePlantSlots(bool fadeIn)
    {
        float duration = 0.35f;
        float startAlpha = seedSlotsCanvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0.1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            seedSlotsCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        seedSlotsCanvasGroup.alpha = targetAlpha;
    }

    private void OnSlotClicked(int slotIndex)
    {
        SelectSlotInternal(slotIndex);

        var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        abilitySystem?.SetAbility(PlayerAbility.Planting);

        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.MenuButtonHover);
    }

    private void OnSlotHovered(int slotIndex)
    {
        // Handle slot hover if needed
    }

    private void OnSlotUnhovered(int slotIndex)
    {
        // Handle slot unhover if needed
    }

    private void BeginDragIcon(int slotIndex)
    {
        if (!seedSlots[slotIndex].CanStartDrag()) return;

        InitializeDrag(slotIndex);
        ActivatePlantingAbility();
    }

    private void InitializeDrag(int slotIndex)
    {
        dragSourceIndex = slotIndex;
        CreateDragIcon(slotIndex);
        seedSlots[slotIndex].SetDragSource(true);
    }

    private void ActivatePlantingAbility()
    {
        var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        abilitySystem?.SetAbility(PlayerAbility.Planting);
    }

    private void CreateDragIcon(int slotIndex)
    {
        dragIcon = new GameObject("DragIcon");
        SetupDragIconTransform(slotIndex);
        SetupDragIconImage(slotIndex);
    }

    private void SetupDragIconTransform(int slotIndex)
    {
        var rt = dragIcon.AddComponent<RectTransform>();
        rt.SetParent(transform.root, false);
        rt.sizeDelta = seedSlots[slotIndex].GetComponent<RectTransform>().sizeDelta;
    }

    private void SetupDragIconImage(int slotIndex)
    {
        var img = dragIcon.AddComponent<Image>();
        img.raycastTarget = false;
        img.sprite = seedSlots[slotIndex].GetSlotIcon();
        img.color = new Color(1f, 1f, 1f, 0.8f);
    }

    private void OnDragIcon(int slotIndex, PointerEventData data)
    {
        if (dragIcon == null) return;

        UpdateDragIconPosition(data);
        UpdateSlotHighlight(data);
    }

    private void UpdateDragIconPosition(PointerEventData data)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            data.position,
            data.pressEventCamera,
            out Vector2 localPos))
        {
            (dragIcon.transform as RectTransform).anchoredPosition = localPos;
        }
    }

    private void UpdateSlotHighlight(PointerEventData data)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        int newHover = GetHoveredSlotIndex(results);
        UpdateHighlightState(newHover);
    }

    private int GetHoveredSlotIndex(List<RaycastResult> results)
    {
        foreach (var result in results)
        {
            for (int i = 0; i < seedSlots.Length; i++)
            {
                if (IsHoveringSlot(result.gameObject, i))
                    return i;
            }
        }
        return -1;
    }

    private bool IsHoveringSlot(GameObject hoveredObject, int slotIndex)
    {
        if (seedSlots[slotIndex] == null) return false;
        
        return hoveredObject == seedSlots[slotIndex].gameObject ||
               hoveredObject.transform.IsChildOf(seedSlots[slotIndex].transform);
    }

    private void UpdateHighlightState(int newHoverIndex)
    {
        if (newHoverIndex == hoveredSlotIndex) return;

        ClearSlotHighlight();

        if (ShouldHighlightSlot(newHoverIndex))
        {
            HighlightHoveredSlot(newHoverIndex);
        }
    }

    private bool ShouldHighlightSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex != dragSourceIndex;
    }

    private void HighlightHoveredSlot(int slotIndex)
    {
        seedSlots[slotIndex].SetHighlighted(true);
        hoveredSlotIndex = slotIndex;
    }

    private void EndDragIcon(int slotIndex, PointerEventData data)
    {
        DestroyDragIcon();

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        int hitIndex = GetHoveredSlotIndex(results);

        ProcessDragResult(hitIndex);
        CleanupDrag();
    }

    private void DestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }
    }

    private void ProcessDragResult(int hitIndex)
    {
        if (hitIndex == dragSourceIndex || hitIndex < 0)
        {
            RestoreOriginalIcon();
        }
        else
        {
            ExecuteDragSwap(hitIndex);
        }
    }

    private void RestoreOriginalIcon()
    {
        seedSlots[dragSourceIndex].SetDragSource(false);
    }

    private void ExecuteDragSwap(int targetIndex)
    {
        SwapSeedSlots(dragSourceIndex, targetIndex);
        RefreshSlotsAfterSwap(targetIndex);
    }

    private void RefreshSlotsAfterSwap(int newSelectedSlot)
    {
        ForceLayoutRebuild();
        InitializeSlots();
        SelectNewSlotAfterSwap(newSelectedSlot);
    }

    private void ForceLayoutRebuild()
    {
        var seedSlotsParent = GameObject.Find("SeedSlots")?.GetComponent<RectTransform>();
        if (seedSlotsParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(seedSlotsParent);
    }

    private void SelectNewSlotAfterSwap(int slotIndex)
    {
        SeedInventory.Instance?.SelectSlot(slotIndex);
        UpdateSelectedSlotUI(slotIndex);
    }

    private void CleanupDrag()
    {
        ClearSlotHighlight();
        dragSourceIndex = -1;
    }

    private void SwapSeedSlots(int a, int b)
    {
        PlantSlot slotA = SeedInventory.Instance?.GetPlantSlot(a);
        PlantSlot slotB = SeedInventory.Instance?.GetPlantSlot(b);

        if (slotA == null || slotB == null) return;

        SwapSlotProperties(slotA, slotB);
    }

    private void SwapSlotProperties(PlantSlot slotA, PlantSlot slotB)
    {
        (slotA.seedType, slotB.seedType) = (slotB.seedType, slotA.seedType);
        (slotA.plantPrefab, slotB.plantPrefab) = (slotB.plantPrefab, slotA.plantPrefab);
        (slotA.plantIcon, slotB.plantIcon) = (slotB.plantIcon, slotA.plantIcon);
        (slotA.seedCount, slotB.seedCount) = (slotB.seedCount, slotA.seedCount);
        (slotA.daysToGrow, slotB.daysToGrow) = (slotB.daysToGrow, slotA.daysToGrow);
        (slotA.description, slotB.description) = (slotB.description, slotA.description);
        (slotA.data, slotB.data) = (slotB.data, slotA.data);
        (slotA.plantName, slotB.plantName) = (slotB.plantName, slotA.plantName);
    }

    private void HighlightSlot(int slotIndex, bool highlight)
    {
        if (slotIndex < 0 || slotIndex >= seedSlots.Length || seedSlots[slotIndex] == null) return;
        seedSlots[slotIndex].SetHighlighted(highlight);
    }

    private void ClearSlotHighlight()
    {
        if (hoveredSlotIndex >= 0 && seedSlots[hoveredSlotIndex] != null)
        {
            seedSlots[hoveredSlotIndex].SetHighlighted(false);
            hoveredSlotIndex = -1;
        }
    }

    private void HandleSeedSlotInput()
    {
        if (!CanHandleInput()) return;

        for (int i = 0; i < seedSlots.Length; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                OnSlotKeyPressed(i);
                break;
            }
        }
    }

    private bool CanHandleInput()
    {
        if (LevelManager.Instance == null) return false;

        var state = LevelManager.Instance.currentGameState;
        return state == GameState.Day || state == GameState.Digging ||
               state == GameState.Planting || state == GameState.Harvesting ||
               state == GameState.Removing;
    }

    private void OnSlotKeyPressed(int slotIndex)
    {
        float now = Time.time;

        if (pendingSwapSlot >= 0)
        {
            HandlePendingSwap(slotIndex);
            return;
        }

        if (IsDoublePress(slotIndex, now))
        {
            InitiateSwapMode(slotIndex);
        }
        else
        {
            SelectSlotInternal(slotIndex);
        }

        UpdatePressHistory(slotIndex, now);
    }

    private void HandlePendingSwap(int targetSlot)
    {
        int sourceSlot = pendingSwapSlot;

        if (sourceSlot == targetSlot)
        {
            CancelSwapMode(sourceSlot);
        }
        else
        {
            ExecuteSwap(sourceSlot, targetSlot);
        }
    }

    private bool IsDoublePress(int slotIndex, float currentTime)
    {
        return slotIndex == lastPressSlot &&
               (currentTime - lastPressTime) < doublePressThreshold;
    }

    private void InitiateSwapMode(int slotIndex)
    {
        pendingSwapSlot = slotIndex;
        HighlightSlot(slotIndex, true);
    }

    private void SelectSlotInternal(int slotIndex)
    {
        SeedInventory.Instance?.SelectSlot(slotIndex);
        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.OnSeedSelect);
        UpdateSelectedSlotUI(slotIndex);
    }

    private void UpdatePressHistory(int slotIndex, float time)
    {
        lastPressSlot = slotIndex;
        lastPressTime = time;
    }

    private void CancelSwapMode(int slotIndex)
    {
        HighlightSlot(slotIndex, false);
        pendingSwapSlot = -1;
    }

    private void ExecuteSwap(int slotA, int slotB)
    {
        SwapSeedSlots(slotA, slotB);
        HighlightSlot(slotA, false);
        pendingSwapSlot = -1;

        RefreshAfterSwap();
    }

    private void RefreshAfterSwap()
    {
        InitializeSlots();
        if (SeedInventory.Instance != null)
            UpdateSelectedSlotUI(SeedInventory.Instance.GetSelectedSlotIndex());
    }

    public void SelectSlot(int slotIndex)
    {
        if (SeedInventory.Instance != null)
        {
            SeedInventory.Instance.SelectSlot(slotIndex);
            UpdateSelectedSlotUI(slotIndex);
        }
    }

    public int GetSelectedSlotIndex()
    {
        return SeedInventory.Instance?.GetSelectedSlotIndex() ?? 0;
    }

    public void RefreshSlotDisplay(int slotIndex)
    {
        UpdateSlotDisplay(slotIndex);
    }

    public void RefreshAllSlots()
    {
        InitializeSlots();
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= seedSlots.Length) return true;

        PlantSlot slot = SeedInventory.Instance?.GetPlantSlot(slotIndex);
        return slot == null || slot.seedCount <= 0;
    }

    public PlantSlot GetSlotData(int slotIndex)
    {
        return SeedInventory.Instance?.GetPlantSlot(slotIndex);
    }

    protected override void CleanupEventListeners()
    {
        if (SeedInventory.Instance != null)
            SeedInventory.Instance.onSlotSelected -= UpdateSelectedSlotUI;

        var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        if (abilitySystem != null)
            abilitySystem.OnAbilityChanged -= OnAbilityChanged;

        UIEvents.OnSlotSelected -= UpdateSelectedSlotUI;
        UIEvents.OnSeedCountsUpdated -= UpdateSeedCounts;

        // Cleanup slot event listeners
        for (int i = 0; i < seedSlots.Length; i++)
        {
            if (seedSlots[i] == null) continue;

            var slot = seedSlots[i];
            slot.OnSlotClicked -= OnSlotClicked;
            slot.OnDragStarted -= BeginDragIcon;
            slot.OnSlotDragged -= OnDragIcon;
            slot.OnDragEnded -= EndDragIcon;
            slot.OnSlotHovered -= OnSlotHovered;
            slot.OnSlotUnhovered -= OnSlotUnhovered;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }
    }

    protected override void OnDestroy()
    {
        CleanupEventListeners();
        base.OnDestroy();
    }

    protected override void CacheReferences()
    {

    }
}