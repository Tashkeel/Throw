using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's collection of dice.
/// </summary>
public class DiceInventory
{
    private readonly Dictionary<DiceData, int> _diceCount = new Dictionary<DiceData, int>();
    private DiceData _defaultDiceData;

    public event Action OnInventoryChanged;

    /// <summary>
    /// Gets the total number of dice in the inventory.
    /// </summary>
    public int TotalDiceCount
    {
        get
        {
            int total = 0;
            foreach (var count in _diceCount.Values)
            {
                total += count;
            }
            return total;
        }
    }

    /// <summary>
    /// Gets all dice types and their counts.
    /// </summary>
    public IReadOnlyDictionary<DiceData, int> AllDice => _diceCount;

    /// <summary>
    /// Initializes the inventory with a starting set of dice.
    /// </summary>
    /// <param name="defaultDice">The default dice type to use.</param>
    /// <param name="startingCount">Number of dice to start with.</param>
    public void Initialize(DiceData defaultDice, int startingCount)
    {
        _diceCount.Clear();
        _defaultDiceData = defaultDice;

        if (defaultDice != null && startingCount > 0)
        {
            _diceCount[defaultDice] = startingCount;
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Adds dice of a specific type to the inventory.
    /// </summary>
    public void AddDice(DiceData diceType, int count = 1)
    {
        if (diceType == null || count <= 0) return;

        if (_diceCount.ContainsKey(diceType))
        {
            _diceCount[diceType] += count;
        }
        else
        {
            _diceCount[diceType] = count;
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Removes dice of a specific type from the inventory.
    /// Returns true if successful, false if not enough dice.
    /// </summary>
    public bool RemoveDice(DiceData diceType, int count = 1)
    {
        if (diceType == null || count <= 0) return false;

        if (!_diceCount.ContainsKey(diceType) || _diceCount[diceType] < count)
        {
            return false;
        }

        _diceCount[diceType] -= count;

        if (_diceCount[diceType] <= 0)
        {
            _diceCount.Remove(diceType);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Gets the count of a specific dice type.
    /// </summary>
    public int GetCount(DiceData diceType)
    {
        if (diceType == null) return 0;
        return _diceCount.TryGetValue(diceType, out int count) ? count : 0;
    }

    /// <summary>
    /// Checks if the inventory has at least the specified count of a dice type.
    /// </summary>
    public bool HasDice(DiceData diceType, int count = 1)
    {
        return GetCount(diceType) >= count;
    }

    /// <summary>
    /// Gets the default dice type for this inventory.
    /// </summary>
    public DiceData GetDefaultDiceType()
    {
        return _defaultDiceData;
    }

    /// <summary>
    /// Returns all dice in the inventory as a flat list (for drawing).
    /// </summary>
    public List<DiceData> GetAllDiceAsList()
    {
        var list = new List<DiceData>();
        foreach (var kvp in _diceCount)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                list.Add(kvp.Key);
            }
        }
        return list;
    }
}
