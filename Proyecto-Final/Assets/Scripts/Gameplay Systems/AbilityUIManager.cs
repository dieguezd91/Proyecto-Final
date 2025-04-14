using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUIManager : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    [SerializeField] private Button plantButton;
    [SerializeField] private Button harvestButton;
    [SerializeField] private Button digButton;

    [Header("ICONS")]
    [SerializeField] private Sprite plantIcon;
    [SerializeField] private Sprite harvestIcon;
    [SerializeField] private Sprite digIcon;

    [Header("BUTTONS SETTINGS")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private PlayerAbilitySystem playerAbilitySystem;

    void Start()
    {
        playerAbilitySystem = FindObjectOfType<PlayerAbilitySystem>();

        if (playerAbilitySystem == null)
        {
            return;
        }

        playerAbilitySystem.OnAbilityChanged += OnAbilityChanged;

        SetupButtons();

        UpdateButtonVisuals(playerAbilitySystem.CurrentAbility);
    }

    private void OnDestroy()
    {
        if (playerAbilitySystem != null)
        {
            playerAbilitySystem.OnAbilityChanged -= OnAbilityChanged;
        }
    }

    private void SetupButtons()
    {
        if (plantButton == null || harvestButton == null || digButton == null)
        {
            return;
        }

        AssignButtonEvents();
    }

    private void AssignButtonEvents()
    {
        plantButton.onClick.RemoveAllListeners();
        harvestButton.onClick.RemoveAllListeners();
        digButton.onClick.RemoveAllListeners();

        plantButton.onClick.AddListener(() => playerAbilitySystem.SetAbility(PlayerAbility.Planting));
        harvestButton.onClick.AddListener(() => playerAbilitySystem.SetAbility(PlayerAbility.Harvesting));
        digButton.onClick.AddListener(() => playerAbilitySystem.SetAbility(PlayerAbility.Digging));
    }

    private void OnAbilityChanged(PlayerAbility newAbility)
    {
        UpdateButtonVisuals(newAbility);
    }

    private void UpdateButtonVisuals(PlayerAbility currentAbility)
    {
        // Plantar
        Image plantButtonImage = plantButton.GetComponent<Image>();
        if (plantButtonImage != null)
        {
            plantButtonImage.color = (currentAbility == PlayerAbility.Planting) ? selectedColor : normalColor;
        }

        // Cosechar
        Image harvestButtonImage = harvestButton.GetComponent<Image>();
        if (harvestButtonImage != null)
        {
            harvestButtonImage.color = (currentAbility == PlayerAbility.Harvesting) ? selectedColor : normalColor;
        }

        // Cavar
        Image digButtonImage = digButton.GetComponent<Image>();
        if (digButtonImage != null)
        {
            digButtonImage.color = (currentAbility == PlayerAbility.Digging) ? selectedColor : normalColor;
        }
    }

    void Update()
    {
        bool isDaytime = GameManager.Instance.currentGameState == GameState.Day;

        if (plantButton != null)
            plantButton.interactable = isDaytime;

        if (harvestButton != null)
            harvestButton.interactable = isDaytime;

        if (digButton != null)
            digButton.interactable = isDaytime;
    }
}