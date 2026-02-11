using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase where dice are thrown and we wait for them to come to rest.
/// </summary>
public class ThrowPhase : IRoundPhase
{
    private readonly Hand _hand;
    private readonly DiceThrower _diceThrower;
    private readonly DiceManager _diceManager;

    private bool _isComplete;
    private bool _hasThrown;
    private int _throwScore;

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

    public ThrowPhase(Hand hand, DiceThrower diceThrower, DiceManager diceManager)
    {
        _hand = hand;
        _diceThrower = diceThrower;
        _diceManager = diceManager;
    }

    public void Enter()
    {
        _isComplete = false;
        _hasThrown = false;
        _throwScore = 0;

        Debug.Log($"Throw Phase: Ready to throw {_hand.Count} dice.");

        // Auto-throw for now (could wait for player input)
        ExecuteThrow();
    }

    public void Update()
    {
        if (!_hasThrown || _isComplete) return;

        // Check if dice have come to rest
        if (_diceManager.RollFinished)
        {
            OnDiceAtRest();
        }
    }

    public void Exit()
    {
        Debug.Log($"Throw Phase complete. Score: {_throwScore}");
    }

    /// <summary>
    /// Executes the throw with all dice in hand.
    /// </summary>
    public void ExecuteThrow()
    {
        if (_hasThrown)
        {
            Debug.LogWarning("Already thrown this phase.");
            return;
        }

        // Clear any existing dice
        _diceThrower.ClearDice();

        // Set number of dice to throw based on hand
        _diceThrower.NumberOfDice = _hand.Count;

        // Execute the throw
        _diceThrower.Throw();
        _hasThrown = true;

        OnThrowExecuted?.Invoke();
        Debug.Log($"Throwing {_hand.Count} dice...");
    }

    private void OnDiceAtRest()
    {
        _throwScore = _diceManager.LastRollTotal;
        _isComplete = true;

        GameEvents.RaiseAllDiceAtRest();
        GameEvents.RaiseDiceRolled(_throwScore);
        OnDiceSettled?.Invoke(_throwScore);

        Debug.Log($"Dice settled. Throw score: {_throwScore}");
    }
}
