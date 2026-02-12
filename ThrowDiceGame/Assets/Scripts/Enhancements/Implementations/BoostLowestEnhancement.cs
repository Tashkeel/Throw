using UnityEngine;
using System.Linq;

/// <summary>
/// Enhancement that boosts the lowest face value on a die.
/// "Lucky Break" - raises the floor on bad rolls.
/// </summary>
public class BoostLowestEnhancement : BaseEnhancement
{
    [Header("Boost Settings")]
    [SerializeField]
    [Tooltip("What to set the lowest value to")]
    private int _newMinimumValue = 3;

    private void Reset()
    {
        _name = "Lucky Break";
        _description = "Raises the lowest face value to 3.";
        _maxDiceCount = 1;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        // Find the minimum value
        int minValue = modifiedValues.Min();

        // Raise all instances of the minimum value
        for (int i = 0; i < modifiedValues.Length; i++)
        {
            if (modifiedValues[i] == minValue && modifiedValues[i] < _newMinimumValue)
            {
                modifiedValues[i] = _newMinimumValue;
            }
        }

        Debug.Log($"[{_name}] Boosted lowest face values from {minValue} to {_newMinimumValue}.");
        return modifiedValues;
    }
}
