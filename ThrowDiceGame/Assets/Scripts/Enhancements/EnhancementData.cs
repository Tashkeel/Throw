using UnityEngine;

/// <summary>
/// ScriptableObject that defines a purchasable enhancement for the shop.
/// Enhancements permanently modify selected dice for the rest of the game.
/// Name and description are pulled from the assigned prefab.
/// </summary>
[CreateAssetMenu(fileName = "NewEnhancement", menuName = "Dice Game/Enhancement")]
public class EnhancementData : ScriptableObject
{
    [Header("Enhancement Implementation")]
    [SerializeField]
    [Tooltip("The enhancement component prefab that implements IEnhancement")]
    private BaseEnhancement _enhancementPrefab;

    [Header("Shop")]
    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    [Tooltip("Cost to purchase this enhancement")]
    private int _cost = 30;

    public string DisplayName => _enhancementPrefab != null ? _enhancementPrefab.Name : "No Prefab";
    public string Description => _enhancementPrefab != null ? _enhancementPrefab.Description : "";
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public BaseEnhancement EnhancementPrefab => _enhancementPrefab;

    /// <summary>
    /// Number of dice required for this enhancement.
    /// </summary>
    public int RequiredDiceCount => _enhancementPrefab != null ? _enhancementPrefab.MaxDiceCount : 1;

    /// <summary>
    /// Creates an instance of this enhancement's implementation.
    /// </summary>
    public IEnhancement CreateEnhancementInstance(Transform parent)
    {
        if (_enhancementPrefab == null)
        {
            Debug.LogError($"EnhancementData '{name}' has no enhancement prefab assigned!");
            return null;
        }

        var instance = Instantiate(_enhancementPrefab, parent);
        instance.name = DisplayName;
        return instance;
    }
}
