using System.Linq;
using UnityEngine;

/// <summary>
/// Enhancement that makes two dice identical copies of the stronger one.
/// "Mirror" — sacrifices the weaker die to clone the stronger.
/// Powerful for MatchingBonusModifier (guaranteed pair potential),
/// but terrible for AscendingOrderModifier (identical dice can't be unique).
/// </summary>
[CreateAssetMenu(fileName = "ENH_Mirror", menuName = "Dice Game/Enhancements/Mirror")]
public class MirrorEnhancement : MultiDiceEnhancementData
{
    [System.NonSerialized]
    private int[] _bestFaceValues;

    public override string Name => "Mirror";
    protected override string DefaultDescription => "Select 2 dice. Both become copies of whichever has the higher total face value.";
    public override int MaxDiceCount => 2;

    protected override void OnPrepared(int[][] allValues)
    {
        if (allValues == null || allValues.Length < 2)
        {
            _bestFaceValues = null;
            return;
        }

        int bestSum = int.MinValue;
        _bestFaceValues = null;

        foreach (var faceValues in allValues)
        {
            int sum = faceValues.Sum();
            if (sum > bestSum)
            {
                bestSum = sum;
                _bestFaceValues = (int[])faceValues.Clone();
            }
        }

        Debug.Log($"[Mirror] Best die has total face value of {bestSum}. Both dice will receive these values.");
    }

    protected override int[] ApplyWithContext(int[] currentValues)
    {
        if (_bestFaceValues != null)
        {
            Debug.Log($"[Mirror] Mirrored die to match best: [{string.Join(", ", _bestFaceValues)}]");
            return (int[])_bestFaceValues.Clone();
        }
        return currentValues;
    }
}
