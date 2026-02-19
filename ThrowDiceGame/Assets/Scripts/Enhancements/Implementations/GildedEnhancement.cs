using UnityEngine;

/// <summary>
/// Enhancement that doubles the highest single face value.
/// "Gilded" - one guaranteed standout roll, without touching the other faces.
/// Great with Wild Card modifier (pushes that max die higher).
/// </summary>
[CreateAssetMenu(fileName = "ENH_Gilded", menuName = "Dice Game/Enhancements/Gilded")]
public class GildedEnhancement : EnhancementData
{
    public override string Name => "Gilded";
    protected override string DefaultDescription => "The highest face value is doubled. Guarantees one outstanding roll.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        int[] modified = (int[])currentValues.Clone();

        int maxIndex = 0;
        for (int i = 1; i < modified.Length; i++)
            if (modified[i] > modified[maxIndex]) maxIndex = i;

        int oldMax = modified[maxIndex];
        modified[maxIndex] = oldMax * 2;
        Debug.Log($"[Gilded] Gilded highest face: {oldMax} -> {modified[maxIndex]}.");
        return modified;
    }
}
