using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpellSlotUI : ImprovedUIButton
{
    [Header("Slot Components")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image slotIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI slotNumberText;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.6f, 1f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float selectedScale = 1.1f;

    [Header("Cooldown Settings")]
    [SerializeField] private Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.7f);

    public int SlotIndex { get; private set; }

    public event Action<int> OnSlotClicked;

    private void Awake()
    {
        OnClick.AddListener(HandleSlotClick);

        if (cooldownOverlay != null)
        {
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            cooldownOverlay.fillClockwise = false;
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.color = cooldownOverlayColor;
        }
    }

    private void OnDestroy()
    {
        OnClick.RemoveListener(HandleSlotClick);
    }

    public void Initialize(int index)
    {
        SlotIndex = index;

        if (slotNumberText != null)
        {
            slotNumberText.text = (index + 1).ToString();
        }

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    private void HandleSlotClick()
    {
        OnSlotClicked?.Invoke(SlotIndex);
    }

    public void UpdateSlotDisplay(SpellSlot spellSlot)
    {
        if (spellSlot == null || !spellSlot.isUnlocked)
        {
            ShowLockedSlot();
        }
        else
        {
            ShowUnlockedSlot(spellSlot);
        }
    }

    private void ShowLockedSlot()
    {
        if (slotIcon != null)
            slotIcon.gameObject.SetActive(false);

        if (slotBackground != null)
            slotBackground.color = lockedColor;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;
    }

    private void ShowUnlockedSlot(SpellSlot spellSlot)
    {
        if (slotIcon != null && spellSlot.spellIcon != null)
        {
            slotIcon.sprite = spellSlot.spellIcon;
            slotIcon.preserveAspect = true;
            slotIcon.gameObject.SetActive(true);
        }

        UpdateCooldownDisplay(spellSlot);
    }

    public void UpdateCooldownDisplay(SpellSlot spellSlot)
    {
        if (spellSlot == null || !spellSlot.isUnlocked) return;

        bool onCooldown = spellSlot.currentCooldown > 0f;

        if (cooldownOverlay != null)
        {
            if (onCooldown)
            {
                float progress = spellSlot.currentCooldown / spellSlot.cooldown;
                cooldownOverlay.fillAmount = progress;
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
            }
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(onCooldown);
            if (onCooldown)
            {
                cooldownText.text = spellSlot.currentCooldown.ToString("F2");
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (slotBackground == null) return;

        bool isLocked = SpellInventory.Instance?.GetSpellSlot(SlotIndex)?.isUnlocked == false;

        if (isLocked)
        {
            slotBackground.color = lockedColor;
        }
        else
        {
            slotBackground.color = selected ? selectedColor : normalColor;
        }

        float scale = selected ? selectedScale : normalScale;
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}