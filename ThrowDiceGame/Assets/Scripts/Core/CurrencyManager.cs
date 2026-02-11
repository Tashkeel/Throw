using System;
using UnityEngine;

/// <summary>
/// Manages the player's currency (money) that can be spent in the shop.
/// Money is earned after successful rounds and resets on game over.
/// </summary>
public class CurrencyManager
{
    private int _currentMoney;
    private int _moneyPerThrowRemaining;

    /// <summary>
    /// Current amount of money the player has.
    /// </summary>
    public int CurrentMoney => _currentMoney;

    /// <summary>
    /// Money earned per remaining throw after a successful round.
    /// </summary>
    public int MoneyPerThrowRemaining => _moneyPerThrowRemaining;

    /// <summary>
    /// Event fired when money amount changes.
    /// </summary>
    public event Action<int> OnMoneyChanged;

    public CurrencyManager(int moneyPerThrowRemaining = 10)
    {
        _moneyPerThrowRemaining = moneyPerThrowRemaining;
        _currentMoney = 0;
    }

    /// <summary>
    /// Adds money to the player's balance.
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);
        GameEvents.RaiseMoneyChanged(_currentMoney);
        GameEvents.RaiseMoneyEarned(amount);
        Debug.Log($"Earned ${amount}. Total: ${_currentMoney}");
    }

    /// <summary>
    /// Attempts to spend money. Returns true if successful.
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || amount > _currentMoney) return false;

        _currentMoney -= amount;
        OnMoneyChanged?.Invoke(_currentMoney);
        GameEvents.RaiseMoneyChanged(_currentMoney);
        Debug.Log($"Spent ${amount}. Remaining: ${_currentMoney}");
        return true;
    }

    /// <summary>
    /// Checks if the player can afford an amount.
    /// </summary>
    public bool CanAfford(int amount)
    {
        return amount > 0 && _currentMoney >= amount;
    }

    /// <summary>
    /// Awards money based on remaining throws after a successful round.
    /// </summary>
    /// <param name="throwsRemaining">Number of unused throws.</param>
    public void AwardRoundBonus(int throwsRemaining)
    {
        if (throwsRemaining <= 0) return;

        int bonus = throwsRemaining * _moneyPerThrowRemaining;
        AddMoney(bonus);
        Debug.Log($"Round bonus: {throwsRemaining} throws remaining x ${_moneyPerThrowRemaining} = ${bonus}");
    }

    /// <summary>
    /// Resets money to zero (called on game over).
    /// </summary>
    public void Reset()
    {
        _currentMoney = 0;
        OnMoneyChanged?.Invoke(_currentMoney);
        GameEvents.RaiseMoneyChanged(_currentMoney);
        Debug.Log("Currency reset.");
    }
}
