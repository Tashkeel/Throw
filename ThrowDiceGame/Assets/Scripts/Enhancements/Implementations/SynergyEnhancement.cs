using UnityEngine;

/// <summary>
/// Enhancement that boosts two dice together - they both get better.
/// "Synergy" - two dice working together.
/// </summary>
[CreateAssetMenu(fileName = "ENH_Synergy", menuName = "Dice Game/Enhancements/Synergy")]
public class SynergyEnhancement : EnhancementData
{
    [Header("Synergy Settings")]
    [SerializeField]
    [Tooltip("Value to add to each face of both dice")]
    private int _valueToAdd = 2;

    public override string Name => "Synergy";
    protected override string DefaultDescription => $"Select 2 dice. Both get +{_valueToAdd} to all faces.";
    public override int MaxDiceCount => 2;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            modifiedValues[i] = currentValues[i] + _valueToAdd;
        }

        Debug.Log($"[Synergy] Applied +{_valueToAdd} synergy bonus to die.");
        return modifiedValues;
    }
}
