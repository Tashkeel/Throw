using System.Collections.Generic;

/// <summary>
/// Abstraction for the two-phase scoring pipeline.
/// Injected into DiceManager to decouple it from ModifierManager.
/// To add a new timing phase in future, add a method here and implement it in
/// ModifierManager — existing code is unaffected (OCP).
/// </summary>
public interface IScoringPipeline
{
    /// <summary>
    /// Applies per-die modifiers to a single die's raw value.
    /// Called as each die settles.
    /// </summary>
    int ProcessPerDie(int raw, int dieIndex, IReadOnlyList<int> allValues, int throwNum, int roundNum);

    /// <summary>
    /// Applies after-throw modifiers to the accumulated score.
    /// Called once all dice have settled.
    /// </summary>
    int ProcessAfterThrow(int total, IReadOnlyList<int> perDieValues, IReadOnlyList<int> rawValues, int throwNum, int roundNum);
}
