using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestorationAltarUIManager : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject altarUIPanel;

    [Header("Restoration Options")]
    [SerializeField] private RestorationOptionButton[] optionButtons;

    [Header("Close Button")]
    [SerializeField] private CloseButton closeButton;

    [Header("Heart Animation")]
    [SerializeField] private HouseHealthHeartAnimator heartAnimator;
    [SerializeField] private float healingAnimationDuration = 1.5f;

    private HouseRestorationSystem restorationSystem;
    private HouseLifeController houseLife;


    private void Start()
    {
        InitializeReferences();
        SubscribeToEvents();
        InitializeUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeReferences()
    {
        restorationSystem = FindObjectOfType<HouseRestorationSystem>();

        if (GameObject.FindGameObjectWithTag("Home") != null)
            houseLife = GameObject.FindGameObjectWithTag("Home").GetComponent<HouseLifeController>();
    }

    private void SubscribeToEvents()
    {
        UIEvents.OnRestorationAltarUIToggleRequested += ToggleUI;

        SetupOptionButtons();

        if (closeButton != null)
            closeButton.OnClick.AddListener(CloseUI);
    }

    private void UnsubscribeFromEvents()
    {
        UIEvents.OnRestorationAltarUIToggleRequested -= ToggleUI;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null)
                optionButtons[i].OnClick.RemoveAllListeners();
        }

        if (closeButton != null)
            closeButton.OnClick.RemoveListener(CloseUI);
    }

    private void SetupOptionButtons()
    {
        if (restorationSystem == null) return;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            int index = i;

            if (index < restorationSystem.OptionCount)
            {
                var option = restorationSystem.GetOption(index);
                optionButtons[i].Setup(index, option);
            }

            optionButtons[i].OnClick.AddListener(() => HandleRestorationAttempt(index));
        }
    }

    private void InitializeUI()
    {
        altarUIPanel.SetActive(false);
        UpdateAllOptionButtons();
    }

    private void Update() { if (!(UIManager.Instance?.Flow != null && UIManager.Instance.Flow.IsOpen(UIModal.Restoration))) return; if (Input.GetKeyDown(KeyCode.Escape) && !InputConsumptionManager.IsEscapeConsumed) { InputConsumptionManager.ConsumeEscape(); CloseUI(); return; } } private void ToggleUI()
    {
        if (UIManager.Instance?.Flow != null && UIManager.Instance.Flow.IsOpen(UIModal.Restoration))
            CloseUI();
        else
            OpenRestoratioUI();
    }

    private void OpenRestoratioUI()
    {
        var flow = UIManager.Instance?.Flow;
        if (flow == null || !flow.Open(UIModal.Restoration))
            return;

        altarUIPanel.SetActive(true);
        TutorialEvents.InvokeRestorationOpened();

        UpdateAllOptionButtons();
        UpdateHeartVisual();
    }

    public void CloseUI()
    {
        altarUIPanel.SetActive(false);
        ClearEventSystemSelection();
        UIManager.Instance?.Flow?.Close(UIModal.Restoration);

        UIEvents.TriggerRestorationAltarUIClosed();
        TutorialEvents.InvokeRestorationClosed();
    }

    private void UpdateHeartVisual()
    {
        if (heartAnimator == null || houseLife == null) return;

        float healthPercent = houseLife.GetHealthPercent();
        heartAnimator.UpdateHeartVisual(healthPercent);
    }

    private void AnimateHouseHealing(float fromPercent, float toPercent)
    {
        if (heartAnimator == null) return;

        heartAnimator.AnimateHealing(fromPercent, toPercent, healingAnimationDuration);
    }

    private void HandleRestorationAttempt(int optionIndex)
    {
        if (restorationSystem == null || houseLife == null) return;

        bool canRestore = !restorationSystem.HasRestoredToday();

        if (!canRestore)
        {
            HandleRestorationFailure();
            return;
        }

        float healthBeforeHealing = houseLife.GetHealthPercent();

        bool success = restorationSystem.TryRestore(optionIndex);

        if (success)
        {
            float healthAfterHealing = houseLife.GetHealthPercent();
            AnimateHouseHealing(healthBeforeHealing, healthAfterHealing);
            UpdateAllOptionButtons();
            SoundManager.Instance?.Play("Restore");
        }
        else
        {
            HandleRestorationFailure();
        }
    }

    private void HandleRestorationFailure()
    {
        SoundManager.Instance?.Play("CantBuy");
    }

    private void UpdateAllOptionButtons()
    {
        if (restorationSystem == null) return;

        bool hasRestoredToday = restorationSystem.HasRestoredToday();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            if (i < restorationSystem.OptionCount)
            {
                optionButtons[i].UpdateDisplay();

                if (hasRestoredToday)
                    optionButtons[i].Interactable = false;
            }
            else
            {
                optionButtons[i].Interactable = false;
            }
        }
    }

    private void ClearEventSystemSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    }


