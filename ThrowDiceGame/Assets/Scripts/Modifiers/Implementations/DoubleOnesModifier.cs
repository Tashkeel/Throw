using UnityEngine;

/// <summary>
/// Modifier that doubles the score of dice showing 1.
/// "Lucky Ones" — turns the lowest roll into a decent score.
/// </summary>
[CreateAssetMenu(fileName = "MOD_DoubleOnes", menuName = "Dice Game/Modifiers/Double Ones")]
public class DoubleOnesModifier : ModifierData, IPerDieModifier
{
    [Header("Double Ones Settings")]
    [SerializeField]
    private int _multiplier = 2;

    public override string Name => "Lucky Ones";
    protected override string DefaultDescription => "Dice showing 1 are worth double.";

    public int ApplyPerDie(PerDieContext ctx)
    {
        if (ctx.OriginalValue == 1)
        {
            int newScore = ctx.CurrentValue * _multiplier;
            Debug.Log($"[Lucky Ones] Die showing 1: {ctx.CurrentValue} -> {newScore}");
            return newScore;
        }
        return ctx.CurrentValue;
    }
}
