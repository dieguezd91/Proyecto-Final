using System;
using System.Collections;
using UnityEngine;

public class InventoryAnimationController : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator bookAnimator;

    [Header("Animation State Names")]
    [SerializeField] private string bookOpenStateName = "bookOpen";
    [SerializeField] private string bookCloseStateName = "bookClose";
    [SerializeField] private string pageFlipStateName = "pageFlip";

    [Header("Page References")]
    [SerializeField] private GameObject inventoryPage;
    [SerializeField] private GameObject optionsPage;
    [SerializeField] private GameObject calendarPage;
    [SerializeField] private GameObject glosaryPage;
    [SerializeField] private GameObject placeholderPage;

    [Header("UI Elements")]
    [SerializeField] private GameObject[] bookButtons;

    private bool isAnimating = false;
    private string currentPageName = "";
    private string pendingPageName = "";

    public event Action OnOpenAnimationComplete;
    public event Action OnCloseAnimationComplete;
    public event Action OnPageReadyToShow;

    public bool IsAnimating => isAnimating;
    public string CurrentPage => currentPageName;

    private void Awake()
    {
        if (bookAnimator == null)
        {
            bookAnimator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        UIEvents.OnInventoryOpened += HandleInventoryOpened;
        UIEvents.OnInventoryClosed += HandleInventoryClosed;
        UIEvents.OnPauseMenuRequested += HandlePauseMenuRequested;
        UIEvents.OnPauseMenuClosed += HandlePauseMenuClosed;
    }

    private void OnDisable()
    {
        UIEvents.OnInventoryOpened -= HandleInventoryOpened;
        UIEvents.OnInventoryClosed -= HandleInventoryClosed;
        UIEvents.OnPauseMenuRequested -= HandlePauseMenuRequested;
        UIEvents.OnPauseMenuClosed -= HandlePauseMenuClosed;
    }

    private void HandleInventoryOpened()
    {
        OpenWithPage("Inventory");
    }

    private void HandleInventoryClosed()
    {
        CloseBook();
    }

    public void OpenWithPage(string pageName)
    {
        if (bookAnimator == null || isAnimating)
        {
            return;
        }

        StartCoroutine(OpenAnimationRoutine(pageName));
    }

    public void CloseBook()
    {
        if (bookAnimator == null || isAnimating)
            return;

        StartCoroutine(CloseAnimationRoutine());
    }

    public void ChangePage(string pageName)
    {
        if (isAnimating || currentPageName == pageName)
        {
            return;
        }

        StartCoroutine(PageFlipAnimationRoutine(pageName));
    }

    private IEnumerator OpenAnimationRoutine(string pageName)
    {
        isAnimating = true;
        pendingPageName = pageName;

        SetUIElementsVisibility(false);
        HideAllPages();

        UIManager.Instance?.InterfaceSounds?.PlaySound(InterfaceSoundType.GameInventoryBookOpen);

        bookAnimator.Play(bookOpenStateName, 0, 0f);

        yield return WaitForAnimationToComplete(bookOpenStateName);

        SetUIElementsVisibility(true);

        isAnimating = false;
        pendingPageName = "";
        OnOpenAnimationComplete?.Invoke();
    }

    private IEnumerator CloseAnimationRoutine()
    {
        isAnimating = true;

        SetUIElementsVisibility(false);
        HideAllPages();
        currentPageName = "";

        UIManager.Instance?.InterfaceSounds?.PlaySound(InterfaceSoundType.GameInventoryBookClose);

        bookAnimator.Play(bookCloseStateName, 0, 0f);

        yield return WaitForAnimationToComplete(bookCloseStateName);

        isAnimating = false;
        OnCloseAnimationComplete?.Invoke();
    }

    private IEnumerator PageFlipAnimationRoutine(string newPageName)
    {
        isAnimating = true;
        pendingPageName = newPageName;

        UIManager.Instance?.InterfaceSounds?.PlaySound(InterfaceSoundType.MenuButtonClick);

        bookAnimator.Play(pageFlipStateName, 0, 0f);

        yield return WaitForAnimationToComplete(pageFlipStateName);

        isAnimating = false;
        pendingPageName = "";
    }

    private IEnumerator WaitForAnimationToComplete(string animationStateName)
    {
        yield return null;

        while (true)
        {
            AnimatorStateInfo stateInfo = bookAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(animationStateName))
            {
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    break;
                }
            }

            yield return null;
        }
    }

    public void OnBookOpenShowContent()
    {
        if (!string.IsNullOrEmpty(pendingPageName))
        {
            ShowPage(pendingPageName);
            OnPageReadyToShow?.Invoke();
        }
    }

    public void OnPageFlipHideOldPage()
    {
        HideAllPages();
    }

    public void OnPageFlipShowNewPage()
    {
        if (!string.IsNullOrEmpty(pendingPageName))
        {
            ShowPage(pendingPageName);
        }
    }

    private void HideAllPages()
    {
        if (inventoryPage != null) inventoryPage.SetActive(false);
        if (optionsPage != null) optionsPage.SetActive(false);
        if (calendarPage != null) calendarPage.SetActive(false);
        if (glosaryPage != null) glosaryPage.SetActive(false);
        if (placeholderPage != null) placeholderPage.SetActive(false);
    }

    private void ShowPage(string pageName)
    {
        currentPageName = pageName;
        HideAllPages();

        switch (pageName)
        {
            case "Inventory":
                if (inventoryPage != null) inventoryPage.SetActive(true);
                break;

            case "Options":
                if (optionsPage != null) optionsPage.SetActive(true);
                break;

            case "Calendar":
                if (calendarPage != null) calendarPage.SetActive(true);
                break;

            case "Glosary":
                if (glosaryPage != null) glosaryPage.SetActive(true);
                break;

            case "Placeholder":
                if (placeholderPage != null) placeholderPage.SetActive(true);
                break;
        }
    }

    private void SetUIElementsVisibility(bool visible)
    {
        if (bookButtons == null || bookButtons.Length == 0)
            return;

        foreach (GameObject element in bookButtons)
        {
            if (element != null)
            {
                element.SetActive(visible);
            }
        }
    }

    private void HandlePauseMenuRequested()
    {
        OpenWithPage("Options");
    }

    private void HandlePauseMenuClosed()
    {
        CloseBook();
    }
}