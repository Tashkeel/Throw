using UnityEngine;

/// <summary>
/// Modifier that multiplies throw score based on how many dice show 1.
/// "Snake Eyes Gambit" - each 1 rolled multiplies the total by 1.5x.
/// Encourages keeping low-value dice with 1s rather than upgrading everything.
/// </summary>
public class SnakeEyesGambitModifier : BaseModifier
{
    [Header("Snake Eyes Settings")]
    [SerializeField]
    [Tooltip("Multiplier per die showing 1 (e.g., 1.5 means 1.5x per 1)")]
    private float _multiplierPerOne = 1.5f;

    private void Reset()
    {
        _name = "Snake Eyes Gambit";
        _description = "Each die showing 1 multiplies throw score by 1.5x.";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.AllDieValues == null || context.AllDieValues.Length == 0)
            return context.CurrentScore;

        int onesCount = 0;
        foreach (int value in context.AllDieValues)
        {
            if (value == 1)
                onesCount++;
        }

        if (onesCount == 0)
            return context.CurrentScore;

        float multiplier = Mathf.Pow(_multiplierPerOne, onesCount);
        int newScore = Mathf.RoundToInt(context.CurrentScore * multiplier);
        Debug.Log($"[Snake Eyes Gambit] {onesCount} ones rolled! Multiplier: {multiplier:F2}x, Score: {context.CurrentScore} -> {newScore}");
        return newScore;
    }
}
