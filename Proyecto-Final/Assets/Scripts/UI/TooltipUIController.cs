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
    [SerializeField] private Vector2 tooltipOffset = new Vector2(100f, -100f);

    private bool isTooltipVisible = false;
    private int currentSlotIndex = -1;
    private float showTimer = 0f;
    private float hideTimer = 0f;
    private bool pendingShow = false;
    private bool pendingHide = false;

    protected override void CacheReferences() { }

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
        if (!ValidateTooltipComponents())
        {
            return;
        }

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

        Vector3 targetPosition = Input.mousePosition + (Vector3)tooltipOffset;
        targetPosition = ClampTooltipToScreen(targetPosition);
        tooltipPanel.transform.position = targetPosition;

        seedNameText.text = data.plantName;
        seedDescriptionText.text = FormatPlantDescription(data);
        ConfigureTooltipImage(data);
    }

    private string FormatPlantDescription(PlantDataSO data)
    {
        return $"{data.description}\n<color=#FFD700>Días para crecer: {data.daysToGrow}</color>";
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
        if (tooltipPanel != null && isTooltipVisible)
        {
            Vector3 targetPosition = Input.mousePosition + (Vector3)tooltipOffset;
            targetPosition = ClampTooltipToScreen(targetPosition);
            tooltipPanel.transform.position = targetPosition;
        }
    }

    private Vector3 ClampTooltipToScreen(Vector3 position)
    {
        if (tooltipPanel == null) return position;

        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (tooltipRect == null) return position;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 tooltipSize = tooltipRect.sizeDelta;

        float margin = 10f;
        position.x = Mathf.Clamp(position.x, tooltipSize.x * 0.5f + margin,
                                screenSize.x - tooltipSize.x * 0.5f - margin);
        position.y = Mathf.Clamp(position.y, tooltipSize.y * 0.5f + margin,
                                screenSize.y - tooltipSize.y * 0.5f - margin);

        return position;
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