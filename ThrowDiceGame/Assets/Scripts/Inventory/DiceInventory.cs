using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's collection of dice.
/// </summary>
public class DiceInventory
{
    private List<InventoryDie> _inventoryDice = new List<InventoryDie>();
    private DiceData _defaultDiceData;

    public event Action OnInventoryChanged;

    /// <summary>
    /// Gets the total number of dice in the inventory.
    /// </summary>
    public int TotalDiceCount
    {
        get
        {
            return _inventoryDice.Count;
        }
    }


    /// <summary>
    /// Initializes the inventory with a starting set of dice.
    /// </summary>
    /// <param name="defaultDice">The default dice type to use.</param>
    /// <param name="startingCount">Number of dice to start with.</param>
    public void Initialize(InventoryDie defaultDice, int startingCount)
    {
        _inventoryDice.Clear();
        _defaultDiceData = defaultDice._dieType;

        if (defaultDice != null && startingCount > 0)
        {
            AddDice(defaultDice, startingCount);
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Adds dice of a specific type to the inventory.
    /// Creates distinct copies so each die is an independent instance.
    /// </summary>
    public void AddDice(InventoryDie die, int count = 1)
    {
        if (die._dieType == null || count <= 0) return;

        // First copy goes in directly
        _inventoryDice.Add(die);

        // Additional copies are cloned so each is independent
        for (int i = 1; i < count; i++)
        {
            _inventoryDice.Add(new InventoryDie(die.GetFaceValues(), die._dieType, die.GetFaceTypes()));
        }

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Removes dice of a specific type from the inventory.
    /// Returns true if successful, false if not enough dice.
    /// </summary>
    public bool RemoveDice(InventoryDie die)
    {
        if (die._dieType == null) return false;

        if (!_inventoryDice.Contains(die))
        {
            return false;
        }

        _inventoryDice.Remove(die);

        OnInventoryChanged?.Invoke();
        return true;
    }


    /// <summary>
    /// Checks if the inventory has at least the specified count of a dice type.
    /// </summary>
    public bool HasDice(int count = 1)
    {
        return TotalDiceCount >= count;
    }

    /// <summary>
    /// Gets the default dice type for this inventory.
    /// </summary>
    public DiceData GetDefaultDiceType()
    {
        return _defaultDiceData;
    }

    public InventoryDie DrawRandomInventoryDie()
    {
        InventoryDie randomDie = _inventoryDice[UnityEngine.Random.Range(0, TotalDiceCount)];
        RemoveDice(randomDie);
        return randomDie;
    }

    /// <summary>
    /// Returns all dice in the inventory as a flat list (for drawing).
    /// </summary>
    public List<InventoryDie> GetAllDiceAsList()
    {
        return _inventoryDice;
    }
}
