using UnityEngine;

/// <summary>
/// Modifier that doubles high die values but zeroes low ones.
/// "High Roller Tax" - all-or-nothing scoring on each die.
/// Forces full commitment to upgrading dice or face harsh penalties.
/// </summary>
[CreateAssetMenu(fileName = "MOD_HighRollerTax", menuName = "Dice Game/Modifiers/High Roller Tax")]
public class HighRollerTaxModifier : ModifierData
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
    public override ScoreModifierTiming Timing => ScoreModifierTiming.PerDie;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.OriginalValue >= _highThreshold)
        {
            int newScore = context.CurrentScore * _highMultiplier;
            Debug.Log($"[High Roller Tax] Die showing {context.OriginalValue} (high): {context.CurrentScore} -> {newScore}");
            return newScore;
        }

        if (context.OriginalValue <= _lowThreshold)
        {
            Debug.Log($"[High Roller Tax] Die showing {context.OriginalValue} (low): {context.CurrentScore} -> 0");
            return 0;
        }

        return context.CurrentScore;
    }
}
