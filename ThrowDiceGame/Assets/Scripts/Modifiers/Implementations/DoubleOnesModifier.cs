using UnityEngine;

/// <summary>
/// Sample modifier that doubles the score of dice showing 1.
/// "Lucky Ones" - turns the lowest roll into a decent score.
/// </summary>
public class DoubleOnesModifier : BaseModifier
{
    [Header("Double Ones Settings")]
    [SerializeField]
    private int _multiplier = 2;

    private void Reset()
    {
        _name = "Lucky Ones";
        _description = "Dice showing 1 are worth double.";
        _timing = ScoreModifierTiming.PerDie;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        // Check if this die's original value was 1
        if (context.OriginalValue == 1)
        {
            int newScore = context.CurrentScore * _multiplier;
            Debug.Log($"[Lucky Ones] Die showing 1: {context.CurrentScore} -> {newScore}");
            return newScore;
        }

        return context.CurrentScore;
    }
}
