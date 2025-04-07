using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonPhaseUI : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private Image moonPhaseImage;
    [SerializeField] private Sprite[] moonPhaseSprites = new Sprite[5];

    [Header("Settings")]
    [SerializeField] private bool showMoonPhaseLabel = true;
    [SerializeField] private Color dayColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightColor = Color.white;

    private void Start()
    {
        if (LunarCycleManager.instance != null)
        {
            LunarCycleManager.instance.onMoonPhaseChanged.AddListener(UpdateMoonPhaseUI);

            UpdateMoonPhaseUI(LunarCycleManager.instance.GetCurrentMoonPhase());
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
        int phaseIndex = (int)phase;

        if (moonPhaseImage != null && moonPhaseSprites.Length > phaseIndex)
        {
            moonPhaseImage.sprite = moonPhaseSprites[phaseIndex];
        }
    }

    public void ToggleMoonPhaseLabel(bool show)
    {
        showMoonPhaseLabel = show;
    }
}