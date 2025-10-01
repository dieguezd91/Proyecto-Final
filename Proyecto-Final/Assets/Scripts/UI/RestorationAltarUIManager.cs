using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestorationAltarUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject altarUIPanel;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Slider goldSlider;
    [SerializeField] private TextMeshProUGUI goldAmountText;
    [SerializeField] private TMP_InputField goldInputField;

    public static bool isUIOpen = false;

    private HouseRestorationSystem restorationSystem;

    private void Start()
    {
        restorationSystem = FindObjectOfType<HouseRestorationSystem>();

        UIEvents.OnRestorationAltarUIToggleRequested += ToggleUI;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => TryRestore(index));
        }

        altarUIPanel.SetActive(false);
        goldSlider.onValueChanged.AddListener(OnSliderChanged);
        goldInputField.onEndEdit.AddListener(OnInputFieldChanged);

        UpdateOptionLabels();
    }

    private void OnDestroy()
    {
        UIEvents.OnRestorationAltarUIToggleRequested -= ToggleUI;
    }

    private void ToggleUI()
    {
        if (isUIOpen)
            CloseUI();
        else
            OpenUI();
    }

    private void OpenUI()
    {
        isUIOpen = true;
        altarUIPanel.SetActive(true);
        LevelManager.Instance?.SetGameState(GameState.OnAltarRestoration);

        int currentGold = InventoryManager.Instance != null ? InventoryManager.Instance.GetGold() : 0;

        goldSlider.maxValue = currentGold;
        goldSlider.minValue = 0;
        goldSlider.wholeNumbers = true;

        goldSlider.value = currentGold;
        goldInputField.text = currentGold.ToString();

        UpdateOptionLabels();
    }

    public void CloseUI()
    {
        isUIOpen = false;
        altarUIPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (LevelManager.Instance?.GetCurrentGameState() == GameState.OnAltarRestoration)
            LevelManager.Instance.SetGameState(GameState.Digging);

        UIEvents.TriggerRestorationAltarUIClosed();
    }

    private void TryRestore(int index)
    {
        bool success = restorationSystem.TryRestore(index);
        if (success)
        {
            CloseUI();
        }
        else
        {
            Debug.Log("No se pudo restaurar (recursos insuficientes o ya usado).");
        }
    }

    private void UpdateOptionLabels()
    {
        if (restorationSystem == null) return;

        for (int i = 0; i < optionLabels.Length; i++)
        {
            bool within = i < restorationSystem.OptionCount;

            if (within)
            {
                var opt = restorationSystem.GetOption(i);
                string materialIcon = GetMaterialSpriteName(opt.materialRequired);

                int gold = InventoryManager.Instance != null ? InventoryManager.Instance.GetGold() : 0;
                bool hasGold = gold >= opt.goldCost;
                bool hasMaterial = InventoryManager.Instance != null &&
                                   InventoryManager.Instance.HasEnoughMaterial(opt.materialRequired, 1);

                string goldText = $"<color={(hasGold ? "green" : "red")}><sprite name=\"GoldIcon\"> {opt.goldCost}</color>";
                string materialText = $"<color={(hasMaterial ? "green" : "red")}><sprite name=\"{materialIcon}\"> 1</color>";

                optionLabels[i].text = $"{opt.restorePercentage}% HP\n{goldText}  +  {materialText}";

                if (optionButtons[i] != null)
                    optionButtons[i].interactable = !restorationSystem.HasRestoredToday() && hasGold && hasMaterial;
            }
            else
            {
                if (optionButtons[i] != null) optionButtons[i].interactable = false;
                if (optionLabels[i] != null) optionLabels[i].text = "-";
            }
        }
    }

    private void OnSliderChanged(float value)
    {
        int stepped = Mathf.FloorToInt(value / 10f) * 10;
        goldSlider.SetValueWithoutNotify(stepped);
        goldInputField.SetTextWithoutNotify(stepped.ToString());
        goldAmountText.text = $"{stepped} oro ofrecido";
    }

    private void OnInputFieldChanged(string input)
    {
        if (int.TryParse(input, out int value))
        {
            int stepped = Mathf.FloorToInt(value / 10f) * 10;
            stepped = Mathf.Clamp(stepped, 0, (int)goldSlider.maxValue);

            goldSlider.SetValueWithoutNotify(stepped);
            goldAmountText.text = $"{stepped} oro ofrecido";
            goldInputField.SetTextWithoutNotify(stepped.ToString());
        }
        else
        {
            goldInputField.SetTextWithoutNotify("0");
        }
    }

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
}