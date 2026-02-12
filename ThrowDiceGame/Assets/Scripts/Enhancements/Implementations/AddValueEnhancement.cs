using UnityEngine;

/// <summary>
/// Enhancement that adds a fixed value to all faces of a die.
/// "Power Up" - makes every roll better.
/// </summary>
public class AddValueEnhancement : BaseEnhancement
{
    [Header("Add Value Settings")]
    [SerializeField]
    [Tooltip("Value to add to each face")]
    private int _valueToAdd = 1;

    private void Reset()
    {
        _name = "Power Up";
        _description = "Adds +1 to all faces of a die.";
        _maxDiceCount = 1;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            modifiedValues[i] = currentValues[i] + _valueToAdd;
        }

        Debug.Log($"[{_name}] Applied +{_valueToAdd} to all faces of die.");
        return modifiedValues;
    }
}
