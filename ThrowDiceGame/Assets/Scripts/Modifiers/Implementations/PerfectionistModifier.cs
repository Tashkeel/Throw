using UnityEngine;

/// <summary>
/// Modifier that rewards throwing all high-value dice.
/// "Perfectionist" — if every die shows 4 or higher, earn a flat bonus.
/// Punishes single bad dice. Pairs well with BoostLowest enhancement.
/// </summary>
[CreateAssetMenu(fileName = "MOD_Perfectionist", menuName = "Dice Game/Modifiers/Perfectionist")]
public class PerfectionistModifier : ModifierData, IAfterThrowModifier
{
    [Header("Perfectionist Settings")]
    [SerializeField]
    [Tooltip("Minimum face value every die must show to trigger the bonus")]
    private int _threshold = 4;

    [SerializeField]
    [Tooltip("Flat bonus added when all dice meet the threshold")]
    private int _bonus = 30;

    public override string Name => "Perfectionist";
    protected override string DefaultDescription => $"If every die shows {_threshold} or higher, earn +{_bonus} bonus points on that throw.";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        if (ctx.RawDieValues == null || ctx.RawDieValues.Count == 0)
            return ctx.TotalScore;

        foreach (int value in ctx.RawDieValues)
        {
            if (value < _threshold)
                return ctx.TotalScore;
        }

        int newScore = ctx.TotalScore + _bonus;
        Debug.Log($"[Perfectionist] All {ctx.RawDieValues.Count} dice >= {_threshold}! +{_bonus} bonus. Score: {ctx.TotalScore} -> {newScore}");
        return newScore;
    }
}
