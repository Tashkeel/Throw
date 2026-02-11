using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopManager))]
public class ShopManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShopManager manager = (ShopManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.LabelField("Shop Open:", manager.IsOpen.ToString());

        EditorGUILayout.Space(5);

        if (manager.IsOpen)
        {
            if (GUILayout.Button("Continue to Next Round", GUILayout.Height(25)))
            {
                manager.Continue();
            }
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use debug controls.", MessageType.Info);
        }
    }
}
