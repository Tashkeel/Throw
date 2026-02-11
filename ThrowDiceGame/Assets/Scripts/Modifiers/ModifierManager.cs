using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active score modifiers and applies them during scoring.
/// </summary>
public class ModifierManager : MonoBehaviour
{
    private List<IScoreModifier> _activeModifiers = new List<IScoreModifier>();
    private Transform _modifierContainer;

    /// <summary>
    /// Singleton instance for easy access.
    /// </summary>
    public static ModifierManager Instance { get; private set; }

    /// <summary>
    /// Read-only list of active modifiers.
    /// </summary>
    public IReadOnlyList<IScoreModifier> ActiveModifiers => _activeModifiers;

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

        // Create container for modifier instances
        _modifierContainer = new GameObject("ModifierInstances").transform;
        _modifierContainer.SetParent(transform);
    }

    /// <summary>
    /// Adds a modifier from ModifierData.
    /// </summary>
    public bool AddModifier(ModifierData modifierData)
    {
        if (modifierData == null) return false;

        var modifier = modifierData.CreateModifierInstance(_modifierContainer);
        if (modifier == null) return false;

        _activeModifiers.Add(modifier);
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log($"Modifier added: {modifier.Name}");
        return true;
    }

    /// <summary>
    /// Adds a modifier instance directly.
    /// </summary>
    public void AddModifier(IScoreModifier modifier)
    {
        if (modifier == null) return;

        _activeModifiers.Add(modifier);
        OnModifiersChanged?.Invoke();
        Debug.Log($"Modifier added: {modifier.Name}");
    }

    /// <summary>
    /// Removes a modifier.
    /// </summary>
    public bool RemoveModifier(IScoreModifier modifier)
    {
        if (modifier == null) return false;

        bool removed = _activeModifiers.Remove(modifier);
        if (removed)
        {
            // Destroy the MonoBehaviour if it is one
            if (modifier is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }
            OnModifiersChanged?.Invoke();
            Debug.Log($"Modifier removed: {modifier.Name}");
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
            if (modifier is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }
        }
        _activeModifiers.Clear();
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log("All modifiers cleared.");
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

        // First, apply per-die modifiers
        int totalScore = 0;
        for (int i = 0; i < dieValues.Length; i++)
        {
            int modifiedDieValue = ApplyPerDieModifiers(dieValues[i], i, dieValues, throwNumber, roundNumber);
            totalScore += modifiedDieValue;
        }

        // Then apply after-throw modifiers
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
