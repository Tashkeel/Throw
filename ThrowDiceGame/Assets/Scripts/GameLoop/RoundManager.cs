using System;
using UnityEngine;

/// <summary>
/// Manages the flow of a single round, including all phases and throw tracking.
/// </summary>
public class RoundManager : MonoBehaviour
{
    [Header("Round Configuration")]
    [SerializeField]
    [Tooltip("Maximum throws allowed per round")]
    private int _maxThrows = 3;

    [SerializeField]
    [Tooltip("Maximum hand size (dice drawn per round)")]
    private int _handSize = 5;

    [SerializeField]
    [Tooltip("Number of discard actions allowed per round")]
    private int _discardsPerRound = 1;

    [SerializeField]
    [Tooltip("Maximum dice that can be discarded per action")]
    private int _maxDiscardCount = 5;

    [Header("References")]
    [SerializeField]
    private DiceThrower _diceThrower;

    [SerializeField]
    private DiceManager _diceManager;

    private DiceInventory _inventory;
    private Hand _hand;
    private ScoreTracker _scoreTracker;

    private IRoundPhase _currentPhase;
    private int _currentThrow;
    private int _currentRound;
    private bool _roundActive;
    private int _discardsRemainingThisRound;

    // Phase instances (reused)
    private HandSetupPhase _handSetupPhase;
    private ThrowPhase _throwPhase;
    private ScoringPhase _scoringPhase;

    // Scoring state - waits for UI confirmation
    private ScoringResult? _pendingScoringResult;
    private bool _waitingForScoringConfirmation;

    /// <summary>
    /// Current phase of the round.
    /// </summary>
    public RoundPhase CurrentPhase => _currentPhase?.PhaseType ?? RoundPhase.None;

    /// <summary>
    /// Current throw number (1-based).
    /// </summary>
    public int CurrentThrow => _currentThrow;

    /// <summary>
    /// Maximum throws per round.
    /// </summary>
    public int MaxThrows => _maxThrows;

    /// <summary>
    /// Maximum hand size (dice per round).
    /// </summary>
    public int HandSize => _handSize;

    /// <summary>
    /// Whether a round is currently active.
    /// </summary>
    public bool IsRoundActive => _roundActive;

    /// <summary>
    /// The player's hand for this round.
    /// </summary>
    public Hand Hand => _hand;

    /// <summary>
    /// The score tracker for this round.
    /// </summary>
    public ScoreTracker ScoreTracker => _scoreTracker;

    /// <summary>
    /// Event fired when the round ends.
    /// </summary>
    public event Action<bool> OnRoundEnded; // true = won, false = lost

    /// <summary>
    /// Provides access to the hand setup phase for UI interaction.
    /// </summary>
    public HandSetupPhase HandSetupPhase => _handSetupPhase;

    /// <summary>
    /// Initializes the round manager with external dependencies.
    /// </summary>
    public void Initialize(DiceInventory inventory, ScoreTracker scoreTracker)
    {
        _inventory = inventory;
        _scoreTracker = scoreTracker;
        _hand = new Hand(inventory, _handSize);
    }

    private void Update()
    {
        if (!_roundActive || _currentPhase == null) return;

        _currentPhase.Update();

        if (_currentPhase.IsComplete)
        {
            TransitionToNextPhase();
        }
    }

    /// <summary>
    /// Starts a new round.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    public void StartRound(int roundNumber)
    {
        _currentRound = roundNumber;
        _currentThrow = 0;
        _roundActive = true;
        _discardsRemainingThisRound = _discardsPerRound;

        // Setup score goal for this round
        _scoreTracker.SetupForRound(roundNumber);

        // Draw initial hand
        _hand.DrawToFull();

        GameEvents.RaiseRoundStarted(roundNumber);
        Debug.Log($"=== ROUND {roundNumber} START === Goal: {_scoreTracker.ScoreGoal}");

        // Start first throw cycle
        StartThrowCycle();
    }

    /// <summary>
    /// Starts a throw cycle (hand setup -> throw -> scoring).
    /// </summary>
    private void StartThrowCycle()
    {
        _currentThrow++;
        GameEvents.RaiseThrowStarted(_currentThrow);
        Debug.Log($"--- Throw {_currentThrow}/{_maxThrows} ---");

        // Start with hand setup phase
        EnterHandSetupPhase();
    }

    private void EnterHandSetupPhase()
    {
        _handSetupPhase = new HandSetupPhase(_hand, _discardsRemainingThisRound, _maxDiscardCount);
        SetPhase(_handSetupPhase);
    }

    private void EnterThrowPhase()
    {
        _throwPhase = new ThrowPhase(_hand, _diceThrower, _diceManager);
        SetPhase(_throwPhase);
    }

    private void EnterScoringPhase(int throwScore)
    {
        _scoringPhase = new ScoringPhase(_scoreTracker, throwScore, _currentThrow, _maxThrows);
        _scoringPhase.OnScoringComplete += HandleScoringComplete;
        SetPhase(_scoringPhase);
    }

    private void SetPhase(IRoundPhase phase)
    {
        _currentPhase?.Exit();
        _currentPhase = phase;
        _currentPhase.Enter();
    }

    private void TransitionToNextPhase()
    {
        var currentPhaseType = _currentPhase.PhaseType;
        _currentPhase.Exit();

        switch (currentPhaseType)
        {
            case RoundPhase.HandSetup:
                EnterThrowPhase();
                break;

            case RoundPhase.Throwing:
                int throwScore = _throwPhase.ThrowScore;
                EnterScoringPhase(throwScore);
                break;

            case RoundPhase.Scoring:
                // Scoring phase handles its own completion via callback
                break;
        }
    }

    private void HandleScoringComplete(ScoringResult result)
    {
        _scoringPhase.OnScoringComplete -= HandleScoringComplete;

        // Store result and wait for UI confirmation
        _pendingScoringResult = result;
        _waitingForScoringConfirmation = true;
    }

    /// <summary>
    /// Called by UI to confirm scoring and continue to next phase.
    /// </summary>
    public void ConfirmScoring()
    {
        if (!_waitingForScoringConfirmation || !_pendingScoringResult.HasValue) return;

        var result = _pendingScoringResult.Value;
        _pendingScoringResult = null;
        _waitingForScoringConfirmation = false;

        // Clear thrown dice
        _diceThrower.ClearDice();

        switch (result)
        {
            case ScoringResult.GoalReached:
                EndRound(true);
                break;

            case ScoringResult.Continue:
                // Start next throw cycle
                StartThrowCycle();
                break;

            case ScoringResult.OutOfThrows:
                EndRound(false);
                break;
        }
    }

    /// <summary>
    /// Whether the round is waiting for scoring confirmation from UI.
    /// </summary>
    public bool WaitingForScoringConfirmation => _waitingForScoringConfirmation;

    private void EndRound(bool won)
    {
        _roundActive = false;
        _currentPhase = null;

        // Return remaining hand to inventory
        _hand.ReturnAllToInventory();

        if (won)
        {
            Debug.Log($"=== ROUND {_currentRound} WON! === Score: {_scoreTracker.CurrentScore}");
            GameEvents.RaiseRoundCompleted(_currentRound);
        }
        else
        {
            Debug.Log($"=== ROUND {_currentRound} LOST === Score: {_scoreTracker.CurrentScore}/{_scoreTracker.ScoreGoal}");
            GameEvents.RaiseRoundFailed(_currentRound);
        }

        OnRoundEnded?.Invoke(won);
    }

    /// <summary>
    /// Called by UI to confirm hand setup and proceed to throwing.
    /// </summary>
    public void ConfirmHandSetup()
    {
        if (_currentPhase is HandSetupPhase setup && setup.WaitingForInput)
        {
            setup.SkipDiscard();
        }
    }

    /// <summary>
    /// Called by UI to perform a discard during hand setup.
    /// </summary>
    public void PerformDiscard(System.Collections.Generic.List<int> indices)
    {
        if (_currentPhase is HandSetupPhase setup && setup.WaitingForInput)
        {
            setup.PerformDiscard(indices);
            // Track discards at round level
            if (_discardsRemainingThisRound > 0)
            {
                _discardsRemainingThisRound--;
            }
        }
    }

    /// <summary>
    /// Number of discards remaining for this round.
    /// </summary>
    public int DiscardsRemainingThisRound => _discardsRemainingThisRound;
}
