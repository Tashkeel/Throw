/// <summary>
/// Applied to each die as it settles.
/// Return the modified die value from ApplyPerDie.
/// </summary>
public interface IPerDieModifier : IModifier
{
    int ApplyPerDie(PerDieContext ctx);
}
