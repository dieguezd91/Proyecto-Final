using UnityEngine;

public class HandAnimatorEvents : MonoBehaviour
{
    private PlayerSpellController playerSpellController;

    void Start()
    {
        playerSpellController = GetComponentInParent<PlayerSpellController>();
    }

    public void OnAttackAnimationEnd()
    {
        if (playerSpellController != null)
        {
            playerSpellController.OnAttackAnimationEnd();
        }
    }

    public void CallShoot()
    {
        if (playerSpellController != null)
        {
            playerSpellController.ShootFromHand();
        }
    }
}
