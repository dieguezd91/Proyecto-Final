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

    [Header("Gizmos Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color deadZoneColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color softZoneColor = new Color(1f, 1f, 0f, 0.3f);
    [SerializeField] private Color maxOffsetColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color directionArrowColor = Color.cyan;
    [SerializeField] private float arrowHeadLength = 0.5f;
    [SerializeField] private float arrowHeadAngle = 20f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (abilitySystem != null && (abilitySystem.IsDigging() || abilitySystem.IsHarvesting()))
            return;

        var state = LevelManager.Instance.currentGameState;
        if (state == GameState.OnInventory || state == GameState.OnCrafting || state == GameState.OnRitual)
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


    private void OnDrawGizmos()
    {
        if (!showGizmos || mainChar == null) return;

        Gizmos.color = deadZoneColor;
        Gizmos.DrawWireSphere(mainChar.position, deadZoneRadius);

        Gizmos.color = softZoneColor;
        Gizmos.DrawWireSphere(mainChar.position, softZoneRadius);

        Gizmos.color = maxOffsetColor;
        Gizmos.DrawWireSphere(mainChar.position, deadZoneRadius + maxOffsetDistance);

        if (Application.isPlaying && cam != null)
        {
            Vector3 playerPos = mainChar.position;
            playerPos.z = 0;

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector3 direction = (mouseWorld - playerPos).normalized;
            float distance = Mathf.Min((mouseWorld - playerPos).magnitude, deadZoneRadius + maxOffsetDistance);

            Vector3 endPos = playerPos + direction * distance;

            Gizmos.color = directionArrowColor;
            Gizmos.DrawLine(playerPos, endPos);

            Vector3 right = Quaternion.LookRotation(Vector3.forward, direction) * Quaternion.Euler(0, 0, arrowHeadAngle) * Vector3.down;
            Vector3 left = Quaternion.LookRotation(Vector3.forward, direction) * Quaternion.Euler(0, 0, -arrowHeadAngle) * Vector3.down;
            Gizmos.DrawLine(endPos, endPos + right * arrowHeadLength);
            Gizmos.DrawLine(endPos, endPos + left * arrowHeadLength);
        }
    }
}
