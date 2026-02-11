using UnityEngine;

/// <summary>
/// ScriptableObject that defines a purchasable modifier for the shop.
/// References a MonoBehaviour-based modifier implementation.
/// </summary>
[CreateAssetMenu(fileName = "NewModifier", menuName = "Dice Game/Modifier")]
public class ModifierData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string _displayName = "New Modifier";

    [SerializeField]
    [TextArea(2, 4)]
    private string _description = "Description of what this modifier does.";

    [SerializeField]
    private Sprite _icon;

    [Header("Shop")]
    [SerializeField]
    [Tooltip("Cost to purchase this modifier")]
    private int _cost = 50;

    [Header("Modifier Implementation")]
    [SerializeField]
    [Tooltip("The modifier component prefab that implements IScoreModifier")]
    private BaseModifier _modifierPrefab;

    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public BaseModifier ModifierPrefab => _modifierPrefab;

    /// <summary>
    /// Creates an instance of this modifier's implementation.
    /// </summary>
    public IScoreModifier CreateModifierInstance(Transform parent)
    {
        if (_modifierPrefab == null)
        {
            Debug.LogError($"ModifierData '{_displayName}' has no modifier prefab assigned!");
            return null;
        }

        var instance = Instantiate(_modifierPrefab, parent);
        instance.name = _displayName;
        return instance;
    }
}
