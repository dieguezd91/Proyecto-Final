using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestorationAltarUIManager : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject altarUIPanel;

    [Header("Restoration Options")]
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private ImprovedUIButton[] optionButtons;

    [Header("Gold Controls")]
    [SerializeField] private Slider goldSlider;
    [SerializeField] private TextMeshProUGUI goldAmountText;
    [SerializeField] private TMP_InputField goldInputField;

    [Header("Close Button")]
    [SerializeField] private ImprovedUIButton closeButton;

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

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;

            if (optionButtons[i] != null)
                optionButtons[i].OnClick.AddListener(() => HandleRestorationAttempt(index));
        }

        goldSlider.onValueChanged.AddListener(OnSliderChanged);
        goldInputField.onEndEdit.AddListener(OnInputFieldChanged);

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

        goldSlider.onValueChanged.RemoveListener(OnSliderChanged);
        goldInputField.onEndEdit.RemoveListener(OnInputFieldChanged);

        if (closeButton != null)
            closeButton.OnClick.RemoveListener(CloseUI);
    }

    private void InitializeUI()
    {
        altarUIPanel.SetActive(false);
        UpdateOptionLabels();
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

        InitializeGoldControls();
        UpdateOptionLabels();
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

    private void InitializeGoldControls()
    {
        int currentGold = GetPlayerGold();

        ConfigureGoldSlider(currentGold);
        SetGoldDisplayValue(currentGold);
    }

    private void ConfigureGoldSlider(int maxGold)
    {
        goldSlider.maxValue = maxGold;
        goldSlider.minValue = 0;
        goldSlider.wholeNumbers = true;
        goldSlider.value = maxGold;
    }

    private void SetGoldDisplayValue(int amount)
    {
        goldInputField.text = amount.ToString();
        goldAmountText.text = $"{amount} oro ofrecido";
    }

    private void OnSliderChanged(float value)
    {
        int steppedValue = CalculateSteppedValue(value);
        UpdateGoldDisplay(steppedValue);
    }

    private void OnInputFieldChanged(string input)
    {
        if (int.TryParse(input, out int value))
        {
            int steppedValue = CalculateSteppedValue(value);
            int clampedValue = Mathf.Clamp(steppedValue, 0, (int)goldSlider.maxValue);
            UpdateGoldDisplay(clampedValue);
        }
        else
        {
            ResetGoldInput();
        }
    }

    private int CalculateSteppedValue(float value)
    {
        return Mathf.FloorToInt(value / GOLD_STEP) * GOLD_STEP;
    }

    private void UpdateGoldDisplay(int amount)
    {
        goldSlider.SetValueWithoutNotify(amount);
        goldInputField.SetTextWithoutNotify(amount.ToString());
        goldAmountText.text = $"{amount} oro ofrecido";
    }

    private void ResetGoldInput()
    {
        goldInputField.SetTextWithoutNotify("0");
    }

    #region Restoration Logic
    private void HandleRestorationAttempt(int optionIndex)
    {
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

    #region Option Labels Update
    private void UpdateOptionLabels()
    {
        if (restorationSystem == null) return;

        for (int i = 0; i < optionLabels.Length; i++)
        {
            UpdateSingleOptionLabel(i);
        }
    }

    private void UpdateSingleOptionLabel(int index)
    {
        bool isValidOption = index < restorationSystem.OptionCount;

        if (isValidOption)
        {
            DisplayRestorationOption(index);
        }
        else
        {
            DisplayEmptyOption(index);
        }
    }

    private void DisplayRestorationOption(int index)
    {
        var option = restorationSystem.GetOption(index);

        bool hasGold = HasSufficientGold(option.goldCost);
        bool hasMaterial = HasSufficientMaterial(option.materialRequired);
        bool canRestore = !restorationSystem.HasRestoredToday();

        optionLabels[index].text = FormatOptionText(option, hasGold, hasMaterial);

        if (optionButtons[index] != null)
            optionButtons[index].Interactable = canRestore && hasGold && hasMaterial;
    }

    private void DisplayEmptyOption(int index)
    {
        if (optionLabels[index] != null)
            optionLabels[index].text = "-";

        if (optionButtons[index] != null)
            optionButtons[index].Interactable = false;
    }

    private string FormatOptionText(HouseRestorationSystem.RestorationOption option, bool hasGold, bool hasMaterial)
    {
        string goldColor = hasGold ? "green" : "red";
        string materialColor = hasMaterial ? "green" : "red";
        string materialIcon = GetMaterialSpriteName(option.materialRequired);

        string goldText = $"<color={goldColor}><sprite name=\"GoldIcon\"> {option.goldCost}</color>";
        string materialText = $"<color={materialColor}><sprite name=\"{materialIcon}\"> 1</color>";

        return $"{option.restorePercentage}% HP\n{goldText}  +  {materialText}";
    }
    #endregion

    #region Resource Validation
    private bool HasSufficientGold(int requiredAmount)
    {
        int playerGold = GetPlayerGold();
        return playerGold >= requiredAmount;
    }

    private bool HasSufficientMaterial(MaterialType materialType)
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.HasEnoughMaterial(materialType, 1);
    }

    private int GetPlayerGold()
    {
        return InventoryManager.Instance != null ? InventoryManager.Instance.GetGold() : 0;
    }
    #endregion

    #region Material Sprite Mapping
    private string GetMaterialSpriteName(MaterialType type)
    {
        return type switch
        {
            MaterialType.SpectralCrystal => "SpectralCrystal",
            MaterialType.WindwalkerEssence => "WindwalkerEssence",
            MaterialType.VoltaicCore => "VoltaicCore",
            MaterialType.EternalEmber => "EternalEmber",
            MaterialType.StellarFragment => "StellarFragment",
            MaterialType.LunarEssence => "LunarEssence",
            MaterialType.CrystallizedTears => "CrystallizedTears",
            MaterialType.FlameberryFruit => "FlameberryFruit",
            MaterialType.AstralRoots => "AstralRoots",
            MaterialType.VoltaicPollen => "VoltaicPollen",
            MaterialType.FrostSpores => "FrostSpores",
            MaterialType.EtherealTendrils => "EtherealTendrils",
            MaterialType.HouseHealingPotion => "HouseHealingPotion",
            MaterialType.Gold => "GoldIcon",
            _ => type.ToString(),
        };
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