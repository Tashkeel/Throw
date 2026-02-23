using UnityEngine;

/// <summary>
/// Modifier that scales the throw score based on the highest die showing.
/// "Wild Card" — rolling a high die multiplies your total score.
/// Huge upside if you roll a 6, risky if all dice roll low.
/// </summary>
[CreateAssetMenu(fileName = "MOD_WildCard", menuName = "Dice Game/Modifiers/Wild Card")]
public class WildCardModifier : ModifierData, IAfterThrowModifier
{
    [Header("Wild Card Settings")]
    [SerializeField]
    [Tooltip("Score multiplier gained per point on the highest die (e.g., 0.1 = +10% per point on highest die)")]
    private float _multiplierPerPoint = 0.1f;

    public override string Name => "Wild Card";
    protected override string DefaultDescription => $"Score multiplied by (1 + highest die / {Mathf.RoundToInt(1f / _multiplierPerPoint)}). A 6 = x{1f + 6 * _multiplierPerPoint:F1}.";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        if (ctx.RawDieValues == null || ctx.RawDieValues.Count == 0)
            return ctx.TotalScore;

        int maxDie = 0;
        foreach (int value in ctx.RawDieValues)
            if (value > maxDie) maxDie = value;

        float multiplier = 1f + maxDie * _multiplierPerPoint;
        int newScore = Mathf.RoundToInt(ctx.TotalScore * multiplier);
        Debug.Log($"[Wild Card] Highest die: {maxDie}, multiplier: {multiplier:F2}x. Score: {ctx.TotalScore} -> {newScore}");
        return newScore;
    }
}
