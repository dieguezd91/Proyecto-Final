using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform mainChar;
    [SerializeField] float followSpeed = 3f;
    [SerializeField] float maxOffsetDistance = 3f;
    [SerializeField] float deadzoneRadius = 2f;

    private Camera cam;


    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        
        Vector3 playerPos = mainChar.position;
        playerPos.z = transform.position.z;

        
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        
        Vector3 toMouse = mouseWorld - playerPos;
        float distToMouse = toMouse.magnitude;

       
        if (distToMouse <= deadzoneRadius)
        {
            transform.position = playerPos;
            return;
        }

        
        Vector3 dir = toMouse.normalized;
        float effectiveDist = Mathf.Min(distToMouse - deadzoneRadius, maxOffsetDistance);
        Vector3 offset = dir * effectiveDist;

        Vector3 targetPos = playerPos + offset;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
