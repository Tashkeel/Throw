using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the shop phase between rounds.
/// Handles purchasing modifiers and applying enhancements to dice.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Inventory")]
    [SerializeField]
    [Tooltip("Available modifiers for purchase")]
    private List<ModifierData> _availableModifiers = new List<ModifierData>();

    [SerializeField]
    [Tooltip("Available enhancements for purchase")]
    private List<EnhancementData> _availableEnhancements = new List<EnhancementData>();

    private bool _isOpen;
    private DiceInventory _inventory;
    private CurrencyManager _currencyManager;
    private ModifierManager _modifierManager;

    /// <summary>
    /// Whether the shop is currently open.
    /// </summary>
    public bool IsOpen => _isOpen;

    /// <summary>
    /// Available modifiers for purchase.
    /// </summary>
    public IReadOnlyList<ModifierData> AvailableModifiers => _availableModifiers;

    /// <summary>
    /// Available enhancements for purchase.
    /// </summary>
    public IReadOnlyList<EnhancementData> AvailableEnhancements => _availableEnhancements;

    /// <summary>
    /// Reference to the currency manager.
    /// </summary>
    public CurrencyManager CurrencyManager => _currencyManager;

    /// <summary>
    /// Reference to the dice inventory.
    /// </summary>
    public DiceInventory Inventory => _inventory;

    /// <summary>
    /// Event fired when the shop is opened.
    /// </summary>
    public event Action OnShopOpened;

    /// <summary>
    /// Event fired when the shop is closed.
    /// </summary>
    public event Action OnShopClosed;

    /// <summary>
    /// Event fired when a purchase is made.
    /// </summary>
    public event Action OnPurchaseMade;

    /// <summary>
    /// Event fired when a modifier is sold.
    /// </summary>
    public event Action OnModifierSold;

    /// <summary>
    /// Initializes the shop with dependencies.
    /// </summary>
    public void Initialize(DiceInventory inventory, CurrencyManager currencyManager, ModifierManager modifierManager)
    {
        _inventory = inventory;
        _currencyManager = currencyManager;
        _modifierManager = modifierManager;
    }

    /// <summary>
    /// Legacy initialize for backwards compatibility.
    /// </summary>
    public void Initialize(DiceInventory inventory)
    {
        _inventory = inventory;
    }

    /// <summary>
    /// Opens the shop.
    /// </summary>
    public void OpenShop()
    {
        if (_isOpen) return;

        _isOpen = true;
        GameEvents.RaiseShopEntered();
        OnShopOpened?.Invoke();

        Debug.Log("=== SHOP OPENED ===");
        Debug.Log($"Current money: ${_currencyManager?.CurrentMoney ?? 0}");
        Debug.Log($"Available modifiers: {_availableModifiers.Count}");
        Debug.Log($"Available enhancements: {_availableEnhancements.Count}");
    }

    /// <summary>
    /// Closes the shop and continues to the next round.
    /// </summary>
    public void CloseShop()
    {
        if (!_isOpen) return;

        _isOpen = false;
        GameEvents.RaiseShopExited();
        OnShopClosed?.Invoke();

        Debug.Log("=== SHOP CLOSED ===");
    }

    /// <summary>
    /// Called by UI to continue from shop to next round.
    /// </summary>
    public void Continue()
    {
        CloseShop();
    }

    /// <summary>
    /// Returns modifiers from available list that are not currently owned.
    /// </summary>
    public List<ModifierData> GetUnownedModifiers()
    {
        var unowned = new List<ModifierData>();
        if (_modifierManager == null) return unowned;

        foreach (var modifier in _availableModifiers)
        {
            if (!_modifierManager.IsModifierOwned(modifier))
            {
                unowned.Add(modifier);
            }
        }
        return unowned;
    }

    /// <summary>
    /// Checks if a modifier is currently owned by the player.
    /// </summary>
    public bool IsModifierOwned(ModifierData modifier)
    {
        return _modifierManager != null && _modifierManager.IsModifierOwned(modifier);
    }

    /// <summary>
    /// Checks if the player can purchase a modifier (afford + not owned + not at capacity).
    /// </summary>
    public bool CanPurchaseModifier(ModifierData modifier)
    {
        if (modifier == null || _currencyManager == null || _modifierManager == null)
            return false;

        return _currencyManager.CanAfford(modifier.Cost)
            && !_modifierManager.IsModifierOwned(modifier)
            && !_modifierManager.IsAtCapacity;
    }

    /// <summary>
    /// Attempts to purchase a modifier. Checks ownership and capacity.
    /// </summary>
    public bool PurchaseModifier(ModifierData modifier)
    {
        if (modifier == null || _currencyManager == null || _modifierManager == null)
            return false;

        if (_modifierManager.IsModifierOwned(modifier))
        {
            Debug.Log($"Already own modifier '{modifier.DisplayName}'");
            return false;
        }

        if (_modifierManager.IsAtCapacity)
        {
            Debug.Log($"Cannot purchase modifier '{modifier.DisplayName}': at capacity ({_modifierManager.MaxModifiers})");
            return false;
        }

        if (!_currencyManager.CanAfford(modifier.Cost))
        {
            Debug.Log($"Cannot afford modifier '{modifier.DisplayName}' (cost: ${modifier.Cost})");
            return false;
        }

        if (!_currencyManager.SpendMoney(modifier.Cost))
            return false;

        if (!_modifierManager.AddModifier(modifier))
        {
            _currencyManager.AddMoney(modifier.Cost);
            return false;
        }

        Debug.Log($"Purchased modifier: {modifier.DisplayName}");
        OnPurchaseMade?.Invoke();
        return true;
    }

    /// <summary>
    /// Sells an active modifier, removing it and refunding half its cost.
    /// </summary>
    public bool SellModifier(ModifierData modifier)
    {
        if (modifier == null || _currencyManager == null || _modifierManager == null)
            return false;

        int sellPrice = _modifierManager.GetSellPrice(modifier);
        string name = modifier.Name;

        if (!_modifierManager.RemoveModifier(modifier))
            return false;

        if (sellPrice > 0)
        {
            _currencyManager.AddMoney(sellPrice);
        }

        Debug.Log($"Sold modifier: {name} for ${sellPrice}");
        OnModifierSold?.Invoke();
        return true;
    }

    /// <summary>
    /// Attempts to apply an enhancement to selected dice.
    /// </summary>
    /// <param name="enhancement">The enhancement to apply.</param>
    /// <param name="selectedDice">List of dice to enhance.</param>
    public bool ApplyEnhancement(EnhancementData enhancement, List<DiceDisplayItem> selectedDice)
    {
        if (enhancement == null || selectedDice == null || _currencyManager == null)
            return false;

        if (selectedDice.Count != enhancement.RequiredDiceCount)
        {
            Debug.Log($"Enhancement '{enhancement.DisplayName}' requires {enhancement.RequiredDiceCount} dice, but {selectedDice.Count} selected.");
            return false;
        }

        if (!_currencyManager.CanAfford(enhancement.Cost))
        {
            Debug.Log($"Cannot afford enhancement '{enhancement.DisplayName}' (cost: ${enhancement.Cost})");
            return false;
        }

        if (!_currencyManager.SpendMoney(enhancement.Cost))
            return false;

        // Let the enhancement analyze all selected dice before applying
        var allFaceValues = new List<int[]>();
        foreach (var diceData in selectedDice)
        {
            allFaceValues.Add(diceData._inventoryDie.GetFaceValues());
        }
        enhancement.PreProcess(allFaceValues);

        // Apply enhancement to each selected die
        foreach (var diceData in selectedDice)
        {
            diceData._inventoryDie.UpgradeDie(enhancement);
        }

        // If the enhancement creates duplicate dice, clone each selected die into inventory
        if (enhancement.CreatesDuplicateDie && _inventory != null)
        {
            foreach (var diceData in selectedDice)
            {
                var original = diceData._inventoryDie;
                var clone = new InventoryDie(original.GetFaceValues(), original._dieType, original.GetFaceTypes());
                _inventory.AddDice(clone);
            }
        }

        Debug.Log($"Applied enhancement '{enhancement.DisplayName}' to {selectedDice.Count} dice.");
        OnPurchaseMade?.Invoke();
        return true;
    }

    /// <summary>
    /// Checks if the player can afford a modifier.
    /// </summary>
    public bool CanAffordModifier(ModifierData modifier)
    {
        return modifier != null && _currencyManager != null && _currencyManager.CanAfford(modifier.Cost);
    }

    /// <summary>
    /// Checks if the player can afford an enhancement.
    /// </summary>
    public bool CanAffordEnhancement(EnhancementData enhancement)
    {
        return enhancement != null && _currencyManager != null && _currencyManager.CanAfford(enhancement.Cost);
    }
}
