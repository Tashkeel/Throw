/// <summary>
/// Provides side-effect dependencies to modifiers that need them.
/// Passed to OnEquipped so modifiers never need to reach into singletons.
/// </summary>
public interface IModifierContext
{
    /// <summary>Currency manager for modifiers that award money (e.g. EconomyOfScale).</summary>
    ICurrencyManager Currency { get; }

    /// <summary>Round number at time of equip.</summary>
    int CurrentRound { get; }

    /// <summary>Score goal for the current round at time of equip.</summary>
    int CurrentScoreGoal { get; }
}
