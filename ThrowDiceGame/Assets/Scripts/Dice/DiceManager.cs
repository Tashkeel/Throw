using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active dice, monitors their state via per-die rest events,
/// applies modifiers progressively, and fires scoring events.
/// </summary>
public class DiceManager : MonoBehaviour
{
    private List<GameplayDie> _trackedDice = new List<GameplayDie>();
    private bool _isWaitingForRest;
    private bool _rollFinished;

    // Progressive scoring state
    private int _throwNumber;
    private int _roundNumber;
    private int _expectedDiceCount;
    private int _diceAtRestCount;
    private int _runningTotal;
    private List<int> _settledValues = new List<int>();
    private bool _monitoringActive;

    /// <summary>
    /// Event fired when all dice have come to rest. Provides total modified score.
    /// </summary>
    public event Action<int> OnRollComplete;

    /// <summary>
    /// Event fired when an individual die is scored. (dieIndex, rawValue, modifiedValue)
    /// </summary>
    public event Action<int, int, int> OnDieScored;

    /// <summary>
    /// Event fired when a money die settles. (dieIndex, value)
    /// </summary>
    public event Action<int, int> OnMoneyDieScored;

    /// <summary>
    /// Returns true if there are dice being tracked and we're waiting for them to settle.
    /// </summary>
    public bool IsWaitingForRest => _isWaitingForRest;

    /// <summary>
    /// Returns true if the current roll has finished (all dice at rest and result calculated).
    /// </summary>
    public bool RollFinished => _rollFinished;

    /// <summary>
    /// Returns a read-only list of all tracked dice.
    /// </summary>
    public IReadOnlyList<GameplayDie> TrackedDice => _trackedDice;

    /// <summary>
    /// The total score from the last completed roll.
    /// </summary>
    public int LastRollTotal { get; private set; }

    /// <summary>
    /// The current running score total (before AfterThrow modifiers).
    /// </summary>
    public int RunningTotal => _runningTotal;

    /// <summary>
    /// Sets throw context before a throw begins. Resets progressive scoring state.
    /// </summary>
    public void SetThrowContext(int throwNumber, int roundNumber, int expectedDiceCount)
    {
        _throwNumber = throwNumber;
        _roundNumber = roundNumber;
        _expectedDiceCount = expectedDiceCount;
        _diceAtRestCount = 0;
        _runningTotal = 0;
        _settledValues.Clear();
        _rollFinished = false;
        _isWaitingForRest = true;
    }

    /// <summary>
    /// Starts rest monitoring on all already-registered dice and sets monitoring flag
    /// so that newly registered dice also start monitoring immediately.
    /// </summary>
    public void StartMonitoringAll()
    {
        _monitoringActive = true;
        foreach (var die in _trackedDice)
        {
            if (die != null)
            {
                die.StartMonitoringRest();
            }
        }
    }

    /// <summary>
    /// Registers a die to be tracked by the manager.
    /// </summary>
    public void RegisterDie(GameplayDie die)
    {
        if (die == null) return;

        if (!_trackedDice.Contains(die))
        {
            _trackedDice.Add(die);
            die.OnDieAtRest += HandleDieAtRest;
            _isWaitingForRest = true;
            _rollFinished = false;

            // If monitoring is already active (staggered spawn), start this die immediately
            if (_monitoringActive)
            {
                die.StartMonitoringRest();
            }
        }
    }

    /// <summary>
    /// Unregisters a die from tracking.
    /// </summary>
    public void UnregisterDie(GameplayDie die)
    {
        if (die != null)
        {
            die.OnDieAtRest -= HandleDieAtRest;
            die.StopMonitoringRest();
        }
        _trackedDice.Remove(die);

        if (_trackedDice.Count == 0)
        {
            ResetState();
        }
    }

    /// <summary>
    /// Clears all tracked dice without destroying them.
    /// Called by DiceThrower when it clears its dice.
    /// </summary>
    public void ClearTrackedDice()
    {
        foreach (var die in _trackedDice)
        {
            if (die != null)
            {
                die.OnDieAtRest -= HandleDieAtRest;
                die.StopMonitoringRest();
            }
        }
        _trackedDice.Clear();
        ResetState();
    }

    /// <summary>
    /// Gets the individual die values from the last roll (settled values).
    /// </summary>
    public int[] GetLastRollValues()
    {
        return _settledValues.ToArray();
    }

    private void HandleDieAtRest(GameplayDie die, int rawValue)
    {
        int dieIndex = _diceAtRestCount;
        _diceAtRestCount++;

        DieSideType sideType = die.GetTopFaceSideType();

        if (sideType == DieSideType.Money)
        {
            // Money die — fire money event, don't add to score
            OnMoneyDieScored?.Invoke(dieIndex, rawValue);
            GameEvents.RaiseMoneyDieScored(dieIndex, rawValue);
            Debug.Log($"Die {dieIndex} earned money: ${rawValue}");
        }
        else
        {
            // Score die — apply modifiers and add to running total
            _settledValues.Add(rawValue);

            int modifiedValue = rawValue;
            if (ModifierManager.Instance != null)
            {
                modifiedValue = ModifierManager.Instance.ApplyPerDieModifiers(
                    rawValue, dieIndex, _settledValues.ToArray(), _throwNumber, _roundNumber);
            }

            _runningTotal += modifiedValue;

            OnDieScored?.Invoke(dieIndex, rawValue, modifiedValue);
            GameEvents.RaiseDieScored(dieIndex, rawValue, modifiedValue);
            Debug.Log($"Die {dieIndex} scored: raw={rawValue}, modified={modifiedValue} (running total: {_runningTotal})");
        }

        // Check if all expected dice have settled
        if (_diceAtRestCount >= _expectedDiceCount)
        {
            OnAllDiceSettled();
        }
    }

    private void OnAllDiceSettled()
    {
        // Apply after-throw modifiers to the running total
        int finalTotal = _runningTotal;
        if (ModifierManager.Instance != null)
        {
            finalTotal = ModifierManager.Instance.ApplyAfterThrowModifiers(
                _runningTotal, _settledValues.ToArray(), _throwNumber, _roundNumber);
        }

        _isWaitingForRest = false;
        _rollFinished = true;
        _monitoringActive = false;
        LastRollTotal = finalTotal;

        Debug.Log($"Roll finished! Score: {finalTotal} (raw sum: {_runningTotal})");

        OnRollComplete?.Invoke(finalTotal);
    }

    private void ResetState()
    {
        _isWaitingForRest = false;
        _rollFinished = false;
        _monitoringActive = false;
        _diceAtRestCount = 0;
        _runningTotal = 0;
    }
}
