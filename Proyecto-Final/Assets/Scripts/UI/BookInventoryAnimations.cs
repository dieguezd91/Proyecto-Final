using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BookButton
{
    public Button button;
    public string pageName;
    public string buttonName;
}

public class BookInventoryAnimations : MonoBehaviour
{
    [Header("Book Buttons")]
    [SerializeField] private List<BookButton> bookButtons = new List<BookButton>();

    [Header("Animation Controller")]
    [SerializeField] private InventoryAnimationController animationController;

    [Header("Settings")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private InterfaceSoundType buttonClickSound = InterfaceSoundType.MenuButtonClick;

    private string currentPage = "";

    private void Awake()
    {
        CacheAnimationController();
    }

    private void Start()
    {
        SetupButtonListeners();
    }

    private void CacheAnimationController()
    {
        if (animationController == null)
        {
            animationController = GetComponent<InventoryAnimationController>();

            if (animationController == null)
            {
                Debug.LogError($"[BookInventoryAnimations] No se encontró InventoryAnimationController en {gameObject.name}");
                return;
            }
        }
    }

    private void SetupButtonListeners()
    {
        foreach (var bookButton in bookButtons)
        {
            if (bookButton.button != null)
            {
                if (string.IsNullOrEmpty(bookButton.pageName))
                {
                    continue;
                }

                string page = bookButton.pageName;
                string name = bookButton.buttonName;

                bookButton.button.onClick.AddListener(() => OnButtonClicked(page, name));
            }
        }
    }

    private void OnButtonClicked(string pageName, string buttonName)
    {
        if (playClickSound && UIManager.Instance?.InterfaceSounds != null)
        {
            UIManager.Instance.InterfaceSounds.PlaySound(buttonClickSound);
        }

        ChangePage(pageName);
    }

    public void ChangePage(string pageName)
    {
        if (animationController == null)
        {
            return;
        }

        if (currentPage == pageName)
        {
            return;
        }

        animationController.ChangePage(pageName);
        currentPage = pageName;
    }

    public string GetCurrentPage()
    {
        return animationController != null ? animationController.CurrentPage : currentPage;
    }

    public void AddButton(Button button, string pageName, string buttonName = "")
    {
        if (button == null)
        {
            return;
        }

        var bookButton = new BookButton
        {
            button = button,
            pageName = pageName,
            buttonName = string.IsNullOrEmpty(buttonName) ? button.name : buttonName
        };

        bookButtons.Add(bookButton);

        button.onClick.AddListener(() => OnButtonClicked(pageName, bookButton.buttonName));
    }

    private void OnDestroy()
    {
        foreach (var bookButton in bookButtons)
        {
            if (bookButton.button != null)
            {
                bookButton.button.onClick.RemoveAllListeners();
            }
        }
    }

    public void ShowInventory()
    {
        ChangePage("Inventory");
    }

    public void ShowOptions()
    {
        ChangePage("Options");
    }

    public void ShowGlosary()
    {
        ChangePage("Glosary");
    }

    public void ShowCalendar()
    {
        ChangePage("Calendar");
    }

    public void ShowPlaceholder()
    {
        ChangePage("Placeholder");
    }
}