using UnityEngine;

/// <summary>
/// Enhancement that tightens a die's variance without changing its average much.
/// "Temper" - faces below 3 get +2, faces above 5 get -1.
/// Trades high-roll ceiling for a more reliable floor.
/// </summary>
[CreateAssetMenu(fileName = "ENH_Temper", menuName = "Dice Game/Enhancements/Temper")]
public class TemperEnhancement : EnhancementData
{
    [Header("Temper Settings")]
    [SerializeField]
    [Tooltip("Faces strictly below this value receive the low bonus")]
    private int _lowThreshold = 3;

    [SerializeField]
    [Tooltip("Bonus added to faces below the low threshold")]
    private int _lowBonus = 2;

    [SerializeField]
    [Tooltip("Faces strictly above this value receive the high penalty")]
    private int _highThreshold = 5;

    [SerializeField]
    [Tooltip("Amount subtracted from faces above the high threshold")]
    private int _highPenalty = 1;

    public override string Name => "Temper";
    protected override string DefaultDescription => $"Faces below {_lowThreshold} get +{_lowBonus}; faces above {_highThreshold} get -{_highPenalty}. More consistent rolls at the cost of peak variance.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modified = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            int v = currentValues[i];
            if (v < _lowThreshold)
                modified[i] = v + _lowBonus;
            else if (v > _highThreshold)
                modified[i] = v - _highPenalty;
            else
                modified[i] = v;
        }

        Debug.Log($"[Temper] Tempered die — raised low faces by +{_lowBonus}, trimmed high faces by -{_highPenalty}.");
        return modified;
    }
}
