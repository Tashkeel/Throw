using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModifierData))]
public class ModifierDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModifierData data = (ModifierData)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Info", EditorStyles.boldLabel);

        if (data.ModifierPrefab != null)
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("Name", data.DisplayName);
            EditorGUILayout.LabelField("Description");
            EditorGUILayout.TextArea(data.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.TextField("Timing", data.ModifierPrefab.Timing.ToString());
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a modifier prefab above to see its name and description.", MessageType.Info);
        }
    }
}
