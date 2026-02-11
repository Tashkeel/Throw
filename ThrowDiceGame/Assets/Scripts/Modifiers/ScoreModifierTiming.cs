/// <summary>
/// Defines when a score modifier should be applied during scoring.
/// </summary>
public enum ScoreModifierTiming
{
    /// <summary>
    /// Applied after each individual die is scored.
    /// Receives the single die's score and can modify it.
    /// </summary>
    PerDie,

    /// <summary>
    /// Applied after all dice are scored for a throw.
    /// Receives the total throw score and can modify it.
    /// </summary>
    AfterThrow
}
