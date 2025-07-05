using UnityEngine;

public class SpawnPointAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetNightMode(bool isNight)
    {
        if (isNight)
            animator.SetBool("IsNight", true);
        else
            animator.SetBool("IsNight", false);
    }
}