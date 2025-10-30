using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mainChar;
    [SerializeField] private PlayerAbilitySystem abilitySystem;

    [Header("Offset Settings")]
    [SerializeField] private float maxOffsetDistance = 5f;
    [SerializeField] private float deadZoneRadius = 1.5f;
    [SerializeField] private float softZoneRadius = 3f;

    [Header("Speed Settings")]
    [SerializeField] private float softFollowSpeed = 2f;
    [SerializeField] private float hardFollowSpeed = 6f;

    [Header("Ritual Settings")]
    [SerializeField] private float ritualCameraSpeed = 3f;
    [SerializeField] private float ritualZoomOut = 2f;

    [Header("Gizmos Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color deadZoneColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color softZoneColor = new Color(1f, 1f, 0f, 0.3f);
    [SerializeField] private Color maxOffsetColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color directionArrowColor = Color.cyan;
    [SerializeField] private float arrowHeadLength = 0.5f;
    [SerializeField] private float arrowHeadAngle = 20f;

    private Camera cam;
    private Transform ritualTarget;
    private float originalCameraSize;

    private void Start()
    {
        cam = Camera.main;

        if (cam != null)
        {
            originalCameraSize = cam.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (abilitySystem != null && (abilitySystem.IsDigging() || abilitySystem.IsHarvesting()))
            return;

        var state = LevelManager.Instance.currentGameState;

        if (state == GameState.OnRitual)
        {
            HandleRitualCamera();
            return;
        }

        if (cam != null && cam.orthographicSize != originalCameraSize)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalCameraSize, Time.deltaTime * ritualCameraSpeed);
        }

        if (state == GameState.OnInventory || state == GameState.OnCrafting)
            return;

        if (mainChar == null || cam == null) return;

        Vector3 playerPos = mainChar.position;
        playerPos.z = transform.position.z;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = playerPos.z;

        Vector3 toMouse = mouseWorld - playerPos;
        float dist = toMouse.magnitude;

        Vector3 offset = Vector3.zero;
        if (dist > deadZoneRadius)
        {
            float effectiveDist = Mathf.Min(dist - deadZoneRadius, maxOffsetDistance);
            offset = toMouse.normalized * effectiveDist;
        }

        Vector3 targetPos = playerPos + offset;
        targetPos.z = transform.position.z;

        float speed = (dist < softZoneRadius) ? softFollowSpeed : hardFollowSpeed;

        transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
    }

    private void HandleRitualCamera()
    {
        if (ritualTarget == null)
        {
            RitualAltar altar = FindObjectOfType<RitualAltar>();
            if (altar != null)
            {
                ritualTarget = altar.transform;
            }
            else
            {
                ritualTarget = mainChar;
            }
        }

        if (ritualTarget == null || cam == null) return;

        Vector3 targetPos = ritualTarget.position;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, ritualCameraSpeed * Time.deltaTime);

        if (ritualZoomOut > 0f)
        {
            float targetZoom = originalCameraSize + ritualZoomOut;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, ritualCameraSpeed * Time.deltaTime);
        }
    }

    public void ResetRitualTarget()
    {
        ritualTarget = null;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || mainChar == null) return;

        Vector3 playerPos = mainChar.position;

        // Dead zone
        Gizmos.color = deadZoneColor;
        Gizmos.DrawWireSphere(playerPos, deadZoneRadius);

        Gizmos.color = softZoneColor;
        Gizmos.DrawWireSphere(playerPos, softZoneRadius);

        Gizmos.color = maxOffsetColor;
        Gizmos.DrawWireSphere(playerPos, maxOffsetDistance + deadZoneRadius);

        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = playerPos.z;
            Vector3 toMouse = mouseWorld - playerPos;

            if (toMouse.magnitude > deadZoneRadius)
            {
                DrawArrow(playerPos, toMouse.normalized * Mathf.Min(toMouse.magnitude, maxOffsetDistance + deadZoneRadius), directionArrowColor);
            }
        }
    }

    private void DrawArrow(Vector3 start, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Vector3 end = start + direction;
        Gizmos.DrawLine(start, end);

        Vector3 right = Quaternion.Euler(0, 0, arrowHeadAngle) * -direction.normalized * arrowHeadLength;
        Vector3 left = Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction.normalized * arrowHeadLength;

        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
}