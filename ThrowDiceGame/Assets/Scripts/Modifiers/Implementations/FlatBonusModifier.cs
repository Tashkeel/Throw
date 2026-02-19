using UnityEngine;

/// <summary>
/// Sample modifier that adds a flat bonus after all dice are scored.
/// "Steady Hand" - consistent bonus to every throw.
/// </summary>
[CreateAssetMenu(fileName = "MOD_FlatBonus", menuName = "Dice Game/Modifiers/Flat Bonus")]
public class FlatBonusModifier : ModifierData
{
    [Header("Flat Bonus Settings")]
    [SerializeField]
    private int _bonusAmount = 5;

    public override string Name => "Steady Hand";
    protected override string DefaultDescription => $"Adds +{_bonusAmount} to every throw.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

    public override int ModifyScore(ScoreModifierContext context)
    {
        int newScore = context.CurrentScore + _bonusAmount;
        Debug.Log($"[Steady Hand] Throw bonus: {context.CurrentScore} + {_bonusAmount} = {newScore}");
        return newScore;
    }
}
