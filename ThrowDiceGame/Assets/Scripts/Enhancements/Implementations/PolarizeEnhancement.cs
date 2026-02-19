using UnityEngine;
using System;

/// <summary>
/// Enhancement that creates a feast-or-famine die.
/// "Polarize" - bottom 3 faces become 1, top 3 faces get +2.
/// Half the time you roll a 1, half the time you roll very high.
/// Synergizes with Snake Eyes Gambit (more 1s = more multiplier).
/// </summary>
[CreateAssetMenu(fileName = "ENH_Polarize", menuName = "Dice Game/Enhancements/Polarize")]
public class PolarizeEnhancement : EnhancementData
{
    [Header("Polarize Settings")]
    [SerializeField]
    [Tooltip("Value to set the lowest faces to")]
    private int _lowFaceValue = 1;

    [SerializeField]
    [Tooltip("Bonus added to the highest faces")]
    private int _highFaceBonus = 2;

    public override string Name => "Polarize";
    protected override string DefaultDescription => $"Set the 3 lowest faces to {_lowFaceValue} and add +{_highFaceBonus} to the 3 highest faces. High risk, high reward.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        int[] sortedIndices = new int[modifiedValues.Length];
        for (int i = 0; i < sortedIndices.Length; i++)
            sortedIndices[i] = i;
        Array.Sort((int[])modifiedValues.Clone(), sortedIndices);

        int midpoint = modifiedValues.Length / 2;

        for (int rank = 0; rank < sortedIndices.Length; rank++)
        {
            int originalIndex = sortedIndices[rank];
            if (rank < midpoint)
            {
                modifiedValues[originalIndex] = _lowFaceValue;
            }
            else
            {
                modifiedValues[originalIndex] += _highFaceBonus;
            }
        }

        Debug.Log($"[Polarize] Polarized die: bottom {midpoint} faces -> {_lowFaceValue}, top {modifiedValues.Length - midpoint} faces -> +{_highFaceBonus}.");
        return modifiedValues;
    }
}
