using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase where players can discard and redraw dice before throwing.
/// </summary>
public class HandSetupPhase : IRoundPhase
{
    private readonly Hand _hand;
    private readonly int _maxDiscards;
    private readonly int _maxDiscardCount;

    private int _discardsRemaining;
    private bool _isComplete;
    private bool _waitingForInput;
    private List<int> _selectedThrowIndices;

    public RoundPhase PhaseType => RoundPhase.HandSetup;
    public bool IsComplete => _isComplete;

    /// <summary>
    /// Number of discard actions remaining this phase.
    /// </summary>
    public int DiscardsRemaining => _discardsRemaining;

    /// <summary>
    /// Maximum dice that can be discarded per action.
    /// </summary>
    public int MaxDiscardCount => _maxDiscardCount;

    /// <summary>
    /// Whether the phase is waiting for player input.
    /// </summary>
    public bool WaitingForInput => _waitingForInput;

    /// <summary>
    /// The indices of dice selected for throwing, set before completing the phase.
    /// </summary>
    public List<int> SelectedThrowIndices => _selectedThrowIndices;

    /// <summary>
    /// Event fired when the phase needs player input for discard selection.
    /// </summary>
    public event Action OnDiscardInputRequested;

    /// <summary>
    /// Event fired when discards are complete and hand is redrawn.
    /// </summary>
    public event Action OnRedrawComplete;

    public HandSetupPhase(Hand hand, int maxDiscards = 1, int maxDiscardCount = 5)
    {
        _hand = hand;
        _maxDiscards = maxDiscards;
        _maxDiscardCount = maxDiscardCount;
    }

    public void Enter()
    {
        _isComplete = false;
        _discardsRemaining = _maxDiscards;

        // Draw from inventory to fill hand back to max size
        _hand.DrawToFull();

        GameEvents.RaiseHandSetupStarted();
        Debug.Log($"Hand Setup: {_hand.Count} dice in hand. {_discardsRemaining} discards available.");

        // Always wait for player to click Throw to proceed
        _waitingForInput = true;
        OnDiscardInputRequested?.Invoke();
    }

    public void Update()
    {
        // Phase waits for player input via PerformDiscard or SkipDiscard
    }

    public void Exit()
    {
        _waitingForInput = false;
        GameEvents.RaiseHandSetupCompleted();
        Debug.Log($"Hand Setup complete. {_hand.Count} dice ready to throw.");
    }

    /// <summary>
    /// Performs a discard action with the specified dice indices.
    /// </summary>
    /// <param name="indicesToDiscard">Indices of dice to discard (max 5).</param>
    public void PerformDiscard(List<int> indicesToDiscard)
    {
        if (_discardsRemaining <= 0 || _isComplete)
        {
            Debug.LogWarning("No discards remaining.");
            return;
        }

        // Limit to max discard count
        if (indicesToDiscard != null && indicesToDiscard.Count > _maxDiscardCount)
        {
            Debug.LogWarning($"Can only discard up to {_maxDiscardCount} dice. Truncating selection.");
            indicesToDiscard = indicesToDiscard.GetRange(0, _maxDiscardCount);
        }

        // Perform the discard (dice are set aside for the round, not returned to inventory)
        int discarded = _hand.DiscardForRound(indicesToDiscard);
        Debug.Log($"Discarded {discarded} dice for the round.");

        // Draw new dice from inventory to fill hand
        int drawn = _hand.DrawToFull();
        Debug.Log($"Drew {drawn} new dice from inventory.");

        _discardsRemaining--;
        OnRedrawComplete?.Invoke();

        // Continue waiting for input - user must click Throw to proceed
        _waitingForInput = true;
        OnDiscardInputRequested?.Invoke();
    }

    /// <summary>
    /// Sets the indices of dice selected for throwing.
    /// </summary>
    public void SetThrowSelection(List<int> indices)
    {
        _selectedThrowIndices = indices != null ? new List<int>(indices) : new List<int>();
    }

    /// <summary>
    /// Confirms the throw selection and completes the phase.
    /// </summary>
    public void ConfirmThrow()
    {
        CompletePhase();
    }

    /// <summary>
    /// Skips the remaining discard opportunities and completes the phase.
    /// </summary>
    public void SkipDiscard()
    {
        CompletePhase();
    }

    /// <summary>
    /// Skips just one discard action (if multiple available).
    /// </summary>
    public void SkipOneDiscard()
    {
        if (_discardsRemaining > 0)
        {
            _discardsRemaining--;
        }
        OnDiscardInputRequested?.Invoke();
    }

    private void CompletePhase()
    {
        _waitingForInput = false;
        _isComplete = true;
    }
}
