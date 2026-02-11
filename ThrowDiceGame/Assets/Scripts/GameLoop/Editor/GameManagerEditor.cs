using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager manager = (GameManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;

        // Game state display
        EditorGUILayout.LabelField("Current State:", manager.CurrentState.ToString());
        EditorGUILayout.LabelField("Current Round:", manager.CurrentRound.ToString());

        if (manager.ScoreTracker != null)
        {
            EditorGUILayout.LabelField("Score:", $"{manager.ScoreTracker.CurrentScore}/{manager.ScoreTracker.ScoreGoal}");
        }

        if (manager.Inventory != null)
        {
            EditorGUILayout.LabelField("Dice in Inventory:", manager.Inventory.TotalDiceCount.ToString());
        }

        EditorGUILayout.Space(5);

        // Action buttons based on state
        switch (manager.CurrentState)
        {
            case GameState.NotStarted:
                if (GUILayout.Button("Start Game", GUILayout.Height(25)))
                {
                    manager.StartGame();
                }
                break;

            case GameState.InShop:
                if (GUILayout.Button("Continue from Shop", GUILayout.Height(25)))
                {
                    manager.ContinueFromShop();
                }
                break;

            case GameState.GameOver:
                if (GUILayout.Button("Restart Game", GUILayout.Height(25)))
                {
                    manager.RequestRestart();
                }
                break;
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use debug controls.", MessageType.Info);
        }
    }
}
