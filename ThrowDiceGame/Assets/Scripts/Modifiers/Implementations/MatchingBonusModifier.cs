using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sample modifier that gives bonus points for matching dice.
/// "Pair Master" - rewards rolling pairs, triples, etc.
/// </summary>
public class MatchingBonusModifier : BaseModifier
{
    [Header("Matching Bonus Settings")]
    [SerializeField]
    [Tooltip("Bonus per pair (2 matching dice)")]
    private int _pairBonus = 3;

    [SerializeField]
    [Tooltip("Bonus per triple (3 matching dice)")]
    private int _tripleBonus = 10;

    [SerializeField]
    [Tooltip("Bonus for 4+ matching dice")]
    private int _quadBonus = 25;

    private void Reset()
    {
        _name = "Pair Master";
        _description = "Bonus points for matching dice: +3 per pair, +10 per triple, +25 for 4+.";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.AllDieValues == null || context.AllDieValues.Length < 2)
            return context.CurrentScore;

        // Count occurrences of each value
        var valueCounts = new Dictionary<int, int>();
        foreach (int value in context.AllDieValues)
        {
            if (valueCounts.ContainsKey(value))
                valueCounts[value]++;
            else
                valueCounts[value] = 1;
        }

        // Calculate bonus based on matches
        int totalBonus = 0;
        foreach (var kvp in valueCounts)
        {
            int count = kvp.Value;
            if (count >= 4)
            {
                totalBonus += _quadBonus;
                Debug.Log($"[Pair Master] Quad+ of {kvp.Key}s! +{_quadBonus}");
            }
            else if (count == 3)
            {
                totalBonus += _tripleBonus;
                Debug.Log($"[Pair Master] Triple {kvp.Key}s! +{_tripleBonus}");
            }
            else if (count == 2)
            {
                totalBonus += _pairBonus;
                Debug.Log($"[Pair Master] Pair of {kvp.Key}s! +{_pairBonus}");
            }
        }

        if (totalBonus > 0)
        {
            Debug.Log($"[Pair Master] Total matching bonus: +{totalBonus}");
        }

        return context.CurrentScore + totalBonus;
    }
}
