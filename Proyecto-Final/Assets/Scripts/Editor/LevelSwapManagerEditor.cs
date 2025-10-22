using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSwapManager))]
public class LevelSwapManagerEditor : Editor
{
    private LevelSwapManager levelSwapManager;
    
    private void OnEnable()
    {
        levelSwapManager = (LevelSwapManager)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Current level status
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Current Level Status", EditorStyles.boldLabel);
        
        GUI.enabled = false;
        EditorGUILayout.EnumPopup("Current Level", levelSwapManager.CurrentLevel);
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Level swap buttons
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Level Controls", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Base level button
        GUI.backgroundColor = levelSwapManager.CurrentLevel == LevelSwapManager.LevelType.Base ? Color.green : Color.white;
        if (GUILayout.Button("Swap to Base", GUILayout.Height(30)))
        {
            levelSwapManager.SwapToBase();
            // Force repaint to update UI immediately
            EditorUtility.SetDirty(levelSwapManager);
            SceneView.RepaintAll();
        }
        
        // Limbo level button
        GUI.backgroundColor = levelSwapManager.CurrentLevel == LevelSwapManager.LevelType.Limbo ? Color.green : Color.white;
        if (GUILayout.Button("Swap to Limbo", GUILayout.Height(30)))
        {
            levelSwapManager.SwapToLimbo();
            // Force repaint to update UI immediately
            EditorUtility.SetDirty(levelSwapManager);
            SceneView.RepaintAll();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Toggle button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Toggle Level", GUILayout.Height(25)))
        {
            levelSwapManager.ToggleLevel();
            // Force repaint to update UI immediately
            EditorUtility.SetDirty(levelSwapManager);
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Auto-assign button
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Auto Assignment", EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Find & Assign Level Data from Children", GUILayout.Height(25)))
        {
            AutoAssignLevelDataFromChildren();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndVertical();
        
        // Always repaint to keep UI updated
        Repaint();
    }
    
    private void AutoAssignLevelDataFromChildren()
    {
        LevelSwapData[] levelDataComponents = levelSwapManager.GetComponentsInChildren<LevelSwapData>(true);
        
        SerializedObject serializedObject = new SerializedObject(levelSwapManager);
        SerializedProperty baseProperty = serializedObject.FindProperty("baseLevelData");
        SerializedProperty limboProperty = serializedObject.FindProperty("limboLevelData");
        
        int assignedCount = 0;
        
        foreach (var levelData in levelDataComponents)
        {
            // Get the private _levelType field using reflection
            var levelTypeField = typeof(LevelSwapData).GetField("_levelType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (levelTypeField != null)
            {
                var levelType = (LevelSwapManager.LevelType)levelTypeField.GetValue(levelData);
                
                switch (levelType)
                {
                    case LevelSwapManager.LevelType.Base:
                        if (baseProperty.objectReferenceValue == null)
                        {
                            baseProperty.objectReferenceValue = levelData;
                            assignedCount++;
                            Debug.Log($"[LevelSwapManager] Assigned {levelData.gameObject.name} as Base Level Data");
                        }
                        break;
                    case LevelSwapManager.LevelType.Limbo:
                        if (limboProperty.objectReferenceValue == null)
                        {
                            limboProperty.objectReferenceValue = levelData;
                            assignedCount++;
                            Debug.Log($"[LevelSwapManager] Assigned {levelData.gameObject.name} as Limbo Level Data");
                        }
                        break;
                }
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        
        string message = assignedCount > 0 
            ? $"Successfully assigned {assignedCount} level data reference(s) based on their level types!"
            : "No new assignments made. Either no LevelSwapData components were found, or references are already assigned.";
            
        EditorUtility.DisplayDialog("Auto-Assignment Complete", message, "OK");
    }
}
