using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnhancementData))]
public class EnhancementDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnhancementData data = (EnhancementData)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Info", EditorStyles.boldLabel);

        if (data.EnhancementPrefab != null)
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("Name", data.DisplayName);
            EditorGUILayout.LabelField("Description");
            EditorGUILayout.TextArea(data.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.TextField("Dice Required", data.RequiredDiceCount.ToString());
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.HelpBox("Assign an enhancement prefab above to see its name and description.", MessageType.Info);
        }
    }
}
