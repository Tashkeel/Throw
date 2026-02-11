using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DiceThrower))]
public class DiceThrowerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiceThrower thrower = (DiceThrower)target;

        EditorGUILayout.Space(10);

        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Throw Dice", GUILayout.Height(30)))
        {
            thrower.Throw();
        }

        if (GUILayout.Button("Clear Dice"))
        {
            thrower.ClearDice();
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use debug buttons.", MessageType.Info);
        }
    }
}
