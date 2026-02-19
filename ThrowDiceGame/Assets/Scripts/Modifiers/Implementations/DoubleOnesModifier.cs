using UnityEngine;

/// <summary>
/// Sample modifier that doubles the score of dice showing 1.
/// "Lucky Ones" - turns the lowest roll into a decent score.
/// </summary>
[CreateAssetMenu(fileName = "MOD_DoubleOnes", menuName = "Dice Game/Modifiers/Double Ones")]
public class DoubleOnesModifier : ModifierData
{
    [Header("Double Ones Settings")]
    [SerializeField]
    private int _multiplier = 2;

    public override string Name => "Lucky Ones";
    protected override string DefaultDescription => "Dice showing 1 are worth double.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.PerDie;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.OriginalValue == 1)
        {
            int newScore = context.CurrentScore * _multiplier;
            Debug.Log($"[Lucky Ones] Die showing 1: {context.CurrentScore} -> {newScore}");
            return newScore;
        }

        return context.CurrentScore;
    }
}
