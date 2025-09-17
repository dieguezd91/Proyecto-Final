using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUIController : UIControllerBase
{
    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI seedNameText;
    [SerializeField] private TextMeshProUGUI seedDescriptionText;
    [SerializeField] private Image fullyGrownImage;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(0f, 120f);

    [SerializeField] private Canvas rootCanvas;
    private RectTransform canvasRect;
    private RectTransform tooltipRect;

    private bool isTooltipVisible = false;
    private int currentSlotIndex = -1;
    private float showTimer = 0f;
    private float hideTimer = 0f;
    private bool pendingShow = false;
    private bool pendingHide = false;

    protected override void CacheReferences()
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>(true);
        if (tooltipPanel != null) tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (rootCanvas != null) canvasRect = rootCanvas.GetComponent<RectTransform>();
    }

    protected override void SetupEventListeners()
    {
        UIEvents.OnTooltipRequested += OnTooltipRequested;
        UIEvents.OnTooltipHideRequested += OnTooltipHideRequested;
    }

    protected override void ConfigureInitialState()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        ResetState();
    }

    void Update()
    {
        HandleUpdate();
    }

    public override void HandleUpdate()
    {
        HandleTooltipTimers();

        if (isTooltipVisible)
        {
            UpdateTooltipPosition();
        }
    }

    private void HandleTooltipTimers()
    {
        if (pendingShow)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0f)
            {
                ExecuteShowTooltip();
            }
        }

        if (pendingHide)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                ExecuteHideTooltip();
            }
        }
    }

    private void OnTooltipRequested(int slotIndex)
    {
        if (LevelManager.Instance.currentGameState != GameState.Planting)
        {
            return;
        }

        if (isTooltipVisible && currentSlotIndex == slotIndex)
        {
            return;
        }

        CancelHide();

        if (isTooltipVisible && currentSlotIndex != slotIndex)
        {
            ShowSlotTooltip(slotIndex);
            return;
        }

        ScheduleShow(slotIndex);
    }

    private void OnTooltipHideRequested()
    {
        CancelShow();

        if (isTooltipVisible)
        {
            ScheduleHide();
        }
    }

    private void ScheduleShow(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        pendingShow = true;
    }

    private void ScheduleHide()
    {
        pendingHide = true;
    }

    private void CancelShow()
    {
        if (pendingShow)
        {
            pendingShow = false;
            showTimer = 0f;
        }
    }

    private void CancelHide()
    {
        if (pendingHide)
        {
            pendingHide = false;
            hideTimer = 0f;
        }
    }

    private void ExecuteShowTooltip()
    {
        pendingShow = false;
        ShowSlotTooltip(currentSlotIndex);
    }

    private void ExecuteHideTooltip()
    {
        pendingHide = false;
        HideTooltip();
    }

    public void ShowSlotTooltip(int slotIndex)
    {
        if (LevelManager.Instance.currentGameState != GameState.Planting)
        {
            HideTooltip();
            return;
        }

        if (!ValidateTooltipComponents())
            return;

        PlantSlot slot = SeedInventory.Instance?.GetPlantSlot(slotIndex);
        if (slot == null || slot.seedCount <= 0)
        {
            HideTooltip();
            return;
        }

        PlantDataSO data = slot.data;
        if (data == null)
        {
            HideTooltip();
            return;
        }

        ShowTooltipWithData(data);
        currentSlotIndex = slotIndex;
    }

    private bool ValidateTooltipComponents()
    {
        return tooltipPanel != null && seedNameText != null && seedDescriptionText != null;
    }

    private void ShowTooltipWithData(PlantDataSO data)
    {
        tooltipPanel.SetActive(true);
        isTooltipVisible = true;

        seedNameText.text = data.plantName;
        seedDescriptionText.text = FormatPlantDescription(data);
        ConfigureTooltipImage(data);

        UpdateTooltipPosition();
    }

    private string FormatPlantDescription(PlantDataSO data)
    {
        return $"{data.description}\n<color=#FFD700>DAYS TO GROW: {data.daysToGrow}</color>";
    }

    private void ConfigureTooltipImage(PlantDataSO data)
    {
        if (fullyGrownImage == null) return;

        if (data.fullyGrownSprite != null)
        {
            fullyGrownImage.sprite = data.fullyGrownSprite;
            fullyGrownImage.preserveAspect = true;
            fullyGrownImage.gameObject.SetActive(true);
        }
        else
        {
            fullyGrownImage.sprite = null;
            fullyGrownImage.gameObject.SetActive(false);
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        ResetState();
    }

    private void ResetState()
    {
        isTooltipVisible = false;
        currentSlotIndex = -1;
        CancelShow();
        CancelHide();
    }

    private void UpdateTooltipPosition()
    {
        if (!isTooltipVisible || tooltipRect == null || canvasRect == null) return;

        var cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, Input.mousePosition, cam, out Vector2 localMouse))
        {
            Vector2 target = localMouse + tooltipOffset;

            float margin = 10f;
            float halfHeight = tooltipRect.rect.height * (1f - tooltipRect.pivot.y);
            if (target.y + halfHeight + margin > canvasRect.rect.yMax)
            {
                target.y = localMouse.y - Mathf.Abs(tooltipOffset.y);
            }

            tooltipRect.anchoredPosition = ClampToCanvas(target, tooltipRect, canvasRect, margin);
        }
    }

    private static Vector2 ClampToCanvas(
    Vector2 anchored, RectTransform tooltip, RectTransform canvas, float margin)
    {
        Rect c = canvas.rect;
        Vector2 size = tooltip.rect.size;
        Vector2 pivot = tooltip.pivot;

        float minX = c.xMin + size.x * pivot.x + margin;
        float maxX = c.xMax - size.x * (1f - pivot.x) - margin;
        float minY = c.yMin + size.y * pivot.y + margin;
        float maxY = c.yMax - size.y * (1f - pivot.y) - margin;

        anchored.x = Mathf.Clamp(anchored.x, minX, maxX);
        anchored.y = Mathf.Clamp(anchored.y, minY, maxY);
        return anchored;
    }

    public void ForceHide()
    {
        CancelShow();
        CancelHide();
        HideTooltip();
    }

    protected override void CleanupEventListeners()
    {
        UIEvents.OnTooltipRequested -= OnTooltipRequested;
        UIEvents.OnTooltipHideRequested -= OnTooltipHideRequested;
    }

    protected override void OnDestroy()
    {
        ForceHide();
        base.OnDestroy();
    }
}