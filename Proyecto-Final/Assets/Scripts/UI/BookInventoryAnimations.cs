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
    private bool listenersSetup = false;

    private void Awake()
    {
        CacheAnimationController();
    }

    private void OnEnable()
    {
        if (!listenersSetup)
        {
            SetupButtonListeners();
        }
    }

    private void Start()
    {
        if (!listenersSetup)
        {
            SetupButtonListeners();
        }
    }

    private void CacheAnimationController()
    {
        if (animationController == null)
        {
            animationController = GetComponent<InventoryAnimationController>();
        }
    }

    private void SetupButtonListeners()
    {
        if (bookButtons == null || bookButtons.Count == 0)
        {
            return;
        }

        int validButtons = 0;
        foreach (var bookButton in bookButtons)
        {
            if (bookButton.button == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(bookButton.pageName))
            {
                continue;
            }

            bookButton.button.onClick.RemoveAllListeners();

            string page = bookButton.pageName;
            string name = bookButton.buttonName;

            bookButton.button.onClick.AddListener(() => OnButtonClicked(page, name));
            validButtons++;
        }

        listenersSetup = true;
    }

    private void OnButtonClicked(string pageName, string buttonName)
    {
        if (animationController == null)
        {
            return;
        }

        if (animationController.IsAnimating)
        {
            return;
        }

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

        button.onClick.RemoveAllListeners();
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

    public void ShowInventory() => ChangePage("Inventory");
    public void ShowOptions() => ChangePage("Options");
    public void ShowGlosary() => ChangePage("Glosary");
    public void ShowCalendar() => ChangePage("Calendar");
    public void ShowPlaceholder() => ChangePage("Placeholder");
}