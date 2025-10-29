using System;
using System.Collections;
using UnityEngine;

public class InventoryAnimationController : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator bookAnimator;

    [Header("Animation State Names (Solo bookSprites layer)")]
    [SerializeField] private string bookOpenStateName = "bookOpen";
    [SerializeField] private string bookCloseStateName = "bookClose";
    [SerializeField] private string bookIdleStateName = "bookIdle";

    [Header("Layer Names")]
    [SerializeField] private string bookSpritesLayerName = "bookSprites";

    [Header("Animation Settings")]
    [SerializeField] private float openAnimationLength = 0.333f;
    [SerializeField] private float closeAnimationLength = 0.333f;

    [Header("Page Activation Timing")]
    [SerializeField][Range(0f, 1f)] private float contentRevealTime = 0.7f;

    [Header("Page References")]
    [SerializeField] private GameObject inventoryPage;
    [SerializeField] private GameObject optionsPage;
    [SerializeField] private GameObject calendarPage;
    [SerializeField] private GameObject glosaryPage;
    [SerializeField] private GameObject placeholderPage;

    private int bookSpritesLayerIndex;
    private bool isAnimating = false;
    private string currentPageName = "";

    public event Action OnOpenAnimationComplete;
    public event Action OnCloseAnimationComplete;
    public event Action OnPageReadyToShow;

    public bool IsAnimating => isAnimating;
    public string CurrentPage => currentPageName;

    private void Awake()
    {
        CacheAnimatorLayer();
        ValidateAnimator();
    }

    private void OnEnable()
    {
        UIEvents.OnInventoryOpened += HandleInventoryOpened;
        UIEvents.OnInventoryClosed += HandleInventoryClosed;
    }

    private void OnDisable()
    {
        UIEvents.OnInventoryOpened -= HandleInventoryOpened;
        UIEvents.OnInventoryClosed -= HandleInventoryClosed;
    }

    private void CacheAnimatorLayer()
    {
        if (bookAnimator == null)
        {
            bookAnimator = GetComponent<Animator>();
        }

        if (bookAnimator != null)
        {
            bookSpritesLayerIndex = bookAnimator.GetLayerIndex(bookSpritesLayerName);

            if (bookSpritesLayerIndex == -1)
            {
                Debug.LogWarning($"[InventoryAnimationController] No se encontró layer '{bookSpritesLayerName}', usando layer 0");
                bookSpritesLayerIndex = 0;
            }
        }
    }

    private void ValidateAnimator()
    {
        if (bookAnimator == null)
        {
            return;
        }
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
        if (bookAnimator == null || isAnimating) return;

        StartCoroutine(CloseAnimationRoutine());
    }

    public void ChangePage(string pageName)
    {
        if (isAnimating)
        {
            return;
        }

        HideAllPages();
        ShowPage(pageName);
    }

    private IEnumerator OpenAnimationRoutine(string pageName)
    {
        isAnimating = true;

        HideAllPages();

        if (bookSpritesLayerIndex != -1)
        {
            bookAnimator.Play(bookOpenStateName, bookSpritesLayerIndex, 0f);
        }

        float delayBeforeReveal = openAnimationLength * contentRevealTime;

        yield return new WaitForSeconds(delayBeforeReveal);

        ShowPage(pageName);
        OnPageReadyToShow?.Invoke();

        float remainingTime = openAnimationLength * (1f - contentRevealTime);
        yield return new WaitForSeconds(remainingTime);

        isAnimating = false;
        OnOpenAnimationComplete?.Invoke();
    }

    private IEnumerator CloseAnimationRoutine()
    {
        isAnimating = true;

        HideAllPages();
        currentPageName = "";

        if (bookSpritesLayerIndex != -1)
        {
            bookAnimator.Play(bookIdleStateName, bookSpritesLayerIndex, 0f);
        }

        yield return new WaitForSeconds(closeAnimationLength);

        isAnimating = false;
        OnCloseAnimationComplete?.Invoke();
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

        switch (pageName)
        {
            case "Inventory":
                if (inventoryPage != null)
                {
                    inventoryPage.SetActive(true);
                }
                break;

            case "Options":
                if (optionsPage != null)
                {
                    optionsPage.SetActive(true);
                }
                break;

            case "Calendar":
                if (calendarPage != null)
                {
                    calendarPage.SetActive(true);
                }
                break;

            case "Glosary":
                if (glosaryPage != null)
                {
                    glosaryPage.SetActive(true);
                }
                break;

            case "Placeholder":
                if (placeholderPage != null)
                {
                    placeholderPage.SetActive(true);
                }
                break;

            default:
                Debug.LogWarning($"[InventoryAnimationController] Página desconocida: {pageName}");
                break;
        }
    }

    public void ForceIdleState()
    {
        if (bookAnimator == null) return;

        if (bookSpritesLayerIndex != -1)
        {
            bookAnimator.Play(bookIdleStateName, bookSpritesLayerIndex, 0f);
        }

        HideAllPages();
        StopAllCoroutines();
        isAnimating = false;
        currentPageName = "";
    }
}