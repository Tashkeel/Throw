using System.Collections.Generic;

/// <summary>
/// Immutable context passed to IPerDieModifier.ApplyPerDie.
/// Describes the state of a single die as it settles.
/// </summary>
public readonly struct PerDieContext
{
    /// <summary>Current value of this die after previous per-die modifiers.</summary>
    public readonly int CurrentValue;

    /// <summary>Original raw die value before any modifications.</summary>
    public readonly int OriginalValue;

    /// <summary>Zero-based index of this die in the overall throw (includes money dice).</summary>
    public readonly int DieIndex;

    /// <summary>Expected total number of dice in this throw.</summary>
    public readonly int TotalDiceCount;

    /// <summary>Raw values of all score dice settled so far (includes this die).</summary>
    public readonly IReadOnlyList<int> AllDieValues;

    /// <summary>Current throw number (1-based).</summary>
    public readonly int ThrowNumber;

    /// <summary>Current round number (1-based).</summary>
    public readonly int RoundNumber;

    public PerDieContext(
        int currentValue,
        int originalValue,
        int dieIndex,
        int totalDiceCount,
        IReadOnlyList<int> allDieValues,
        int throwNumber,
        int roundNumber)
    {
        CurrentValue = currentValue;
        OriginalValue = originalValue;
        DieIndex = dieIndex;
        TotalDiceCount = totalDiceCount;
        AllDieValues = allDieValues;
        ThrowNumber = throwNumber;
        RoundNumber = roundNumber;
    }
}
