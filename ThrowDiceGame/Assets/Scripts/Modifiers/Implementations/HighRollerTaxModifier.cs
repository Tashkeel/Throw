using UnityEngine;

/// <summary>
/// Modifier that doubles high die values but zeroes low ones.
/// "High Roller Tax" — all-or-nothing scoring on each die.
/// Forces full commitment to upgrading dice or face harsh penalties.
/// </summary>
[CreateAssetMenu(fileName = "MOD_HighRollerTax", menuName = "Dice Game/Modifiers/High Roller Tax")]
public class HighRollerTaxModifier : ModifierData, IPerDieModifier
{
    [Header("High Roller Tax Settings")]
    [SerializeField]
    [Tooltip("Minimum value to qualify for the bonus (inclusive)")]
    private int _highThreshold = 4;

    [SerializeField]
    [Tooltip("Maximum value that gets zeroed (inclusive)")]
    private int _lowThreshold = 2;

    [SerializeField]
    [Tooltip("Multiplier for high-value dice")]
    private int _highMultiplier = 2;

    public override string Name => "High Roller Tax";
    protected override string DefaultDescription => $"Dice showing {_highThreshold}+ score x{_highMultiplier}, but dice showing 1-{_lowThreshold} score 0.";

    public int ApplyPerDie(PerDieContext ctx)
    {
        if (ctx.OriginalValue >= _highThreshold)
        {
            int newScore = ctx.CurrentValue * _highMultiplier;
            Debug.Log($"[High Roller Tax] Die showing {ctx.OriginalValue} (high): {ctx.CurrentValue} -> {newScore}");
            return newScore;
        }

        if (ctx.OriginalValue <= _lowThreshold)
        {
            Debug.Log($"[High Roller Tax] Die showing {ctx.OriginalValue} (low): {ctx.CurrentValue} -> 0");
            return 0;
        }

        return ctx.CurrentValue;
    }
}
