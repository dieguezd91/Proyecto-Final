using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform mainChar;
    [SerializeField] float vel = 5f;

    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 playerPos = mainChar.position;
        playerPos.z = 0f;
        Vector3 midPoint = (playerPos + mouseWorld) / 2f;
        midPoint.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, midPoint, vel * Time.deltaTime);
    }
}
