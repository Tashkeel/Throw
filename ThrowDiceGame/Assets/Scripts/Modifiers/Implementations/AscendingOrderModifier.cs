using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modifier that gives a bonus when all dice show different values.
/// "Ascending Order" - rewards diversity over matching.
/// Conflicts with MatchingBonusModifier and Synergy enhancement.
/// </summary>
public class AscendingOrderModifier : BaseModifier
{
    [Header("Ascending Order Settings")]
    [SerializeField]
    [Tooltip("Score multiplier when all dice show unique values (e.g., 1.5 = +50%)")]
    private float _uniqueMultiplier = 1.5f;

    private void Reset()
    {
        _name = "Ascending Order";
        _description = "If all dice show different values, +50% bonus to throw score.";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.AllDieValues == null || context.AllDieValues.Length < 2)
            return context.CurrentScore;

        // Check if all values are unique
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
