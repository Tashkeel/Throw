using UnityEngine;

/// <summary>
/// Enhancement that boosts two dice together - they both get better.
/// "Synergy" - two dice working together.
/// </summary>
public class SynergyEnhancement : BaseEnhancement
{
    [Header("Synergy Settings")]
    [SerializeField]
    [Tooltip("Value to add to each face of both dice")]
    private int _valueToAdd = 2;

    private void Reset()
    {
        _name = "Synergy";
        _description = "Select 2 dice. Both get +2 to all faces.";
        _maxDiceCount = 2;
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modifiedValues = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            modifiedValues[i] = currentValues[i] + _valueToAdd;
        }

        Debug.Log($"[{_name}] Applied +{_valueToAdd} synergy bonus to die.");
        return modifiedValues;
    }
}
