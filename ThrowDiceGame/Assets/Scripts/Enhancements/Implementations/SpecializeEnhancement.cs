using UnityEngine;
using System.Linq;

/// <summary>
/// Enhancement that doubles the highest face but zeroes the lowest.
/// "Specialize" - higher ceiling, lower floor. More risk, more reward.
/// Pairs well with BoostLowest (fix the 0) or High Roller Tax (massive high face).
/// </summary>
public class SpecializeEnhancement : BaseEnhancement
{
    private void Reset()
    {
        _name = "Specialize";
        _description = "Double the highest face value, but set the lowest face to 0.";
        _maxDiceCount = 1;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        int maxValue = modifiedValues.Max();
        int minValue = modifiedValues.Min();

        // Double the first occurrence of the highest face
        bool doubledMax = false;
        // Zero the first occurrence of the lowest face
        bool zeroedMin = false;

        for (int i = 0; i < modifiedValues.Length; i++)
        {
            if (!doubledMax && modifiedValues[i] == maxValue)
            {
                modifiedValues[i] = maxValue * 2;
                doubledMax = true;
            }
            else if (!zeroedMin && modifiedValues[i] == minValue)
            {
                modifiedValues[i] = 0;
                zeroedMin = true;
            }
        }

        Debug.Log($"[{_name}] Doubled highest ({maxValue} -> {maxValue * 2}), zeroed lowest ({minValue} -> 0).");
        return modifiedValues;
    }
}
