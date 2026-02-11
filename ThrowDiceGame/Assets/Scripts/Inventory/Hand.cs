using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the player's current hand of dice for a round.
/// Dice in hand are drawn from inventory and can be discarded back.
/// </summary>
public class Hand
{
    private readonly List<DiceData> _dice = new List<DiceData>();
    private readonly List<DiceData> _discardedThisRound = new List<DiceData>();
    private readonly DiceInventory _inventory;
    private readonly int _maxHandSize;

    public event Action OnHandChanged;

    /// <summary>
    /// Current dice in hand.
    /// </summary>
    public IReadOnlyList<DiceData> Dice => _dice;

    /// <summary>
    /// Number of dice currently in hand.
    /// </summary>
    public int Count => _dice.Count;

    /// <summary>
    /// Maximum hand size.
    /// </summary>
    public int MaxSize => _maxHandSize;

    /// <summary>
    /// Whether the hand is full.
    /// </summary>
    public bool IsFull => _dice.Count >= _maxHandSize;

    /// <summary>
    /// Number of dice discarded this round (not yet returned to inventory).
    /// </summary>
    public int DiscardedThisRoundCount => _discardedThisRound.Count;

    public Hand(DiceInventory inventory, int maxHandSize)
    {
        _inventory = inventory;
        _maxHandSize = maxHandSize;
    }

    /// <summary>
    /// Draws dice from inventory to fill the hand up to max size.
    /// </summary>
    /// <returns>Number of dice drawn.</returns>
    public int DrawToFull()
    {
        int drawn = 0;
        var defaultDice = _inventory.GetDefaultDiceType();

        while (_dice.Count < _maxHandSize && _inventory.TotalDiceCount > 0)
        {
            // For now, just draw from the default dice type
            // Future: Could implement weighted drawing or player choice
            if (_inventory.HasDice(defaultDice))
            {
                if (_inventory.RemoveDice(defaultDice, 1))
                {
                    _dice.Add(defaultDice);
                    drawn++;
                }
            }
            else
            {
                // Draw from any available dice
                bool foundDice = false;
                foreach (var kvp in _inventory.AllDice)
                {
                    if (kvp.Value > 0)
                    {
                        if (_inventory.RemoveDice(kvp.Key, 1))
                        {
                            _dice.Add(kvp.Key);
                            drawn++;
                            foundDice = true;
                            break;
                        }
                    }
                }
                if (!foundDice) break;
            }
        }

        if (drawn > 0)
        {
            OnHandChanged?.Invoke();
        }

        return drawn;
    }

    /// <summary>
    /// Discards specific dice from hand back to inventory.
    /// </summary>
    /// <param name="indices">Indices of dice to discard.</param>
    /// <returns>Number of dice discarded.</returns>
    public int Discard(List<int> indices)
    {
        if (indices == null || indices.Count == 0) return 0;

        // Sort indices in descending order to remove from end first
        indices.Sort((a, b) => b.CompareTo(a));

        int discarded = 0;
        foreach (int index in indices)
        {
            if (index >= 0 && index < _dice.Count)
            {
                var diceData = _dice[index];
                _dice.RemoveAt(index);
                _inventory.AddDice(diceData, 1);
                discarded++;
            }
        }

        if (discarded > 0)
        {
            OnHandChanged?.Invoke();
        }

        return discarded;
    }

    /// <summary>
    /// Discards specific dice from hand for the rest of the round.
    /// These dice are NOT returned to inventory until the round ends.
    /// </summary>
    /// <param name="indices">Indices of dice to discard.</param>
    /// <returns>Number of dice discarded.</returns>
    public int DiscardForRound(List<int> indices)
    {
        if (indices == null || indices.Count == 0) return 0;

        // Sort indices in descending order to remove from end first
        indices.Sort((a, b) => b.CompareTo(a));

        int discarded = 0;
        foreach (int index in indices)
        {
            if (index >= 0 && index < _dice.Count)
            {
                var diceData = _dice[index];
                _dice.RemoveAt(index);
                _discardedThisRound.Add(diceData);
                discarded++;
            }
        }

        if (discarded > 0)
        {
            OnHandChanged?.Invoke();
        }

        return discarded;
    }

    /// <summary>
    /// Discards a single die at the specified index.
    /// </summary>
    public bool DiscardAt(int index)
    {
        if (index < 0 || index >= _dice.Count) return false;

        var diceData = _dice[index];
        _dice.RemoveAt(index);
        _inventory.AddDice(diceData, 1);

        OnHandChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Returns all dice from hand and discarded pile back to inventory.
    /// </summary>
    public void ReturnAllToInventory()
    {
        foreach (var dice in _dice)
        {
            _inventory.AddDice(dice, 1);
        }
        _dice.Clear();

        // Also return dice that were discarded during the round
        foreach (var dice in _discardedThisRound)
        {
            _inventory.AddDice(dice, 1);
        }
        _discardedThisRound.Clear();

        OnHandChanged?.Invoke();
    }

    /// <summary>
    /// Clears the hand and discarded pile without returning dice to inventory.
    /// Use when dice are consumed (e.g., after final throw).
    /// </summary>
    public void Clear()
    {
        _dice.Clear();
        _discardedThisRound.Clear();
        OnHandChanged?.Invoke();
    }

    /// <summary>
    /// Gets the dice data at a specific index.
    /// </summary>
    public DiceData GetAt(int index)
    {
        if (index < 0 || index >= _dice.Count) return null;
        return _dice[index];
    }
}
