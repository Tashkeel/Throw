using UnityEngine;

/// <summary>
/// Modifier that converts excess score into money at round end.
/// "Economy of Scale" — earn $2 for every point scored above the round goal.
/// Encourages overshooting the goal for long-term economic scaling.
/// Uses IModifierContext to receive the CurrencyManager without touching singletons.
/// </summary>
[CreateAssetMenu(fileName = "MOD_EconomyOfScale", menuName = "Dice Game/Modifiers/Economy of Scale")]
public class EconomyOfScaleModifier : ModifierData, IAfterThrowModifier
{
    [Header("Economy of Scale Settings")]
    [SerializeField]
    [Tooltip("Money earned per point of surplus above the score goal")]
    private int _moneyPerSurplusPoint = 2;

    private IModifierContext _context;
    private int _cachedScoreGoal;
    private int _latestCumulativeScore;

    public override string Name => "Economy of Scale";
    protected override string DefaultDescription => $"Earn ${_moneyPerSurplusPoint} for every point scored above the round goal.";

    public override void OnEquipped(IModifierContext ctx)
    {
        _context = ctx;
        _cachedScoreGoal = 0;
        _latestCumulativeScore = 0;

        GameEvents.OnScoreGoalSet += HandleScoreGoalSet;
        GameEvents.OnRoundStarted += HandleRoundStarted;
        GameEvents.OnScoringCompleted += HandleScoringCompleted;
        GameEvents.OnRoundCompleted += HandleRoundCompleted;
    }

    public override void OnUnequipped()
    {
        _context = null;

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

        if (_context?.Currency != null)
        {
            _context.Currency.AddMoney(moneyEarned);
            Debug.Log($"[Economy of Scale] Surplus: {surplus} points above goal. Earned ${moneyEarned}!");
        }
        else
        {
            Debug.LogWarning("[Economy of Scale] CurrencyManager not available via context. Could not award bonus money.");
        }
    }

    // Score is not altered; money is awarded via the round-completed event
    public int ApplyAfterThrow(AfterThrowContext ctx) => ctx.TotalScore;
}
