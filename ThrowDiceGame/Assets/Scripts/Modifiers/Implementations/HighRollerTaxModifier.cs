using UnityEngine;

/// <summary>
/// Modifier that doubles high die values but zeroes low ones.
/// "High Roller Tax" - all-or-nothing scoring on each die.
/// Forces full commitment to upgrading dice or face harsh penalties.
/// </summary>
public class HighRollerTaxModifier : BaseModifier
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

    private void Reset()
    {
        _name = "High Roller Tax";
        _description = "Dice showing 4+ score double, but dice showing 1-2 score 0.";
        _timing = ScoreModifierTiming.PerDie;
    }

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
