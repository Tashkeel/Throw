using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active dice, monitors their state, and calculates results when all dice come to rest.
/// </summary>
public class DiceManager : MonoBehaviour
{
    [Header("Rest Detection")]
    [SerializeField]
    [Tooltip("How often to check if dice have come to rest (in seconds)")]
    private float _checkInterval = 0.1f;

    [SerializeField]
    [Tooltip("Minimum time dice must be at rest before confirming the roll is finished")]
    private float _restConfirmationTime = 0.2f;

    private List<Die> _trackedDice = new List<Die>();
    private float _checkTimer;
    private float _restTimer;
    private bool _isWaitingForRest;
    private bool _rollFinished;

    /// <summary>
    /// Event fired when all dice have come to rest.
    /// </summary>
    public event Action<int> OnRollComplete; // total score

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
    public IReadOnlyList<Die> TrackedDice => _trackedDice;

    /// <summary>
    /// The total score from the last completed roll.
    /// </summary>
    public int LastRollTotal { get; private set; }

    private void Update()
    {
        if (!_isWaitingForRest || _trackedDice.Count == 0)
        {
            return;
        }

        _checkTimer += Time.deltaTime;

        if (_checkTimer >= _checkInterval)
        {
            _checkTimer = 0f;
            CheckDiceAtRest();
        }
    }

    /// <summary>
    /// Registers a die to be tracked by the manager.
    /// </summary>
    public void RegisterDie(Die die)
    {
        if (die == null) return;

        if (!_trackedDice.Contains(die))
        {
            _trackedDice.Add(die);
            _isWaitingForRest = true;
            _rollFinished = false;
            _restTimer = 0f;
        }
    }

    /// <summary>
    /// Unregisters a die from tracking.
    /// </summary>
    public void UnregisterDie(Die die)
    {
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
        _trackedDice.Clear();
        ResetState();
    }

    private void ResetState()
    {
        _isWaitingForRest = false;
        _rollFinished = false;
        _checkTimer = 0f;
        _restTimer = 0f;
    }

    private void CheckDiceAtRest()
    {
        // Remove any null references (destroyed dice)
        _trackedDice.RemoveAll(d => d == null);

        if (_trackedDice.Count == 0)
        {
            ResetState();
            return;
        }

        bool allAtRest = true;

        foreach (var die in _trackedDice)
        {
            if (!die.IsAtRest)
            {
                allAtRest = false;
                break;
            }
        }

        if (allAtRest)
        {
            _restTimer += _checkInterval;

            // Confirm rest state persists for the confirmation duration
            if (_restTimer >= _restConfirmationTime)
            {
                OnRollFinished();
            }
        }
        else
        {
            // Reset confirmation timer if any die is still moving
            _restTimer = 0f;
        }
    }

    private void OnRollFinished()
    {
        _isWaitingForRest = false;
        _rollFinished = true;

        int total = CalculateTotal();
        LastRollTotal = total;

        Debug.Log($"Roll finished! Score: {total}");

        OnRollComplete?.Invoke(total);
    }

    private int CalculateTotal()
    {
        // Collect all die values
        var dieValues = new List<int>();
        foreach (var die in _trackedDice)
        {
            if (die != null && die.TryGetTopFaceValue(out int value))
            {
                dieValues.Add(value);
            }
        }

        if (dieValues.Count == 0) return 0;

        int[] valuesArray = dieValues.ToArray();

        // Apply modifiers if ModifierManager exists
        if (ModifierManager.Instance != null)
        {
            // Get current throw and round info (default to 1 if not available)
            int throwNumber = 1;
            int roundNumber = 1;

            return ModifierManager.Instance.CalculateModifiedScore(valuesArray, throwNumber, roundNumber);
        }

        // Fallback: simple sum if no modifier manager
        int total = 0;
        foreach (int value in valuesArray)
        {
            total += value;
        }
        return total;
    }

    /// <summary>
    /// Gets the individual die values from the last roll (before modifiers).
    /// </summary>
    public int[] GetLastRollValues()
    {
        var values = new List<int>();
        foreach (var die in _trackedDice)
        {
            if (die != null && die.TryGetTopFaceValue(out int value))
            {
                values.Add(value);
            }
        }
        return values.ToArray();
    }
}
