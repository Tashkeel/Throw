using UnityEngine;
using System;

/// <summary>
/// Enhancement that creates a feast-or-famine die.
/// "Polarize" - bottom 3 faces become 1, top 3 faces get +2.
/// Half the time you roll a 1, half the time you roll very high.
/// Synergizes with Snake Eyes Gambit (more 1s = more multiplier).
/// </summary>
public class PolarizeEnhancement : BaseEnhancement
{
    [Header("Polarize Settings")]
    [SerializeField]
    [Tooltip("Value to set the lowest faces to")]
    private int _lowFaceValue = 1;

    [SerializeField]
    [Tooltip("Bonus added to the highest faces")]
    private int _highFaceBonus = 2;

    private void Reset()
    {
        _name = "Polarize";
        _description = "Set the 3 lowest faces to 1 and add +2 to the 3 highest faces. High risk, high reward.";
        _maxDiceCount = 1;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = (int[])currentValues.Clone();

        // Sort to identify lowest/highest, but track original indices
        int[] indices = new int[modifiedValues.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        // Sort indices by their corresponding values
        Array.Sort((int[])modifiedValues.Clone(), indices);

        // Bottom half → set to low value, Top half → add bonus
        int midpoint = modifiedValues.Length / 2;

        // Sort a copy to determine which indices are low vs high
        int[] sorted = (int[])modifiedValues.Clone();
        Array.Sort(sorted);

        // Build index-to-rank mapping
        int[] sortedIndices = new int[modifiedValues.Length];
        for (int i = 0; i < sortedIndices.Length; i++)
            sortedIndices[i] = i;
        Array.Sort((int[])modifiedValues.Clone(), sortedIndices);

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

        Debug.Log($"[{_name}] Polarized die: bottom {midpoint} faces -> {_lowFaceValue}, top {modifiedValues.Length - midpoint} faces -> +{_highFaceBonus}.");
        return modifiedValues;
    }
}
