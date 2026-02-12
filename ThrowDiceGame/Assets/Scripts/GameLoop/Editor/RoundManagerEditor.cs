using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoundManager))]
public class RoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoundManager manager = (RoundManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;

        // Round state display
        EditorGUILayout.LabelField("Round Active:", manager.IsRoundActive.ToString());
        EditorGUILayout.LabelField("Current Phase:", manager.CurrentPhase.ToString());
        EditorGUILayout.LabelField("Current Throw:", $"{manager.CurrentThrow}/{manager.MaxThrows}");

        if (manager.Hand != null)
        {
            EditorGUILayout.LabelField("Hand Size:", $"{manager.Hand.Count}/{manager.Hand.MaxSize}");
        }

        if (manager.ScoreTracker != null)
        {
            EditorGUILayout.LabelField("Round Score:", $"{manager.ScoreTracker.CurrentScore}/{manager.ScoreTracker.ScoreGoal}");
        }

        EditorGUILayout.Space(5);

        // Phase-specific actions
        if (manager.IsRoundActive)
        {
            switch (manager.CurrentPhase)
            {
                case RoundPhase.HandSetup:
                    if (GUILayout.Button("Throw All Dice (Debug)", GUILayout.Height(25)))
                    {
                        // Debug: select all dice in hand and throw
                        var allIndices = new System.Collections.Generic.List<int>();
                        if (manager.Hand != null)
                        {
                            for (int i = 0; i < manager.Hand.Count; i++)
                                allIndices.Add(i);
                        }
                        manager.ConfirmHandSetup(allIndices);
                    }
                    break;
            }
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see debug info.", MessageType.Info);
        }
    }
}
