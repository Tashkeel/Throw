using System.Collections.Generic;

/// <summary>
/// Immutable context passed to IAfterThrowModifier.ApplyAfterThrow.
/// Describes the state of the whole throw once all dice have settled.
/// </summary>
public readonly struct AfterThrowContext
{
    /// <summary>Accumulated score after the per-die phase. Modify this value by returning a new int.</summary>
    public readonly int TotalScore;

    /// <summary>Per-die modified scores (one entry per score die, post-per-die-modifiers).</summary>
    public readonly IReadOnlyList<int> PerDieValues;

    /// <summary>Original raw die values from the throw (useful for combo detection).</summary>
    public readonly IReadOnlyList<int> RawDieValues;

    /// <summary>Current throw number (1-based).</summary>
    public readonly int ThrowNumber;

    /// <summary>Current round number (1-based).</summary>
    public readonly int RoundNumber;

    public AfterThrowContext(
        int totalScore,
        IReadOnlyList<int> perDieValues,
        IReadOnlyList<int> rawDieValues,
        int throwNumber,
        int roundNumber)
    {
        TotalScore = totalScore;
        PerDieValues = perDieValues;
        RawDieValues = rawDieValues;
        ThrowNumber = throwNumber;
        RoundNumber = roundNumber;
    }
}
