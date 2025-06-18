using UnityEngine;

public class HandAnimatorEvents : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void OnAttackAnimationEnd()
    {
        if (playerController != null)
        {
            playerController.OnAttackAnimationEnd();
        }
    }

    public void CallShoot()
    {
        if (playerController != null)
        {
            playerController.ShootFromHand();
        }
    }
}
