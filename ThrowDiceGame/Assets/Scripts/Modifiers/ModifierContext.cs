/// <summary>
/// Concrete implementation of IModifierContext.
/// Created by ModifierManager when equipping a modifier.
/// </summary>
public class ModifierContext : IModifierContext
{
    public ICurrencyManager Currency { get; }
    public int CurrentRound { get; }
    public int CurrentScoreGoal { get; }

    public ModifierContext(ICurrencyManager currency, int currentRound = 0, int currentScoreGoal = 0)
    {
        Currency = currency;
        CurrentRound = currentRound;
        CurrentScoreGoal = currentScoreGoal;
    }
}
