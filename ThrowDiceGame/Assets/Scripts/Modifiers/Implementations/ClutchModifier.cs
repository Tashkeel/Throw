using UnityEngine;

/// <summary>
/// Modifier that rewards waiting for the final throw.
/// "Clutch" - the last throw of a round scores double.
/// Creates tension: commit early for safety, or push to throw 3 for maximum payoff.
/// </summary>
[CreateAssetMenu(fileName = "MOD_Clutch", menuName = "Dice Game/Modifiers/Clutch")]
public class ClutchModifier : ModifierData
{
    [Header("Clutch Settings")]
    [SerializeField]
    [Tooltip("Score multiplier applied on the final throw")]
    private float _lastThrowMultiplier = 2.0f;

    [SerializeField]
    [Tooltip("Which throw number is considered the final throw")]
    private int _finalThrowNumber = 3;

    public override string Name => "Clutch";
    protected override string DefaultDescription => $"The last throw of a round (throw {_finalThrowNumber}) scores x{_lastThrowMultiplier:F1}. No bonus on earlier throws.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

    public override int ModifyScore(ScoreModifierContext context)
    {
        if (context.ThrowNumber < _finalThrowNumber)
            return context.CurrentScore;

        int bonus = Mathf.RoundToInt(context.CurrentScore * (_lastThrowMultiplier - 1f));
        int newScore = context.CurrentScore + bonus;
        Debug.Log($"[Clutch] Final throw! Score x{_lastThrowMultiplier}: {context.CurrentScore} -> {newScore}");
        return newScore;
    }
}
