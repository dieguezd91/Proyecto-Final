using UnityEngine;
using UnityEngine.UI;

public class MoonPhaseUI : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    [SerializeField] private Image moonPhaseImage;
    [SerializeField] private Sprite[] moonPhaseSprites = new Sprite[5];

    [Header("SETTINGS")]
    [SerializeField] private bool showMoonPhaseLabel = true;
    [SerializeField] private Color dayColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightColor = Color.white;

    private void Start()
    {
        if (LunarCycleManager.Instance != null)
        {
            LunarCycleManager.Instance.onMoonPhaseChanged.AddListener(UpdateMoonPhaseUI);

            UpdateMoonPhaseUI(LunarCycleManager.Instance.GetCurrentMoonPhase());
        }
    }

    private void Update()
    {
        if (moonPhaseImage != null && GameManager.Instance != null)
        {
            moonPhaseImage.color = GameManager.Instance.currentGameState == GameState.Night ? nightColor : dayColor;
        }
    }

    private void UpdateMoonPhaseUI(MoonPhase phase)
    {
        Debug.Log($"Actualizando UI para fase lunar: {phase}");

        int phaseIndex = (int)phase;
        if (moonPhaseImage != null && moonPhaseSprites.Length > phaseIndex)
        {
            moonPhaseImage.sprite = moonPhaseSprites[phaseIndex];
        }
        else
        {
            if (moonPhaseImage == null)
                Debug.LogError("MoonPhaseUI: moonPhaseImage es null");
            else if (moonPhaseSprites.Length <= phaseIndex)
                Debug.LogError($"MoonPhaseUI: No hay suficientes sprites para la fase {phase}. Array length: {moonPhaseSprites.Length}, Requested index: {phaseIndex}");
        }
    }

    public void ToggleMoonPhaseLabel(bool show)
    {
        showMoonPhaseLabel = show;
    }
}