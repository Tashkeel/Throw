using UnityEngine;

/// <summary>
/// Modifier that converts excess score into money at round end.
/// "Economy of Scale" - earn $2 for every point scored above the round goal.
/// Encourages overshooting the goal for long-term economic scaling.
/// </summary>
[CreateAssetMenu(fileName = "MOD_EconomyOfScale", menuName = "Dice Game/Modifiers/Economy of Scale")]
public class EconomyOfScaleModifier : ModifierData
{
    [Header("Economy of Scale Settings")]
    [SerializeField]
    [Tooltip("Money earned per point of surplus above the score goal")]
    private int _moneyPerSurplusPoint = 2;

    private int _cachedScoreGoal;
    private int _latestCumulativeScore;

    public override string Name => "Economy of Scale";
    protected override string DefaultDescription => $"Earn ${_moneyPerSurplusPoint} for every point scored above the round goal.";
    public override ScoreModifierTiming Timing => ScoreModifierTiming.AfterThrow;

    public override void OnActivated()
    {
        // Reset per-run state so editor play sessions start clean
        _cachedScoreGoal = 0;
        _latestCumulativeScore = 0;

        GameEvents.OnScoreGoalSet += HandleScoreGoalSet;
        GameEvents.OnRoundStarted += HandleRoundStarted;
        GameEvents.OnScoringCompleted += HandleScoringCompleted;
        GameEvents.OnRoundCompleted += HandleRoundCompleted;
    }

    public override void OnDeactivated()
    {
        GameEvents.OnScoreGoalSet -= HandleScoreGoalSet;
        GameEvents.OnRoundStarted -= HandleRoundStarted;
        GameEvents.OnScoringCompleted -= HandleScoringCompleted;
        GameEvents.OnRoundCompleted -= HandleRoundCompleted;
    }

    private void HandleScoreGoalSet(int goal, int round)
    {
        _cachedScoreGoal = goal;
    }

    private void HandleRoundStarted(int round)
    {
        _latestCumulativeScore = 0;
    }

    private void HandleScoringCompleted(int totalScore, bool goalReached)
    {
        _latestCumulativeScore = totalScore;
    }

    private void HandleRoundCompleted(int round)
    {
        int surplus = _latestCumulativeScore - _cachedScoreGoal;
        if (surplus <= 0) return;

        int moneyEarned = surplus * _moneyPerSurplusPoint;

        var currencyManager = ModifierManager.Instance?.CurrencyManager;
        if (currencyManager != null)
        {
            currencyManager.AddMoney(moneyEarned);
            Debug.Log($"[Economy of Scale] Surplus: {surplus} points above goal. Earned ${moneyEarned}!");
        }
        else
        {
            Debug.LogWarning("[Economy of Scale] CurrencyManager not available. Could not award bonus money.");
        }
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        // Score is not altered; money is awarded via round-completed event
        return context.CurrentScore;
    }
}
