#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryManager))]
public class ResourceInventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        //ResourceInventory inventory = (ResourceInventory)target;

        //EditorGUILayout.Space(10);
        //EditorGUILayout.LabelField("MATERIALS DICTIONARY", EditorStyles.boldLabel);

        //if (inventory.materialsDictionary != null)
        //{
        //    foreach (var kvp in inventory.materials)
        //    {
        //        EditorGUILayout.BeginHorizontal();
        //        EditorGUILayout.LabelField(kvp.Key.ToString(), GUILayout.Width(150));
        //        EditorGUILayout.LabelField(kvp.Value.ToString(), GUILayout.Width(50));
        //        EditorGUILayout.EndHorizontal();
        //    }
        //}
        //else
        //{
        //    EditorGUILayout.HelpBox("El diccionario está vacío o es null.", MessageType.Info);
        //}
    }
}
#endif