using System;
using UnityEngine;

/// <summary>
/// Tracks score within a round and manages score goals.
/// </summary>
public class ScoreTracker
{
    private int _currentRoundScore;
    private int _scoreGoal;
    private int _currentRound;

    [Header("Difficulty Scaling")]
    private readonly int _baseScoreGoal;
    private readonly int _scoreIncreasePerRound;

    public event Action<int> OnScoreUpdated;
    public event Action<int, int> OnGoalSet; // goal, round

    /// <summary>
    /// Current accumulated score this round.
    /// </summary>
    public int CurrentScore => _currentRoundScore;

    /// <summary>
    /// The score goal to beat this round.
    /// </summary>
    public int ScoreGoal => _scoreGoal;

    /// <summary>
    /// Whether the current score meets or exceeds the goal.
    /// </summary>
    public bool GoalReached => _currentRoundScore >= _scoreGoal;

    /// <summary>
    /// How much score is still needed to reach the goal.
    /// </summary>
    public int ScoreRemaining => Mathf.Max(0, _scoreGoal - _currentRoundScore);

    public ScoreTracker(int baseScoreGoal = 20, int scoreIncreasePerRound = 10)
    {
        _baseScoreGoal = baseScoreGoal;
        _scoreIncreasePerRound = scoreIncreasePerRound;
    }

    /// <summary>
    /// Sets up the score tracker for a new round.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    public void SetupForRound(int roundNumber)
    {
        _currentRound = roundNumber;
        _currentRoundScore = 0;
        _scoreGoal = CalculateGoalForRound(roundNumber);

        OnGoalSet?.Invoke(_scoreGoal, roundNumber);
        GameEvents.RaiseScoreGoalSet(_scoreGoal, roundNumber);

        Debug.Log($"Round {roundNumber}: Score goal is {_scoreGoal}");
    }

    /// <summary>
    /// Adds points to the current round score.
    /// </summary>
    public void AddScore(int points)
    {
        if (points <= 0) return;

        _currentRoundScore += points;
        OnScoreUpdated?.Invoke(_currentRoundScore);
        GameEvents.RaiseScoreChanged(_currentRoundScore);

        Debug.Log($"Score updated: {_currentRoundScore}/{_scoreGoal} (added {points})");
    }

    /// <summary>
    /// Resets the score for a new round (does not change the goal).
    /// </summary>
    public void ResetScore()
    {
        _currentRoundScore = 0;
        OnScoreUpdated?.Invoke(_currentRoundScore);
        GameEvents.RaiseScoreChanged(_currentRoundScore);
    }

    /// <summary>
    /// Calculates the score goal for a specific round.
    /// </summary>
    private int CalculateGoalForRound(int roundNumber)
    {
        // Round 1: baseGoal, Round 2: baseGoal + increase, etc.
        return _baseScoreGoal + (_scoreIncreasePerRound * (roundNumber - 1));
    }

    /// <summary>
    /// Checks if the goal has been reached and logs the result.
    /// </summary>
    public bool CheckGoalReached()
    {
        bool reached = GoalReached;
        if (reached)
        {
            Debug.Log($"Goal reached! Score: {_currentRoundScore}/{_scoreGoal}");
        }
        else
        {
            Debug.Log($"Goal not reached. Score: {_currentRoundScore}/{_scoreGoal}");
        }
        return reached;
    }
}
