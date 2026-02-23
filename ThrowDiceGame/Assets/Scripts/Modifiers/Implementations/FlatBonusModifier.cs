using UnityEngine;

/// <summary>
/// Modifier that adds a flat bonus after all dice are scored.
/// "Steady Hand" — consistent bonus to every throw.
/// </summary>
[CreateAssetMenu(fileName = "MOD_FlatBonus", menuName = "Dice Game/Modifiers/Flat Bonus")]
public class FlatBonusModifier : ModifierData, IAfterThrowModifier
{
    [Header("Flat Bonus Settings")]
    [SerializeField]
    private int _bonusAmount = 5;

    public override string Name => "Steady Hand";
    protected override string DefaultDescription => $"Adds +{_bonusAmount} to every throw.";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        int newScore = ctx.TotalScore + _bonusAmount;
        Debug.Log($"[Steady Hand] Throw bonus: {ctx.TotalScore} + {_bonusAmount} = {newScore}");
        return newScore;
    }
}
