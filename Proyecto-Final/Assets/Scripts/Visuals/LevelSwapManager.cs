using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelSwapManager : MonoBehaviour
{
    [Header("Level References")]
    [SerializeField] private LevelSwapData baseLevelData;
    [SerializeField] private LevelSwapData limboLevelData;
    
    [Header("Current State")]
    private LevelType currentLevel = LevelType.Base;
    
    public enum LevelType
    {
        Base,
        Limbo
    }
    
    public LevelType CurrentLevel => currentLevel;
    public LevelSwapData BaseLevelData => baseLevelData;
    public LevelSwapData LimboLevelData => limboLevelData;
    
    private void Start()
    {
        // Initialize with base level active
        SwapToLevel(currentLevel);
    }
    
    public void SwapToBase()
    {
        SwapToLevel(LevelType.Base);
    }
    
    public void SwapToLimbo()
    {
        SwapToLevel(LevelType.Limbo);
    }
    
    public void SwapToLevel(LevelType targetLevel)
    {
        if (currentLevel == targetLevel) return;
        
        // Disable current level
        switch (currentLevel)
        {
            case LevelType.Base:
                if (baseLevelData != null) baseLevelData.SetActive(false);
                break;
            case LevelType.Limbo:
                if (limboLevelData != null) limboLevelData.SetActive(false);
                break;
        }
        
        // Enable target level
        switch (targetLevel)
        {
            case LevelType.Base:
                if (baseLevelData != null) baseLevelData.SetActive(true);
                break;
            case LevelType.Limbo:
                if (limboLevelData != null) limboLevelData.SetActive(true);
                break;
        }
        
        currentLevel = targetLevel;
        
        #if UNITY_EDITOR
        // Mark scene as dirty so changes are saved
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
        #endif
        
        Debug.Log($"[LevelSwapManager] Swapped to {targetLevel} level");
    }
    
    public void ToggleLevel()
    {
        LevelType targetLevel = currentLevel == LevelType.Base ? LevelType.Limbo : LevelType.Base;
        SwapToLevel(targetLevel);
    }
    
    [ContextMenu("Swap to Base")]
    private void ContextSwapToBase()
    {
        SwapToBase();
    }
    
    [ContextMenu("Swap to Limbo")]
    private void ContextSwapToLimbo()
    {
        SwapToLimbo();
    }
    
    [ContextMenu("Toggle Level")]
    private void ContextToggleLevel()
    {
        ToggleLevel();
    }
    
    public bool ValidateReferences()
    {
        bool isValid = true;
        
        if (baseLevelData == null)
        {
            Debug.LogWarning("[LevelSwapManager] Base Level Data reference is missing!");
            isValid = false;
        }
        
        if (limboLevelData == null)
        {
            Debug.LogWarning("[LevelSwapManager] Limbo Level Data reference is missing!");
            isValid = false;
        }
        
        return isValid;
    }
}
