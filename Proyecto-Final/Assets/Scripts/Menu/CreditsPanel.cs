using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CreditsPanel : UIControllerBase
{
    [Header("Credits Panel Buttons")]
    [SerializeField] private ImprovedUIButton _goBackButton;

    [Header("Scroll Settings")]
    [SerializeField] private RectTransform _contentToScroll;
    [SerializeField] private float _scrollSpeed = 75f;
    [SerializeField] private float _startPositionY = -500f;
    [SerializeField] private float _limitY;

    [Header("Panel Events")]
    [HideInInspector] public UnityEvent OnGoBackClicked = new();

    private void Start()
    {
        Initialize();
        Setup();
    }

    private void Update()
    {
        if (_contentToScroll != null)
        {
            if (_contentToScroll.anchoredPosition.y < _limitY)
            {
                _contentToScroll.anchoredPosition += Vector2.up * _scrollSpeed * Time.deltaTime;
            }
            else
            {
                Vector2 finalPos = _contentToScroll.anchoredPosition;
                finalPos.y = _limitY;
                _contentToScroll.anchoredPosition = finalPos;
            }
        }
    }

    private void OnEnable()
    {
        ResetPosition();
    }

    public void ResetPosition()
    {
        if (_contentToScroll != null)
        {
            Vector2 newPos = _contentToScroll.anchoredPosition;
            newPos.y = _startPositionY;
            _contentToScroll.anchoredPosition = newPos;
        }
    }

    protected override void CacheReferences() { }

    protected override void SetupEventListeners()
    {
        if (_goBackButton != null)
        {
            _goBackButton.OnClick.AddListener(() => { OnGoBackClicked.Invoke(); });
        }
    }
}