using UnityEngine;
using System.Linq;

/// <summary>
/// Enhancement that doubles the highest face but zeroes the lowest.
/// "Specialize" - higher ceiling, lower floor. More risk, more reward.
/// Pairs well with BoostLowest (fix the 0) or High Roller Tax (massive high face).
/// </summary>
[CreateAssetMenu(fileName = "ENH_Specialize", menuName = "Dice Game/Enhancements/Specialize")]
public class SpecializeEnhancement : EnhancementData
{
    public override string Name => "Specialize";
    protected override string DefaultDescription => "Double the highest face value, but set the lowest face to 0.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        int maxValue = modifiedValues.Max();
        int minValue = modifiedValues.Min();

        bool doubledMax = false;
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

        Debug.Log($"[Specialize] Doubled highest ({maxValue} -> {maxValue * 2}), zeroed lowest ({minValue} -> 0).");
        return modifiedValues;
    }
}
