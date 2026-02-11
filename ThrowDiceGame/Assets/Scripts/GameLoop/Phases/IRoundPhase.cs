/// <summary>
/// Interface for phases within a round.
/// Implements the State pattern for round phase management.
/// </summary>
public interface IRoundPhase
{
    /// <summary>
    /// The type of phase this represents.
    /// </summary>
    RoundPhase PhaseType { get; }

    /// <summary>
    /// Called when entering this phase.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called every frame while in this phase.
    /// </summary>
    void Update();

    /// <summary>
    /// Called when exiting this phase.
    /// </summary>
    void Exit();

    /// <summary>
    /// Returns true when this phase is complete and ready to transition.
    /// </summary>
    bool IsComplete { get; }
}
