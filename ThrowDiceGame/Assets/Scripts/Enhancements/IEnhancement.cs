/// <summary>
/// Interface for enhancements that permanently modify dice.
/// </summary>
public interface IEnhancement
{
    /// <summary>
    /// Display name of the enhancement.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the enhancement does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Number of dice this enhancement must be applied to.
    /// </summary>
    int MaxDiceCount { get; }

    /// <summary>
    /// Applies the enhancement to a die's face values.
    /// </summary>
    /// <param name="currentValues">Current face values of the die.</param>
    /// <returns>Modified face values.</returns>
    int[] ApplyToDie(int[] currentValues);
}
