using UnityEngine;

public class CloudFade : MonoBehaviour
{
    [Header("REFERENCES")]
    public Animator roofAnimator;

    private bool isInside = false;

    private void Start()
    {
        if (roofAnimator == null)
        {
            Debug.LogError("No se asignó el Animator del techo");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInside)
        {
            isInside = true;
            roofAnimator.SetBool("PlayerInside", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInside)
        {
            isInside = false;
            roofAnimator.SetBool("PlayerInside", false);
        }
    }
}
