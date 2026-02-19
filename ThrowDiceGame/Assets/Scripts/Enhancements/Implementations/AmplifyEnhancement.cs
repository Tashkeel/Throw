using UnityEngine;

/// <summary>
/// Enhancement that multiplies all face values by 1.5.
/// "Amplify" - raw power scaling. Best on already-strong dice.
/// </summary>
[CreateAssetMenu(fileName = "ENH_Amplify", menuName = "Dice Game/Enhancements/Amplify")]
public class AmplifyEnhancement : EnhancementData
{
    [Header("Amplify Settings")]
    [SerializeField]
    [Tooltip("Multiplier applied to every face value (rounded down, minimum 1)")]
    private float _multiplier = 1.5f;

    public override string Name => "Amplify";
    protected override string DefaultDescription => $"Multiply all face values by {_multiplier:F1} (rounded down, min 1). Stronger dice benefit more.";
    public override int MaxDiceCount => 1;

    public override int[] ApplyToDie(int[] currentValues)
    {
        if (currentValues == null) return null;

        int[] modified = new int[currentValues.Length];
        for (int i = 0; i < currentValues.Length; i++)
            modified[i] = Mathf.Max(1, Mathf.FloorToInt(currentValues[i] * _multiplier));

        Debug.Log($"[Amplify] Applied {_multiplier}x multiplier to all faces.");
        return modified;
    }
}
