using UnityEngine;

/// <summary>
/// Modifier that multiplies throw score based on how many dice show 1.
/// "Snake Eyes Gambit" — each 1 rolled multiplies the total by 1.5x.
/// Encourages keeping low-value dice with 1s rather than upgrading everything.
/// </summary>
[CreateAssetMenu(fileName = "MOD_SnakeEyesGambit", menuName = "Dice Game/Modifiers/Snake Eyes Gambit")]
public class SnakeEyesGambitModifier : ModifierData, IAfterThrowModifier
{
    [Header("Snake Eyes Settings")]
    [SerializeField]
    [Tooltip("Multiplier per die showing 1 (e.g., 1.5 means 1.5x per 1)")]
    private float _multiplierPerOne = 1.5f;

    public override string Name => "Snake Eyes Gambit";
    protected override string DefaultDescription => $"Each die showing 1 multiplies throw score by {_multiplierPerOne:F1}x.";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        if (ctx.RawDieValues == null || ctx.RawDieValues.Count == 0)
            return ctx.TotalScore;

        int onesCount = 0;
        foreach (int value in ctx.RawDieValues)
        {
            if (value == 1)
                onesCount++;
        }

        if (onesCount == 0)
            return ctx.TotalScore;

        float multiplier = Mathf.Pow(_multiplierPerOne, onesCount);
        int newScore = Mathf.RoundToInt(ctx.TotalScore * multiplier);
        Debug.Log($"[Snake Eyes Gambit] {onesCount} ones rolled! Multiplier: {multiplier:F2}x, Score: {ctx.TotalScore} -> {newScore}");
        return newScore;
    }
}
