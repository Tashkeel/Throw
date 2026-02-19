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
    /// Whether this enhancement creates a duplicate of the selected die after applying.
    /// </summary>
    bool CreatesDuplicateDie { get; }

    /// <summary>
    /// Called before ApplyToDie with all selected dice face values.
    /// Allows the enhancement to analyze multiple dice before individual application.
    /// </summary>
    void PreProcess(System.Collections.Generic.List<int[]> allSelectedDiceFaceValues);

    /// <summary>
    /// Applies the enhancement to a die's face values.
    /// </summary>
    /// <param name="currentValues">Current face values of the die.</param>
    /// <returns>Modified face values.</returns>
    int[] ApplyToDie(int[] currentValues);
}
