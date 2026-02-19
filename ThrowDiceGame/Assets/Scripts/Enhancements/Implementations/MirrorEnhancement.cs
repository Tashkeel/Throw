using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Enhancement that makes two dice identical copies of the stronger one.
/// "Mirror" - sacrifices the weaker die to clone the stronger.
/// Powerful for MatchingBonusModifier (guaranteed pair potential),
/// but terrible for AscendingOrderModifier (identical dice can't be unique).
/// </summary>
public class MirrorEnhancement : BaseEnhancement
{
    private int[] _bestFaceValues;

    private void Reset()
    {
        _name = "Mirror";
        _description = "Select 2 dice. Both become copies of whichever has the higher total face value.";
        _maxDiceCount = 2;
    }

    public override void PreProcess(List<int[]> allSelectedDiceFaceValues)
    {
        if (allSelectedDiceFaceValues == null || allSelectedDiceFaceValues.Count < 2)
        {
            _bestFaceValues = null;
            return;
        }

        // Find the die with the highest total face value
        int bestSum = int.MinValue;
        _bestFaceValues = null;

        foreach (var faceValues in allSelectedDiceFaceValues)
        {
            int sum = faceValues.Sum();
            if (sum > bestSum)
            {
                bestSum = sum;
                _bestFaceValues = (int[])faceValues.Clone();
            }
        }

        Debug.Log($"[{_name}] Best die has total face value of {bestSum}. Both dice will receive these values.");
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (_bestFaceValues != null)
        {
            Debug.Log($"[{_name}] Mirrored die to match best: [{string.Join(", ", _bestFaceValues)}]");
            return (int[])_bestFaceValues.Clone();
        }

        // Fallback: return unchanged if PreProcess wasn't called
        return currentValues;
    }
}
