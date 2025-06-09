using UnityEngine;

public class SurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float checkRadius = 0.1f;

    public string DetectSurfaceTag()
    {
        Collider2D hit = Physics2D.OverlapCircle(checkPoint.position, checkRadius);
        if (hit != null)
        {
            Debug.Log("Pisando: " + hit.name + " / Tag: " + hit.tag);
            return hit.tag;
        }
        return "Default";
    }
}
