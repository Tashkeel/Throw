using System;
using UnityEngine;

/// <summary>
/// Phase where the throw score is tallied and win/continue/lose is determined.
/// </summary>
public class ScoringPhase : IRoundPhase
{
    private readonly ScoreTracker _scoreTracker;
    private readonly int _throwScore;
    private readonly int _currentThrow;
    private readonly int _maxThrows;

    private bool _isComplete;
    private ScoringResult _result;

    public RoundPhase PhaseType => RoundPhase.Scoring;
    public bool IsComplete => _isComplete;

    /// <summary>
    /// The result of the scoring evaluation.
    /// </summary>
    public ScoringResult Result => _result;

    /// <summary>
    /// Event fired when scoring is complete with the result.
    /// </summary>
    public event Action<ScoringResult> OnScoringComplete;

    public ScoringPhase(ScoreTracker scoreTracker, int throwScore, int currentThrow, int maxThrows)
    {
        _scoreTracker = scoreTracker;
        _throwScore = throwScore;
        _currentThrow = currentThrow;
        _maxThrows = maxThrows;
    }

    public void Enter()
    {
        _isComplete = false;

        GameEvents.RaiseScoringStarted(_scoreTracker.CurrentScore);

        // Score is already added progressively during ThrowPhase (per-die + AfterThrow bonus)
        Debug.Log($"Scoring: Throw {_currentThrow}/{_maxThrows}, Throw score {_throwScore}, Total: {_scoreTracker.CurrentScore}/{_scoreTracker.ScoreGoal}");

        // Evaluate result
        EvaluateResult();
    }

    public void Update()
    {
        // Scoring is instant, no update needed
    }

    public void Exit()
    {
        GameEvents.RaiseScoringCompleted(_scoreTracker.CurrentScore, _result == ScoringResult.GoalReached);
        GameEvents.RaiseThrowCompleted(_currentThrow, _scoreTracker.CurrentScore);
    }

    private void EvaluateResult()
    {
        if (_scoreTracker.GoalReached)
        {
            _result = ScoringResult.GoalReached;
            Debug.Log($"GOAL REACHED! Final score: {_scoreTracker.CurrentScore}");
        }
        else if (_currentThrow >= _maxThrows)
        {
            _result = ScoringResult.OutOfThrows;
            Debug.Log($"OUT OF THROWS! Final score: {_scoreTracker.CurrentScore}/{_scoreTracker.ScoreGoal}");
        }
        else
        {
            _result = ScoringResult.Continue;
            Debug.Log($"Continue to throw {_currentThrow + 1}. Need {_scoreTracker.ScoreRemaining} more points.");
        }

        _isComplete = true;
        OnScoringComplete?.Invoke(_result);
    }
}

/// <summary>
/// Result of scoring evaluation.
/// </summary>
public enum ScoringResult
{
    /// <summary>
    /// Player reached the score goal - round won!
    /// </summary>
    GoalReached,

    /// <summary>
    /// Player has more throws remaining.
    /// </summary>
    Continue,

    /// <summary>
    /// Player used all throws without reaching goal - round lost.
    /// </summary>
    OutOfThrows
}
