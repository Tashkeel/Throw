using UnityEngine;

/// <summary>
/// Enhancement that halves a die's face values and creates a duplicate.
/// "Split" - trade raw value for more dice in hand.
/// More dice means more combo potential (pairs, matching) but weaker individual rolls.
/// </summary>
public class SplitEnhancement : BaseEnhancement
{
    private void Reset()
    {
        _name = "Split";
        _description = "Halve all face values (rounded up) and create a copy. Two weaker dice instead of one strong one.";
        _maxDiceCount = 1;
    }

    /// <summary>
    /// Signals ShopManager to clone the die after applying.
    /// </summary>
    public override bool CreatesDuplicateDie => true;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            modifiedValues[i] = Mathf.CeilToInt(currentValues[i] / 2f);
        }

        Debug.Log($"[{_name}] Split die: halved all face values. A duplicate will be added to inventory.");
        return modifiedValues;
    }
}
