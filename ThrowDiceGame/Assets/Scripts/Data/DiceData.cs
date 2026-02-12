using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a type of die with its face values and visual prefab.
/// Can be modified at runtime by enhancements.
/// </summary>
[CreateAssetMenu(fileName = "NewDice", menuName = "Dice Game/Dice Data")]
public class DiceData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string _displayName = "Standard Die";

    [SerializeField]
    [TextArea(2, 4)]
    private string _description = "A standard six-sided die.";

    [Header("Configuration")]
    [SerializeField]
    [Tooltip("The six face values for this die type (+Y, -Y, +X, -X, +Z, -Z)")]
    private int[] _faceValues = new int[] { 1, 6, 3, 4, 2, 5 };

    [SerializeField]
    [Tooltip("The side type for each face (Score or Money). Must match _faceValues length.")]
    private DieSideType[] _faceTypes = new DieSideType[] {
        DieSideType.Score, DieSideType.Score, DieSideType.Score,
        DieSideType.Score, DieSideType.Score, DieSideType.Score
    };

    [Header("Visuals")]
    [SerializeField]
    [Tooltip("The prefab to instantiate when this die is thrown")]
    private GameplayDie _diePrefab;

    // Runtime enhancement tracking
    private bool _isEnhanced = false;

    public string DisplayName => _displayName;
    public string Description => _description;
    public GameplayDie DiePrefab => _diePrefab;
    public bool IsEnhanced => _isEnhanced;

    private void OnValidate()
    {
        if (_faceValues == null || _faceValues.Length != 6)
        {
            _faceValues = new int[] { 1, 6, 3, 4, 2, 5 };
        }

        if (_faceTypes == null || _faceTypes.Length != 6)
        {
            _faceTypes = new DieSideType[] {
                DieSideType.Score, DieSideType.Score, DieSideType.Score,
                DieSideType.Score, DieSideType.Score, DieSideType.Score
            };
        }
    }

    /// <summary>
    /// Creates a standard die data at runtime (for default inventory).
    /// </summary>
    public static InventoryDie CreateDie(DiceData data)
    {
        InventoryDie newDie = new InventoryDie(data._faceValues, data, data._faceTypes);
        return newDie;
    }

    public static DiceData CreateDefaultDie()
    {
        return CreateInstance<DiceData>();
    }

    /// <summary>
    /// Sets face values at runtime (for enhancements).
    /// </summary>
    public void SetFaceValues(int[] newValues)
    {
        if (newValues == null || newValues.Length != 6)
        {
            Debug.LogWarning("Invalid face values array. Must have 6 elements.");
            return;
        }
        _faceValues = (int[])newValues.Clone();
    }
}
