using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BookButton
{
    public Button button;
    public string animationState;
}

public class BookInventoryAnimations : MonoBehaviour
{
    [Header("Book Buttons")]
    public List<BookButton> bookButtons = new List<BookButton>();

    [Header("Animator")]
    public Animator animator;

    private void Start()
    {
        foreach (var bookButton in bookButtons)
        {
            if (bookButton.button != null)
            {
                string state = bookButton.animationState;
                bookButton.button.onClick.AddListener(() => ChangeAnimationState(state));
            }
        }
    }

    private void ChangeAnimationState(string animationState)
    {
        if (animator != null)
        {
            animator.Play(animationState);
            Debug.Log($"Animación libro: {animationState}");
        }
        else
        {
            Debug.LogWarning("Animator no asignado.");
        }
    }
}
