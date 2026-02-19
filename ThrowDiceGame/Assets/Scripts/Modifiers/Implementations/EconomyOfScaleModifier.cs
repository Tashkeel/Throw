using UnityEngine;

/// <summary>
/// Modifier that converts excess score into money at round end.
/// "Economy of Scale" - earn $2 for every point scored above the round goal.
/// Encourages overshooting the goal for long-term economic scaling.
/// </summary>
public class EconomyOfScaleModifier : BaseModifier
{
    [Header("Economy of Scale Settings")]
    [SerializeField]
    [Tooltip("Money earned per point of surplus above the score goal")]
    private int _moneyPerSurplusPoint = 2;

    private int _cachedScoreGoal;
    private int _latestCumulativeScore;

    private void Reset()
    {
        _name = "Economy of Scale";
        _description = "Earn $2 for every point scored above the round goal.";
        _timing = ScoreModifierTiming.AfterThrow;
    }

    private void OnEnable()
    {
        GameEvents.OnScoreGoalSet += HandleScoreGoalSet;
        GameEvents.OnRoundStarted += HandleRoundStarted;
        GameEvents.OnScoringCompleted += HandleScoringCompleted;
        GameEvents.OnRoundCompleted += HandleRoundCompleted;
    }

    private void OnDisable()
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
        // This modifier does not alter score; money is awarded via events
        return context.CurrentScore;
    }
}
