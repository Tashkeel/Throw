using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modifier that gives a bonus when all dice show different values.
/// "Ascending Order" - rewards diversity over matching.
/// Conflicts with MatchingBonusModifier and Synergy enhancement.
/// </summary>
[CreateAssetMenu(fileName = "MOD_AscendingOrder", menuName = "Dice Game/Modifiers/Ascending Order")]
public class AscendingOrderModifier : ModifierData
{
    [Header("Ascending Order Settings")]
    [SerializeField]
    [Tooltip("Score multiplier when all dice show unique values (e.g., 1.5 = +50%)")]
    private float _uniqueMultiplier = 1.5f;

    public override string Name => "Ascending Order";
    protected override string DefaultDescription => $"If all dice show different values, +{(_uniqueMultiplier - 1f) * 100:F0}% bonus to throw score.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.AllDieValues == null || context.AllDieValues.Length < 2)
            return context.CurrentScore;

        var seen = new HashSet<int>();
        bool allUnique = true;
        foreach (int value in context.AllDieValues)
        {
            if (!seen.Add(value))
            {
                allUnique = false;
                break;
            }
        }

        if (allUnique)
        {
            int newScore = Mathf.RoundToInt(context.CurrentScore * _uniqueMultiplier);
            Debug.Log($"[Ascending Order] All dice unique! {context.CurrentScore} x {_uniqueMultiplier} = {newScore}");
            return newScore;
        }

        return context.CurrentScore;
    }
}
