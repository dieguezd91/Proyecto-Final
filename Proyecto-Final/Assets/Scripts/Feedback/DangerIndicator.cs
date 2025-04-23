using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerIndicator : MonoBehaviour
{
    public Transform player;
    public Transform house;
    public RectTransform uiIndicator;
    public Camera mainCamera;
    public float edgeBuffer = 50f;

    private bool isActive = false;

    private void Update()
    {
        if (!isActive) return;

        Vector3 dir = house.position - player.position;
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) / 2f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(player.position + dir.normalized * 10f);

        // Clamp a los bordes
        screenPos.x = Mathf.Clamp(screenPos.x, edgeBuffer, Screen.width - edgeBuffer);
        screenPos.y = Mathf.Clamp(screenPos.y, edgeBuffer, Screen.height - edgeBuffer);
        screenPos.z = 0;

        uiIndicator.position = screenPos;

        // Rotar hacia la casa
        Vector3 dirToHouse = house.position - player.position;
        float angle = Mathf.Atan2(dirToHouse.y, dirToHouse.x) * Mathf.Rad2Deg;
        uiIndicator.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 si el ícono apunta hacia arriba
    }

    public void Activate()
    {
        isActive = true;
        uiIndicator.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        uiIndicator.gameObject.SetActive(false);
    }
}

