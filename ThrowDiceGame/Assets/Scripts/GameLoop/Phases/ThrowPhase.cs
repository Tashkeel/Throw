using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase where dice are thrown and we wait for them to come to rest.
/// Handles progressive scoring (per-die) and money die routing.
/// </summary>
public class ThrowPhase : IRoundPhase
{
    private readonly Hand _hand;
    private readonly DiceThrower _diceThrower;
    private readonly DiceManager _diceManager;
    private readonly ScoreTracker _scoreTracker;
    private readonly CurrencyManager _currencyManager;
    private readonly int _throwNumber;
    private readonly int _roundNumber;
    private readonly List<int> _selectedIndices;

    private bool _isComplete;
    private bool _hasThrown;
    private int _throwScore;
    private List<InventoryDie> _selectedDice;
    private int _perDieScoreSum;

    public RoundPhase PhaseType => RoundPhase.Throwing;
    public bool IsComplete => _isComplete;

    /// <summary>
    /// The score from the current throw (valid after dice come to rest).
    /// </summary>
    public int ThrowScore => _throwScore;

    /// <summary>
    /// Event fired when the throw is executed.
    /// </summary>
    public event Action OnThrowExecuted;

    /// <summary>
    /// Event fired when all dice have come to rest.
    /// </summary>
    public event Action<int> OnDiceSettled; // score

    public ThrowPhase(Hand hand, DiceThrower diceThrower, DiceManager diceManager,
        ScoreTracker scoreTracker, CurrencyManager currencyManager,
        int throwNumber, int roundNumber, List<int> selectedIndices)
    {
        _hand = hand;
        _diceThrower = diceThrower;
        _diceManager = diceManager;
        _scoreTracker = scoreTracker;
        _currencyManager = currencyManager;
        _throwNumber = throwNumber;
        _roundNumber = roundNumber;
        _selectedIndices = selectedIndices;
    }

    public void Enter()
    {
        _isComplete = false;
        _hasThrown = false;
        _throwScore = 0;
        _perDieScoreSum = 0;

        Debug.Log($"Throw Phase: Ready to throw {_selectedIndices.Count} of {_hand.Count} dice.");

        // Auto-throw
        ExecuteThrow();
    }

    public void Update()
    {
        if (!_hasThrown || _isComplete) return;

        // Check if dice have come to rest (via RollFinished set by DiceManager)
        if (_diceManager.RollFinished)
        {
            OnDiceAtRest();
        }
    }

    public void Exit()
    {
        UnsubscribeFromEvents();
        Debug.Log($"Throw Phase complete. Score: {_throwScore}");
    }

    /// <summary>
    /// Executes the throw with selected dice from hand.
    /// </summary>
    public void ExecuteThrow()
    {
        if (_hasThrown)
        {
            Debug.LogWarning("Already thrown this phase.");
            return;
        }

        // Extract selected dice from hand (removes them from hand)
        _selectedDice = _hand.ExtractDice(_selectedIndices);

        if (_selectedDice.Count == 0)
        {
            Debug.LogError("ThrowPhase: No dice selected for throwing.");
            return;
        }

        // Clear any existing dice
        _diceThrower.ClearDice();

        // Set number of dice to throw
        _diceThrower.NumberOfDice = _selectedDice.Count;

        // Pass selected dice data so spawned GameplayDie get correct face values
        _diceThrower.SetHandDice(_selectedDice);

        // Set throw context on DiceManager before throwing
        _diceManager.SetThrowContext(_throwNumber, _roundNumber, _selectedDice.Count);

        // Subscribe to per-die scoring events
        _diceManager.OnDieScored += HandleDieScored;
        _diceManager.OnMoneyDieScored += HandleMoneyDieScored;
        _diceManager.OnRollComplete += HandleRollComplete;

        // Execute the throw
        _diceThrower.Throw();
        _hasThrown = true;

        // Start monitoring after dice begin spawning
        _diceManager.StartMonitoringAll();

        OnThrowExecuted?.Invoke();
        Debug.Log($"Throwing {_selectedDice.Count} dice...");
    }

    private void HandleDieScored(int dieIndex, int rawValue, int modifiedValue)
    {
        _perDieScoreSum += modifiedValue;

        // Add score progressively — this fires OnScoreChanged → HUD updates live
        _scoreTracker.AddScore(modifiedValue);
    }

    private void HandleMoneyDieScored(int dieIndex, int value)
    {
        // Route money die value to currency
        _currencyManager.AddMoney(value);
    }

    private void HandleRollComplete(int finalTotal)
    {
        // Apply AfterThrow modifier bonus (difference between final total and sum of per-die scores)
        int afterThrowBonus = finalTotal - _perDieScoreSum;
        if (afterThrowBonus > 0)
        {
            _scoreTracker.AddScore(afterThrowBonus);
        }

        _throwScore = finalTotal;
    }

    private void OnDiceAtRest()
    {
        _isComplete = true;

        GameEvents.RaiseAllDiceAtRest();
        GameEvents.RaiseDiceRolled(_throwScore);
        OnDiceSettled?.Invoke(_throwScore);

        Debug.Log($"Dice settled. Throw score: {_throwScore}");
    }

    private void UnsubscribeFromEvents()
    {
        _diceManager.OnDieScored -= HandleDieScored;
        _diceManager.OnMoneyDieScored -= HandleMoneyDieScored;
        _diceManager.OnRollComplete -= HandleRollComplete;
    }
}
