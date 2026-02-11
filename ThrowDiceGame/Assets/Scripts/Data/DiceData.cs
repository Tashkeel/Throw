using System.Collections.Generic;
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

    [Header("Visuals")]
    [SerializeField]
    [Tooltip("The prefab to instantiate when this die is thrown")]
    private Die _diePrefab;

    // Runtime enhancement tracking
    private bool _isEnhanced = false;
    private List<string> _appliedEnhancements = new List<string>();

    public string DisplayName => _displayName;
    public string Description => _description;
    public Die DiePrefab => _diePrefab;
    public bool IsEnhanced => _isEnhanced;
    public IReadOnlyList<string> AppliedEnhancements => _appliedEnhancements;

    /// <summary>
    /// Gets a copy of the face values array.
    /// </summary>
    public int[] GetFaceValues()
    {
        var copy = new int[_faceValues.Length];
        System.Array.Copy(_faceValues, copy, _faceValues.Length);
        return copy;
    }

    private void OnValidate()
    {
        if (_faceValues == null || _faceValues.Length != 6)
        {
            _faceValues = new int[] { 1, 6, 3, 4, 2, 5 };
        }
    }

    /// <summary>
    /// Creates a standard die data at runtime (for default inventory).
    /// </summary>
    public static DiceData CreateStandardDie()
    {
        var data = CreateInstance<DiceData>();
        data._displayName = "Standard Die";
        data._description = "A standard six-sided die.";
        data._faceValues = new int[] { 1, 6, 3, 4, 2, 5 };
        return data;
    }

    /// <summary>
    /// Creates an enhanced copy of this die with the given enhancement applied.
    /// </summary>
    public DiceData CreateEnhancedCopy(IEnhancement enhancement)
    {
        var copy = CreateInstance<DiceData>();
        copy._displayName = _displayName + " +";
        copy._description = _description;
        copy._diePrefab = _diePrefab;
        copy._isEnhanced = true;

        // Copy existing enhancements
        copy._appliedEnhancements = new List<string>(_appliedEnhancements);
        copy._appliedEnhancements.Add(enhancement.Name);

        // Apply enhancement to face values
        copy._faceValues = enhancement.ApplyToDie(GetFaceValues());

        Debug.Log($"Created enhanced die '{copy._displayName}' with {enhancement.Name}");
        return copy;
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

    /// <summary>
    /// Creates a runtime copy of this die data (for inventory instances).
    /// </summary>
    public DiceData CreateRuntimeCopy()
    {
        var copy = CreateInstance<DiceData>();
        copy._displayName = _displayName;
        copy._description = _description;
        copy._faceValues = (int[])_faceValues.Clone();
        copy._diePrefab = _diePrefab;
        copy._isEnhanced = _isEnhanced;
        copy._appliedEnhancements = new List<string>(_appliedEnhancements);
        return copy;
    }
}
