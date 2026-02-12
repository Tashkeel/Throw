using UnityEngine;

/// <summary>
/// ScriptableObject that defines a purchasable enhancement for the shop.
/// Enhancements permanently modify selected dice for the rest of the game.
/// </summary>
[CreateAssetMenu(fileName = "NewEnhancement", menuName = "Dice Game/Enhancement")]
public class EnhancementData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string _displayName = "New Enhancement";

    [SerializeField]
    [TextArea(2, 4)]
    private string _description = "Description of what this enhancement does.";

    [SerializeField]
    private Sprite _icon;

    [Header("Shop")]
    [SerializeField]
    [Tooltip("Cost to purchase this enhancement")]
    private int _cost = 30;

    [Header("Enhancement Implementation")]
    [SerializeField]
    [Tooltip("The enhancement component prefab that implements IEnhancement")]
    private BaseEnhancement _enhancementPrefab;

    public string DisplayName => _displayName;
    public string Description => _description;
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
            Debug.LogError($"EnhancementData '{_displayName}' has no enhancement prefab assigned!");
            return null;
        }

        var instance = Instantiate(_enhancementPrefab, parent);
        instance.name = _displayName;
        return instance;
    }
}
