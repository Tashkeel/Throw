using UnityEngine;
using System.Linq;

/// <summary>
/// Enhancement that boosts the lowest face value on a die.
/// "Lucky Break" - raises the floor on bad rolls.
/// </summary>
[CreateAssetMenu(fileName = "ENH_BoostLowest", menuName = "Dice Game/Enhancements/Boost Lowest")]
public class BoostLowestEnhancement : EnhancementData
{
    [Header("Boost Settings")]
    [SerializeField]
    [Tooltip("What to set the lowest value to")]
    private int _newMinimumValue = 3;

    public override string Name => "Lucky Break";
    protected override string DefaultDescription => $"Raises the lowest face value to {_newMinimumValue}.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        int minValue = modifiedValues.Min();

        for (int i = 0; i < modifiedValues.Length; i++)
        {
            if (modifiedValues[i] == minValue && modifiedValues[i] < _newMinimumValue)
            {
                modifiedValues[i] = _newMinimumValue;
            }
        }

        Debug.Log($"[Lucky Break] Boosted lowest face values from {minValue} to {_newMinimumValue}.");
        return modifiedValues;
    }
}
