using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SeedSlotsUIController : UIControllerBase
{
    [Header("Seed Slots")]
    [SerializeField] private GameObject[] slotObjects = new GameObject[9];
    [SerializeField] private Image[] slotIcons = new Image[9];
    [SerializeField] private Image[] slotBackgrounds = new Image[9];
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[9];
    [SerializeField] private TextMeshProUGUI[] seedCount = new TextMeshProUGUI[9];
    [SerializeField] private CanvasGroup seedSlotsCanvasGroup;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;

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

        RegisterSlotDragEvents();
    }

    protected override void ConfigureInitialState()
    {
        SetupSlots();
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

    private void SetupSlots()
    {
        foreach (var slotObj in slotObjects)
        {
            if (slotObj != null && slotObj.GetComponent<CanvasGroup>() == null)
                slotObj.AddComponent<CanvasGroup>();
        }
    }

    public void InitializeSlots()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] == null) continue;

            SetupTooltipHandler(i);
            SetupSlotNumber(i);
            UpdateSlotDisplay(i);
        }

        if (SeedInventory.Instance != null)
            UpdateSelectedSlotUI(SeedInventory.Instance.GetSelectedSlotIndex());
    }

    private void SetupTooltipHandler(int slotIndex)
    {
        var tooltip = slotObjects[slotIndex].GetComponent<PlantSlotTooltipHandler>();
        if (tooltip == null)
            tooltip = slotObjects[slotIndex].AddComponent<PlantSlotTooltipHandler>();
        tooltip.slotIndex = slotIndex;
    }

    private void SetupSlotNumber(int slotIndex)
    {
        if (slotNumbers[slotIndex] != null)
            slotNumbers[slotIndex].text = (slotIndex + 1).ToString();
    }

    private void UpdateSlotDisplay(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotObjects.Length) return;

        PlantSlot plantSlot = SeedInventory.Instance?.GetPlantSlot(slotIndex);

        if (HasValidSeed(plantSlot))
        {
            ShowSlotContent(slotIndex, plantSlot);
        }
        else
        {
            HideSlotContent(slotIndex);
        }
    }

    private bool HasValidSeed(PlantSlot plantSlot)
    {
        return plantSlot != null && plantSlot.seedCount > 0 && plantSlot.plantPrefab != null;
    }

    private void ShowSlotContent(int slotIndex, PlantSlot plantSlot)
    {
        if (slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].sprite = plantSlot.plantIcon;
            slotIcons[slotIndex].preserveAspect = true;
            slotIcons[slotIndex].gameObject.SetActive(true);
        }

        if (seedCount[slotIndex] != null)
        {
            seedCount[slotIndex].text = plantSlot.seedCount.ToString();
            seedCount[slotIndex].enabled = true;
        }
    }

    private void HideSlotContent(int slotIndex)
    {
        if (slotIcons[slotIndex] != null)
            slotIcons[slotIndex].gameObject.SetActive(false);

        if (seedCount[slotIndex] != null)
            seedCount[slotIndex].enabled = false;
    }

    public void UpdateSeedCounts()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    private void UpdateSelectedSlotUI(int selectedIndex)
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;

            bool isSelected = (i == selectedIndex);
            UpdateSlotVisualState(i, isSelected);
        }
    }

    private void UpdateSlotVisualState(int slotIndex, bool isSelected)
    {
        slotBackgrounds[slotIndex].color = isSelected ? selectedColor : normalColor;

        float scale = isSelected ? selectedScale : normalScale;
        slotObjects[slotIndex].transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void OnAbilityChanged(PlayerAbility newAbility)
    {
        if (seedSlotsCanvasGroup == null) return;

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

    private void HandleSeedSlotInput()
    {
        if (!CanHandleInput()) return;

        for (int i = 0; i < slotObjects.Length; i++)
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

    private void RegisterSlotDragEvents()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] == null) continue;

            SetupSlotEventTrigger(i);
        }
    }

    private void SetupSlotEventTrigger(int slotIndex)
    {
        var trigger = slotObjects[slotIndex].GetComponent<EventTrigger>() ??
                     slotObjects[slotIndex].AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        AddClickEvent(trigger, slotIndex);
        AddDragEvents(trigger, slotIndex);
        AddTooltipEvents(trigger, slotIndex);
    }

    private void AddClickEvent(EventTrigger trigger, int slotIndex)
    {
        var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        clickEntry.callback.AddListener(evt => OnSlotClicked(slotIndex));
        trigger.triggers.Add(clickEntry);
    }

    private void AddDragEvents(EventTrigger trigger, int slotIndex)
    {
        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener(_ => BeginDragIcon(slotIndex));
        trigger.triggers.Add(beginDrag);

        var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener(evt => OnDragIcon((PointerEventData)evt));
        trigger.triggers.Add(drag);

        var endDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDrag.callback.AddListener(evt => EndDragIcon((PointerEventData)evt));
        trigger.triggers.Add(endDrag);
    }

    private void AddTooltipEvents(EventTrigger trigger, int slotIndex)
    {
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(evt => UIEvents.TriggerTooltipRequested(slotIndex));
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(evt => UIEvents.TriggerTooltipHide());
        trigger.triggers.Add(exit);
    }

    private void OnSlotClicked(int slotIndex)
    {
        SelectSlotInternal(slotIndex);

        var abilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        abilitySystem?.SetAbility(PlayerAbility.Planting);

        UIManager.Instance.InterfaceSounds?.PlaySound(InterfaceSoundType.MenuButtonHover);
    }

    private void BeginDragIcon(int slotIndex)
    {
        if (!CanStartDrag(slotIndex)) return;

        InitializeDrag(slotIndex);
        ActivatePlantingAbility();
    }

    private bool CanStartDrag(int slotIndex)
    {
        PlantSlot slot = SeedInventory.Instance?.GetPlantSlot(slotIndex);
        return slot != null && slot.data != null && slot.seedCount > 0;
    }

    private void InitializeDrag(int slotIndex)
    {
        dragSourceIndex = slotIndex;
        CreateDragIcon(slotIndex);
        HideOriginalIcon(slotIndex);
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
        rt.sizeDelta = slotObjects[slotIndex].GetComponent<RectTransform>().sizeDelta;
    }

    private void SetupDragIconImage(int slotIndex)
    {
        var img = dragIcon.AddComponent<Image>();
        img.raycastTarget = false;
        img.sprite = slotIcons[slotIndex].sprite;
        img.color = new Color(1f, 1f, 1f, 0.8f);
    }

    private void HideOriginalIcon(int slotIndex)
    {
        if (slotIcons[slotIndex] != null)
            slotIcons[slotIndex].gameObject.SetActive(false);
    }

    private void OnDragIcon(PointerEventData data)
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
            for (int i = 0; i < slotObjects.Length; i++)
            {
                if (IsHoveringSlot(result.gameObject, i))
                    return i;
            }
        }
        return -1;
    }

    private bool IsHoveringSlot(GameObject hoveredObject, int slotIndex)
    {
        return hoveredObject == slotObjects[slotIndex] ||
               hoveredObject.transform.IsChildOf(slotObjects[slotIndex].transform);
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
        slotBackgrounds[slotIndex].color = highlightColor;
        hoveredSlotIndex = slotIndex;
    }

    private void EndDragIcon(PointerEventData data)
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
        if (slotIcons[dragSourceIndex] != null)
            slotIcons[dragSourceIndex].gameObject.SetActive(true);
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
        UpdateTooltipHandlers();
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

    private void UpdateTooltipHandlers()
    {
        for (int i = 0; i < slotObjects.Length; i++)
        {
            var tooltip = slotObjects[i]?.GetComponent<PlantSlotTooltipHandler>();
            if (tooltip != null)
                tooltip.slotIndex = i;
        }
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
        if (slotIndex < 0 || slotIndex >= slotObjects.Length) return;

        if (highlight)
        {
            slotBackgrounds[slotIndex].color = selectedColor;
            slotObjects[slotIndex].transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
        }
        else
        {
            slotBackgrounds[slotIndex].color = normalColor;
            slotObjects[slotIndex].transform.localScale = new Vector3(normalScale, normalScale, 1f);
        }
    }

    private void ClearSlotHighlight()
    {
        if (hoveredSlotIndex >= 0)
        {
            RestoreSlotNormalState(hoveredSlotIndex);
            hoveredSlotIndex = -1;
        }
    }

    private void RestoreSlotNormalState(int slotIndex)
    {
        bool isSelected = slotIndex == SeedInventory.Instance?.GetSelectedSlotIndex();
        slotBackgrounds[slotIndex].color = isSelected ? selectedColor : normalColor;
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
        if (slotIndex < 0 || slotIndex >= slotObjects.Length) return true;

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