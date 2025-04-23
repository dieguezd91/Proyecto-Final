using UnityEngine;

public class CameraOffsetController : MonoBehaviour
{
    private Vector3 offset = Vector3.zero;

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        transform.localPosition = offset;
    }

    public void ResetOffset()
    {
        offset = Vector3.zero;
        transform.localPosition = Vector3.zero;
    }
}