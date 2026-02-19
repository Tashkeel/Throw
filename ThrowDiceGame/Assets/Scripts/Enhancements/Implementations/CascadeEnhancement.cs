using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhancement that rewards selecting multiple dice together.
/// "Cascade" - select 3 dice; each die gains +1 per other die in the selection.
/// With 3 dice selected, every die gets +2 to all faces.
/// Weaker per-die than Synergy but applies across 3 dice at once.
/// </summary>
[CreateAssetMenu(fileName = "ENH_Cascade", menuName = "Dice Game/Enhancements/Cascade")]
public class CascadeEnhancement : EnhancementData
{
    [System.NonSerialized]
    private int _bonusPerDie;

    public override string Name => "Cascade";
    protected override string DefaultDescription => "Select 3 dice. Each die gets +1 to all faces per other selected die (+2 each with 3 dice).";
    public override int MaxDiceCount => 3;

    public override void PreProcess(List<int[]> allSelectedDiceFaceValues)
    {
        int count = allSelectedDiceFaceValues?.Count ?? 1;
        _bonusPerDie = count - 1;
        Debug.Log($"[Cascade] {count} dice selected. Each will receive +{_bonusPerDie} to all faces.");
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modified = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
            modified[i] = currentValues[i] + _bonusPerDie;

        Debug.Log($"[Cascade] Applied +{_bonusPerDie} cascade bonus to die.");
        return modified;
    }
}
