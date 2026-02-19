using UnityEngine;

/// <summary>
/// Enhancement that raises all faces by half the die's current peak value.
/// "Apex" - stronger dice grow faster; compounds well with other enhancements.
/// A die with a 6 top-face gains +3 everywhere; a die with a 2 top-face gains only +1.
/// </summary>
[CreateAssetMenu(fileName = "ENH_Apex", menuName = "Dice Game/Enhancements/Apex")]
public class ApexEnhancement : EnhancementData
{
    public override string Name => "Apex";
    protected override string DefaultDescription => "Add half the die's highest face (rounded up) to every face. Better dice benefit more.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int maxVal = 0;
        foreach (int v in currentValues)
            if (v > maxVal) maxVal = v;

        int bonus = Mathf.CeilToInt(maxVal / 2f);
        int[] modified = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
            modified[i] = currentValues[i] + bonus;

        Debug.Log($"[Apex] Highest face was {maxVal}. Added +{bonus} to all faces.");
        return modified;
    }
}
