using UnityEngine;

/// <summary>
/// Modifier that doubles the value of dice showing odd numbers.
/// "Odd Power" - turns the "bad" low rolls into a strength.
/// Pairs poorly with High Roller Tax (which zeroes 1-2s) but well with DoubleOnes.
/// </summary>
[CreateAssetMenu(fileName = "MOD_OddPower", menuName = "Dice Game/Modifiers/Odd Power")]
public class OddPowerModifier : ModifierData
{
    public override string Name => "Odd Power";
    protected override string DefaultDescription => "Dice showing odd values (1, 3, 5) score double.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.PerDie;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.OriginalValue % 2 != 0)
        {
            int newScore = context.CurrentScore * 2;
            Debug.Log($"[Odd Power] Die {context.DieIndex} shows {context.OriginalValue} (odd) — score doubled: {context.CurrentScore} -> {newScore}");
            return newScore;
        }
        return context.CurrentScore;
    }
}
