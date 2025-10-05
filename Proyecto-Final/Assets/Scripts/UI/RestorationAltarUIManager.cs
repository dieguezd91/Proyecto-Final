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
    private HouseRestorationSystem restorationSystem;
    private const int GOLD_STEP = 10;

    public static bool isUIOpen = false;

    #region Unity Lifecycle
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
    #endregion

    #region Initialization
    private void InitializeReferences()
    {
        restorationSystem = FindObjectOfType<HouseRestorationSystem>();
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
    #endregion

    #region UI Toggle
    private void ToggleUI()
    {
        if (isUIOpen)
            CloseUI();
        else
            OpenUI();
    }

    private void OpenUI()
    {
        altarUIPanel.SetActive(true);
        isUIOpen = true;

        LevelManager.Instance?.SetGameState(GameState.OnAltarRestoration);
        UpdateAllOptionButtons();
    }

    public void CloseUI()
    {
        altarUIPanel.SetActive(false);
        isUIOpen = false;

        ClearEventSystemSelection();
        RestoreGameState();

        UIEvents.TriggerRestorationAltarUIClosed();
    }
    #endregion

    #region Restoration Logic
    private void HandleRestorationAttempt(int optionIndex)
    {
        bool canRestore = restorationSystem != null && !restorationSystem.HasRestoredToday();

        if (!canRestore)
        {
            HandleRestorationFailure();
            return;
        }

        bool success = restorationSystem.TryRestore(optionIndex);

        if (success)
        {
            CloseUI();
        }
        else
        {
            HandleRestorationFailure();
        }
    }

    private void HandleRestorationFailure()
    {
        Debug.Log("No se pudo restaurar (recursos insuficientes o ya usado).");
    }
    #endregion

    #region Option Buttons Update
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
    #endregion

    #region Helper Methods
    private void ClearEventSystemSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void RestoreGameState()
    {
        if (LevelManager.Instance?.GetCurrentGameState() == GameState.OnAltarRestoration)
            LevelManager.Instance.SetGameState(GameState.Digging);
    }
    #endregion
}