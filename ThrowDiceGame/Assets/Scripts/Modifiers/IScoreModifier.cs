/// <summary>
/// Interface for score modifiers that can alter scoring during gameplay.
/// Implement this interface to create custom scoring rules.
/// </summary>
public interface IScoreModifier
{
    /// <summary>
    /// Display name of the modifier.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the modifier does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// When this modifier should be applied.
    /// </summary>
    ScoreModifierTiming Timing { get; }

    /// <summary>
    /// Modifies a score value.
    /// </summary>
    /// <param name="context">Context information about the current scoring.</param>
    /// <returns>The modified score.</returns>
    int ModifyScore(ScoreModifierContext context);
}

/// <summary>
/// Context information passed to modifiers during scoring.
/// </summary>
public struct ScoreModifierContext
{
    /// <summary>
    /// The current score value to potentially modify.
    /// For PerDie timing, this is the single die's value.
    /// For AfterThrow timing, this is the total throw score.
    /// </summary>
    public int CurrentScore;

    /// <summary>
    /// The original/base value before any modifications.
    /// </summary>
    public int OriginalValue;

    /// <summary>
    /// For PerDie timing: which die index (0-based) is being scored.
    /// For AfterThrow timing: -1.
    /// </summary>
    public int DieIndex;

    /// <summary>
    /// Total number of dice in this throw.
    /// </summary>
    public int TotalDiceCount;

    /// <summary>
    /// All die values in this throw (useful for combo detection).
    /// </summary>
    public int[] AllDieValues;

    /// <summary>
    /// Current throw number (1-based).
    /// </summary>
    public int ThrowNumber;

    /// <summary>
    /// Current round number (1-based).
    /// </summary>
    public int RoundNumber;
}
