using UnityEngine;

/// <summary>
/// Modifier that rewards throwing all high-value dice.
/// "Perfectionist" - if every die shows 4 or higher, earn a flat bonus.
/// Punishes single bad dice. Pairs well with BoostLowest enhancement.
/// </summary>
[CreateAssetMenu(fileName = "MOD_Perfectionist", menuName = "Dice Game/Modifiers/Perfectionist")]
public class PerfectionistModifier : ModifierData
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
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.AllDieValues == null || context.AllDieValues.Length == 0)
            return context.CurrentScore;

        foreach (int value in context.AllDieValues)
        {
            if (value < _threshold)
                return context.CurrentScore;
        }

        int newScore = context.CurrentScore + _bonus;
        Debug.Log($"[Perfectionist] All {context.AllDieValues.Length} dice >= {_threshold}! +{_bonus} bonus. Score: {context.CurrentScore} -> {newScore}");
        return newScore;
    }
}
