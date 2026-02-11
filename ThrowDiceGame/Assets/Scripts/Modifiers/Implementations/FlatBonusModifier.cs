using UnityEngine;

/// <summary>
/// Sample modifier that adds a flat bonus after all dice are scored.
/// "Steady Hand" - consistent bonus to every throw.
/// </summary>
public class FlatBonusModifier : BaseModifier
{
    [Header("Flat Bonus Settings")]
    [SerializeField]
    private int _bonusAmount = 5;

    private void Reset()
    {
        _name = "Steady Hand";
        _description = "Adds +5 to every throw.";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        int newScore = context.CurrentScore + _bonusAmount;
        Debug.Log($"[Steady Hand] Throw bonus: {context.CurrentScore} + {_bonusAmount} = {newScore}");
        return newScore;
    }
}
