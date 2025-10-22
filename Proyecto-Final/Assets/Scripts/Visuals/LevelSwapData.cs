using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSwapData : MonoBehaviour
{
    [SerializeField] private LevelSwapManager.LevelType _levelType;
    
    [Header("Tilemap Components")]
    public List<TilemapRenderer> TilemapRenderers = new();
    
    [Header("Collider Components")]
    public List<Collider2D> Colliders = new();

    public void SetActive(bool active)
    {
        SetTilemapRenderersActive(active);
        SetCollidersActive(active);
    }

    public void SetTilemapRenderersActive(bool active)
    {
        foreach (var tilemapRenderer in TilemapRenderers)
            if (tilemapRenderer != null) tilemapRenderer.enabled = active;
    }

    public void SetCollidersActive(bool active)
    {
        foreach (var collider in Colliders)
            if (collider != null) collider.enabled = active;
    }

    [ContextMenu("Enable All")]
    private void EnableAll()
    {
        SetActive(true);
        Debug.Log($"[{gameObject.name}] Enabled all tilemap renderers and colliders");
    }

    [ContextMenu("Disable All")]
    private void DisableAll()
    {
        SetActive(false);
        Debug.Log($"[{gameObject.name}] Disabled all tilemap renderers and colliders");
    }

    [ContextMenu("Enable Only Tilemaps")]
    private void EnableOnlyTilemaps()
    {
        SetTilemapRenderersActive(true);
        SetCollidersActive(false);
        Debug.Log($"[{gameObject.name}] Enabled only tilemap renderers");
    }

    [ContextMenu("Enable Only Colliders")]
    private void EnableOnlyColliders()
    {
        SetTilemapRenderersActive(false);
        SetCollidersActive(true);
        Debug.Log($"[{gameObject.name}] Enabled only colliders");
    }

    [ContextMenu("Auto-Fill From Children")]
    private void AutoFillFromChildren()
    {
        TilemapRenderers.Clear();
        Colliders.Clear();

        // Get all tilemap renderers in children
        TilemapRenderer[] childTilemapRenderers = GetComponentsInChildren<TilemapRenderer>(true);
        TilemapRenderers.AddRange(childTilemapRenderers);

        // Get all colliders in children
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>(true);
        Colliders.AddRange(childColliders);

        Debug.Log($"[{gameObject.name}] Auto-filled: {TilemapRenderers.Count} tilemap renderers, {Colliders.Count} colliders");
    }

    [ContextMenu("Remove Null References")]
    private void RemoveNullReferences()
    {
        int removedTilemaps = TilemapRenderers.RemoveAll(renderer => renderer == null);
        int removedColliders = Colliders.RemoveAll(collider => collider == null);
        
        Debug.Log($"[{gameObject.name}] Removed {removedTilemaps} null tilemap renderers and {removedColliders} null colliders");
    }

    // Validation method that can be called in editor
    public bool ValidateReferences()
    {
        bool hasValidReferences = true;
        
        foreach (var renderer in TilemapRenderers)
        {
            if (renderer == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Found null TilemapRenderer reference");
                hasValidReferences = false;
            }
        }
        
        foreach (var collider in Colliders)
        {
            if (collider == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Found null Collider2D reference");
                hasValidReferences = false;
            }
        }
        
        return hasValidReferences;
    }
}