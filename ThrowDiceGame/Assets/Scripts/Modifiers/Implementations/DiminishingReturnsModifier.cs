using UnityEngine;

/// <summary>
/// Modifier that penalises later throws, rewarding early round wins.
/// "Diminishing Returns" — each throw after the first scores 10% less.
/// The strategic opposite of Momentum: win quickly or lose value.
/// </summary>
[CreateAssetMenu(fileName = "MOD_DiminishingReturns", menuName = "Dice Game/Modifiers/Diminishing Returns")]
public class DiminishingReturnsModifier : ModifierData, IAfterThrowModifier
{
    [Header("Diminishing Returns Settings")]
    [SerializeField]
    [Tooltip("Score penalty percentage per throw after the first (e.g., 0.1 = -10% per throw)")]
    private float _penaltyPerThrow = 0.1f;

    public override string Name => "Diminishing Returns";
    protected override string DefaultDescription => $"Each throw after the first scores {_penaltyPerThrow * 100:F0}% less. (1st: normal, 2nd: -{_penaltyPerThrow * 100:F0}%, 3rd: -{_penaltyPerThrow * 200:F0}%)";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        if (ctx.ThrowNumber <= 1)
            return ctx.TotalScore;

        float penaltyMultiplier = _penaltyPerThrow * (ctx.ThrowNumber - 1);
        int penalty = Mathf.RoundToInt(ctx.TotalScore * penaltyMultiplier);
        int newScore = ctx.TotalScore - penalty;
        Debug.Log($"[Diminishing Returns] Throw {ctx.ThrowNumber}: -{penaltyMultiplier * 100:F0}% penalty = -{penalty}. Score: {ctx.TotalScore} -> {newScore}");
        return newScore;
    }
}
