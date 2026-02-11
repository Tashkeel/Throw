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
    /// Attempts to purchase a modifier.
    /// </summary>
    public bool PurchaseModifier(ModifierData modifier)
    {
        if (modifier == null || _currencyManager == null || _modifierManager == null)
            return false;

        if (!_currencyManager.CanAfford(modifier.Cost))
        {
            Debug.Log($"Cannot afford modifier '{modifier.DisplayName}' (cost: ${modifier.Cost})");
            return false;
        }

        if (!_currencyManager.SpendMoney(modifier.Cost))
            return false;

        if (!_modifierManager.AddModifier(modifier))
        {
            // Refund if adding failed
            _currencyManager.AddMoney(modifier.Cost);
            return false;
        }

        Debug.Log($"Purchased modifier: {modifier.DisplayName}");
        OnPurchaseMade?.Invoke();
        return true;
    }

    /// <summary>
    /// Attempts to apply an enhancement to selected dice.
    /// </summary>
    /// <param name="enhancement">The enhancement to apply.</param>
    /// <param name="selectedDice">List of dice to enhance.</param>
    public bool ApplyEnhancement(EnhancementData enhancement, List<DiceData> selectedDice)
    {
        if (enhancement == null || selectedDice == null || _currencyManager == null)
            return false;

        // Validate dice count
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

        // Create enhancement instance
        var enhancementInstance = enhancement.CreateEnhancementInstance(transform);
        if (enhancementInstance == null)
        {
            _currencyManager.AddMoney(enhancement.Cost);
            return false;
        }

        // Apply enhancement to each selected die
        foreach (var diceData in selectedDice)
        {
            // Create enhanced copy
            var enhancedDice = diceData.CreateEnhancedCopy(enhancementInstance);

            // Remove original from inventory, add enhanced version
            _inventory.RemoveDice(diceData, 1);
            _inventory.AddDice(enhancedDice, 1);
        }

        // Cleanup the enhancement instance
        if (enhancementInstance is MonoBehaviour mb)
        {
            Destroy(mb.gameObject);
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
