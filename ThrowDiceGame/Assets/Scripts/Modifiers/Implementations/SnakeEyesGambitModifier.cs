using UnityEngine;

/// <summary>
/// Modifier that multiplies throw score based on how many dice show 1.
/// "Snake Eyes Gambit" - each 1 rolled multiplies the total by 1.5x.
/// Encourages keeping low-value dice with 1s rather than upgrading everything.
/// </summary>
[CreateAssetMenu(fileName = "MOD_SnakeEyesGambit", menuName = "Dice Game/Modifiers/Snake Eyes Gambit")]
public class SnakeEyesGambitModifier : ModifierData
{
    [Header("Snake Eyes Settings")]
    [SerializeField]
    [Tooltip("Multiplier per die showing 1 (e.g., 1.5 means 1.5x per 1)")]
    private float _multiplierPerOne = 1.5f;

    public override string Name => "Snake Eyes Gambit";
    protected override string DefaultDescription => $"Each die showing 1 multiplies throw score by {_multiplierPerOne:F1}x.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

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
