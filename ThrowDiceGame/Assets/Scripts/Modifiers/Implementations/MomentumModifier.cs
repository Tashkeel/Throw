using UnityEngine;

/// <summary>
/// Modifier that rewards using all throws in a round.
/// "Momentum" - each consecutive throw scores more than the last.
/// Creates a tension: use all throws for max score, or win early for more money.
/// </summary>
public class MomentumModifier : BaseModifier
{
    [Header("Momentum Settings")]
    [SerializeField]
    [Tooltip("Score bonus percentage per throw after the first (e.g., 0.2 = +20% per throw)")]
    private float _bonusPerThrow = 0.2f;

    private void Reset()
    {
        _name = "Momentum";
        _description = "Each throw scores +20% more than the last. (1st: normal, 2nd: +20%, 3rd: +40%)";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.ThrowNumber <= 1)
            return context.CurrentScore;

        float bonusMultiplier = _bonusPerThrow * (context.ThrowNumber - 1);
        int bonus = Mathf.RoundToInt(context.CurrentScore * bonusMultiplier);
        int newScore = context.CurrentScore + bonus;
        Debug.Log($"[Momentum] Throw {context.ThrowNumber}: +{bonusMultiplier * 100:F0}% bonus = +{bonus}, Score: {context.CurrentScore} -> {newScore}");
        return newScore;
    }
}
