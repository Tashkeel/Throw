using UnityEngine;

/// <summary>
/// Modifier that doubles the value of dice showing odd numbers.
/// "Odd Power" — turns the "bad" low rolls into a strength.
/// Pairs poorly with High Roller Tax (which zeroes 1-2s) but well with DoubleOnes.
/// </summary>
[CreateAssetMenu(fileName = "MOD_OddPower", menuName = "Dice Game/Modifiers/Odd Power")]
public class OddPowerModifier : ModifierData, IPerDieModifier
{
    public override string Name => "Odd Power";
    protected override string DefaultDescription => "Dice showing odd values (1, 3, 5) score double.";

    public int ApplyPerDie(PerDieContext ctx)
    {
        if (ctx.OriginalValue % 2 != 0)
        {
            int newScore = ctx.CurrentValue * 2;
            Debug.Log($"[Odd Power] Die {ctx.DieIndex} shows {ctx.OriginalValue} (odd) — score doubled: {ctx.CurrentValue} -> {newScore}");
            return newScore;
        }
        return ctx.CurrentValue;
    }
}
