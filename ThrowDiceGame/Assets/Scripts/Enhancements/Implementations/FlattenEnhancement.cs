using UnityEngine;

/// <summary>
/// Enhancement that sets all faces to the average value.
/// "Flatten" - eliminates variance for perfectly consistent rolls.
/// Loses high faces but removes low ones. Great for predictable scoring.
/// </summary>
public class FlattenEnhancement : BaseEnhancement
{
    private void Reset()
    {
        _name = "Flatten";
        _description = "Set all faces to the average value (rounded up). Consistent rolls, no surprises.";
        _maxDiceCount = 1;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null || currentValues.Length == 0) return currentValues;

        // Calculate average (rounded up)
        float sum = 0;
        foreach (int value in currentValues)
        {
            sum += value;
        }
        int average = Mathf.CeilToInt(sum / currentValues.Length);

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < modifiedValues.Length; i++)
        {
            modifiedValues[i] = average;
        }

        Debug.Log($"[{_name}] Flattened all faces to {average} (from avg {sum / currentValues.Length:F1}).");
        return modifiedValues;
    }
}
