using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modifier that gives a bonus when all dice show different values.
/// "Ascending Order" — rewards diversity over matching.
/// Conflicts with MatchingBonusModifier and Synergy enhancement.
/// </summary>
[CreateAssetMenu(fileName = "MOD_AscendingOrder", menuName = "Dice Game/Modifiers/Ascending Order")]
public class AscendingOrderModifier : ModifierData, IAfterThrowModifier
{
    [Header("Ascending Order Settings")]
    [SerializeField]
    [Tooltip("Score multiplier when all dice show unique values (e.g., 1.5 = +50%)")]
    private float _uniqueMultiplier = 1.5f;

    public override string Name => "Ascending Order";
    protected override string DefaultDescription => $"If all dice show different values, +{(_uniqueMultiplier - 1f) * 100:F0}% bonus to throw score.";

    public int ApplyAfterThrow(AfterThrowContext ctx)
    {
        if (ctx.RawDieValues == null || ctx.RawDieValues.Count < 2)
            return ctx.TotalScore;

        var seen = new HashSet<int>();
        bool allUnique = true;
        foreach (int value in ctx.RawDieValues)
        {
            if (!seen.Add(value))
            {
                allUnique = false;
                break;
            }
        }

        if (allUnique)
        {
            int newScore = Mathf.RoundToInt(ctx.TotalScore * _uniqueMultiplier);
            Debug.Log($"[Ascending Order] All dice unique! {ctx.TotalScore} x {_uniqueMultiplier} = {newScore}");
            return newScore;
        }

        return ctx.TotalScore;
    }
}
