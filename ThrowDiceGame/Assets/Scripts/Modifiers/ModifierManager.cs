using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active score modifiers and applies them during scoring.
/// Supports capacity limits, ownership tracking, and selling.
/// </summary>
public class ModifierManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Maximum number of modifiers that can be equipped at once")]
    private int _maxModifiers = 4;

    private List<ModifierData> _activeModifiers = new List<ModifierData>();

    /// <summary>
    /// Singleton instance for easy access.
    /// </summary>
    public static ModifierManager Instance { get; private set; }

    /// <summary>
    /// Reference to the currency manager, set by GameManager.
    /// Allows modifiers to award money (e.g., Economy of Scale).
    /// </summary>
    public CurrencyManager CurrencyManager { get; set; }

    /// <summary>
    /// Read-only list of active modifiers.
    /// </summary>
    public IReadOnlyList<ModifierData> ActiveModifiers => _activeModifiers;

    /// <summary>
    /// Maximum number of modifiers allowed.
    /// </summary>
    public int MaxModifiers => _maxModifiers;

    /// <summary>
    /// Whether the modifier capacity has been reached.
    /// </summary>
    public bool IsAtCapacity => _activeModifiers.Count >= _maxModifiers;

    /// <summary>
    /// Event fired when modifiers change.
    /// </summary>
    public event Action OnModifiersChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Adds a modifier. Respects capacity.
    /// </summary>
    public bool AddModifier(ModifierData modifierData)
    {
        if (modifierData == null) return false;

        if (IsAtCapacity)
        {
            Debug.Log($"Cannot add modifier '{modifierData.DisplayName}': at capacity ({_maxModifiers})");
            return false;
        }

        _activeModifiers.Add(modifierData);
        modifierData.OnActivated();
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log($"Modifier added: {modifierData.Name} ({_activeModifiers.Count}/{_maxModifiers})");
        return true;
    }

    /// <summary>
    /// Removes a modifier.
    /// </summary>
    public bool RemoveModifier(ModifierData modifier)
    {
        if (modifier == null) return false;

        bool removed = _activeModifiers.Remove(modifier);
        if (removed)
        {
            modifier.OnDeactivated();
            OnModifiersChanged?.Invoke();
            GameEvents.RaiseModifiersChanged();
            Debug.Log($"Modifier removed: {modifier.Name} ({_activeModifiers.Count}/{_maxModifiers})");
        }
        return removed;
    }

    /// <summary>
    /// Clears all active modifiers.
    /// </summary>
    public void ClearAllModifiers()
    {
        foreach (var modifier in _activeModifiers)
        {
            modifier.OnDeactivated();
        }
        _activeModifiers.Clear();
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log("All modifiers cleared.");
    }

    /// <summary>
    /// Checks if the given ModifierData is currently active.
    /// </summary>
    public bool IsModifierOwned(ModifierData modifierData)
    {
        if (modifierData == null) return false;
        return _activeModifiers.Contains(modifierData);
    }

    /// <summary>
    /// Gets the sell price for a modifier (half of purchase cost).
    /// </summary>
    public int GetSellPrice(ModifierData modifier)
    {
        if (modifier == null) return 0;
        return modifier.Cost / 2;
    }

    /// <summary>
    /// Applies all per-die modifiers to a single die's score.
    /// </summary>
    public int ApplyPerDieModifiers(int dieValue, int dieIndex, int[] allDieValues, int throwNumber, int roundNumber)
    {
        int modifiedScore = dieValue;

        var context = new ScoreModifierContext
        {
            CurrentScore = dieValue,
            OriginalValue = dieValue,
            DieIndex = dieIndex,
            TotalDiceCount = allDieValues.Length,
            AllDieValues = allDieValues,
            ThrowNumber = throwNumber,
            RoundNumber = roundNumber
        };

        foreach (var modifier in _activeModifiers)
        {
            if (modifier.Timing == ScoreModifierTiming.PerDie)
            {
                context.CurrentScore = modifiedScore;
                modifiedScore = modifier.ModifyScore(context);
            }
        }

        return modifiedScore;
    }

    /// <summary>
    /// Applies all after-throw modifiers to the total throw score.
    /// </summary>
    public int ApplyAfterThrowModifiers(int totalScore, int[] allDieValues, int throwNumber, int roundNumber)
    {
        int modifiedScore = totalScore;

        var context = new ScoreModifierContext
        {
            CurrentScore = totalScore,
            OriginalValue = totalScore,
            DieIndex = -1,
            TotalDiceCount = allDieValues.Length,
            AllDieValues = allDieValues,
            ThrowNumber = throwNumber,
            RoundNumber = roundNumber
        };

        foreach (var modifier in _activeModifiers)
        {
            if (modifier.Timing == ScoreModifierTiming.AfterThrow)
            {
                context.CurrentScore = modifiedScore;
                modifiedScore = modifier.ModifyScore(context);
            }
        }

        return modifiedScore;
    }

    /// <summary>
    /// Applies all modifiers and returns the final score.
    /// Call this from scoring logic with all die values.
    /// </summary>
    public int CalculateModifiedScore(int[] dieValues, int throwNumber, int roundNumber)
    {
        if (dieValues == null || dieValues.Length == 0) return 0;

        int totalScore = 0;
        for (int i = 0; i < dieValues.Length; i++)
        {
            int modifiedDieValue = ApplyPerDieModifiers(dieValues[i], i, dieValues, throwNumber, roundNumber);
            totalScore += modifiedDieValue;
        }

        totalScore = ApplyAfterThrowModifiers(totalScore, dieValues, throwNumber, roundNumber);

        return totalScore;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
