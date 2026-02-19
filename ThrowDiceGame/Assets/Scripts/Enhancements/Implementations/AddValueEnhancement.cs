using UnityEngine;

/// <summary>
/// Enhancement that adds a fixed value to all faces of a die.
/// "Power Up" - makes every roll better.
/// </summary>
[CreateAssetMenu(fileName = "ENH_AddValue", menuName = "Dice Game/Enhancements/Add Value")]
public class AddValueEnhancement : EnhancementData
{
    [Header("Add Value Settings")]
    [SerializeField]
    [Tooltip("Value to add to each face")]
    private int _valueToAdd = 1;

    public override string Name => "Power Up";
    protected override string DefaultDescription => $"Adds +{_valueToAdd} to all faces of a die.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            modifiedValues[i] = currentValues[i] + _valueToAdd;
        }

        Debug.Log($"[Power Up] Applied +{_valueToAdd} to all faces of die.");
        return modifiedValues;
    }
}
