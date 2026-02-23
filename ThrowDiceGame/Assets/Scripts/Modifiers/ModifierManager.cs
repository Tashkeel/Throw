using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active score modifiers and implements IScoringPipeline so DiceManager
/// can invoke modifiers without depending on the singleton.
/// </summary>
public class ModifierManager : MonoBehaviour, IScoringPipeline
{
    [SerializeField]
    [Tooltip("Maximum number of modifiers that can be equipped at once")]
    private int _maxModifiers = 4;

    private List<ModifierData> _activeModifiers = new List<ModifierData>();

    /// <summary>Singleton instance for legacy access. Prefer injecting IScoringPipeline.</summary>
    public static ModifierManager Instance { get; private set; }

    /// <summary>
    /// Reference to the currency manager, set by GameManager.
    /// Passed to modifiers via IModifierContext on equip.
    /// </summary>
    public CurrencyManager CurrencyManager { get; set; }

    /// <summary>Read-only list of active modifiers.</summary>
    public IReadOnlyList<ModifierData> ActiveModifiers => _activeModifiers;

    /// <summary>Maximum number of modifiers allowed.</summary>
    public int MaxModifiers => _maxModifiers;

    /// <summary>Whether the modifier capacity has been reached.</summary>
    public bool IsAtCapacity => _activeModifiers.Count >= _maxModifiers;

    /// <summary>Event fired when modifiers change.</summary>
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

    // ── IScoringPipeline ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public int ProcessPerDie(int raw, int dieIndex, IReadOnlyList<int> allValues, int throwNum, int roundNum)
    {
        int modified = raw;
        foreach (var modifier in _activeModifiers)
        {
            if (modifier is IPerDieModifier perDie)
            {
                var ctx = new PerDieContext(modified, raw, dieIndex, allValues.Count, allValues, throwNum, roundNum);
                modified = perDie.ApplyPerDie(ctx);
            }
        }
        return modified;
    }

    /// <inheritdoc/>
    public int ProcessAfterThrow(int total, IReadOnlyList<int> perDieValues, IReadOnlyList<int> rawValues, int throwNum, int roundNum)
    {
        int modified = total;
        foreach (var modifier in _activeModifiers)
        {
            if (modifier is IAfterThrowModifier afterThrow)
            {
                var ctx = new AfterThrowContext(modified, perDieValues, rawValues, throwNum, roundNum);
                modified = afterThrow.ApplyAfterThrow(ctx);
            }
        }
        return modified;
    }

    // ── Modifier management ───────────────────────────────────────────────────

    /// <summary>Adds a modifier. Respects capacity.</summary>
    public bool AddModifier(ModifierData modifierData)
    {
        if (modifierData == null) return false;

        if (IsAtCapacity)
        {
            Debug.Log($"Cannot add modifier '{modifierData.DisplayName}': at capacity ({_maxModifiers})");
            return false;
        }

        _activeModifiers.Add(modifierData);
        modifierData.OnEquipped(new ModifierContext(CurrencyManager));
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log($"Modifier added: {modifierData.Name} ({_activeModifiers.Count}/{_maxModifiers})");
        return true;
    }

    /// <summary>Removes a modifier.</summary>
    public bool RemoveModifier(ModifierData modifier)
    {
        if (modifier == null) return false;

        bool removed = _activeModifiers.Remove(modifier);
        if (removed)
        {
            modifier.OnUnequipped();
            OnModifiersChanged?.Invoke();
            GameEvents.RaiseModifiersChanged();
            Debug.Log($"Modifier removed: {modifier.Name} ({_activeModifiers.Count}/{_maxModifiers})");
        }
        return removed;
    }

    /// <summary>Clears all active modifiers.</summary>
    public void ClearAllModifiers()
    {
        foreach (var modifier in _activeModifiers)
        {
            modifier.OnUnequipped();
        }
        _activeModifiers.Clear();
        OnModifiersChanged?.Invoke();
        GameEvents.RaiseModifiersChanged();
        Debug.Log("All modifiers cleared.");
    }

    /// <summary>Checks if the given ModifierData is currently active.</summary>
    public bool IsModifierOwned(ModifierData modifierData)
    {
        if (modifierData == null) return false;
        return _activeModifiers.Contains(modifierData);
    }

    /// <summary>Gets the sell price for a modifier (half of purchase cost).</summary>
    public int GetSellPrice(ModifierData modifier)
    {
        if (modifier == null) return 0;
        return modifier.Cost / 2;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
