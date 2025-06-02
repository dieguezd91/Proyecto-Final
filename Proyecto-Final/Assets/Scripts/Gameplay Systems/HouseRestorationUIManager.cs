using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HouseRestorationUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject altarUIPanel;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Slider goldSlider;
    [SerializeField] private TextMeshProUGUI goldAmountText;
    [SerializeField] private TMP_InputField goldInputField;

    private bool isPlayerNear = false;
    public static bool isUIOpen = false;

    [SerializeField] private GameObject interactionPromptCanvas;
    [SerializeField] private float promptDistance = 2.5f;
    private Transform player;


    private HouseRestorationSystem restorationSystem;

    private void Start()
    {
        restorationSystem = FindObjectOfType<HouseRestorationSystem>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);

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

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            ToggleUI();
        }

        if (player != null && interactionPromptCanvas != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            interactionPromptCanvas.SetActive(dist <= promptDistance);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (isUIOpen)
                CloseUI();
        }
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
        GameManager.Instance?.SetGameState(GameState.OnAltarRestoration);

        int currentGold = InventoryManager.Instance.GetGold()   ;
        goldSlider.maxValue = currentGold;
        goldSlider.minValue = 0;
        goldSlider.wholeNumbers = true;

        goldSlider.value = currentGold;
        goldInputField.text = currentGold.ToString();

        UpdateOptionLabels();
    }

    private void CloseUI()
    {
        isUIOpen = false;
        altarUIPanel.SetActive(false);

        if (GameManager.Instance?.GetCurrentGameState() == GameState.OnAltarRestoration)
            GameManager.Instance.SetGameState(GameState.Digging);
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
        for (int i = 0; i < optionLabels.Length; i++)
        {
            if (i < restorationSystem.OptionCount)
            {
                var opt = restorationSystem.GetOption(i);
                optionLabels[i].text = $"{opt.label}: {opt.restorePercentage}% vida\n{opt.goldCost} oro + 1 {opt.materialRequired}";
            }
            optionButtons[i].interactable = !restorationSystem.HasRestoredToday();
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
}