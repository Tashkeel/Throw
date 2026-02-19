using UnityEngine;

/// <summary>
/// ScriptableObject that defines a purchasable modifier for the shop.
/// References a MonoBehaviour-based modifier implementation.
/// Name and description are pulled from the assigned prefab.
/// </summary>
[CreateAssetMenu(fileName = "NewModifier", menuName = "Dice Game/Modifier")]
public class ModifierData : ScriptableObject
{
    [Header("Modifier Implementation")]
    [SerializeField]
    [Tooltip("The modifier component prefab that implements IScoreModifier")]
    private BaseModifier _modifierPrefab;

    [Header("Shop")]
    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    [Tooltip("Cost to purchase this modifier")]
    private int _cost = 50;

    public string DisplayName => _modifierPrefab != null ? _modifierPrefab.Name : "No Prefab";
    public string Description => _modifierPrefab != null ? _modifierPrefab.Description : "";
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
            Debug.LogError($"ModifierData '{name}' has no modifier prefab assigned!");
            return null;
        }

        var instance = Instantiate(_modifierPrefab, parent);
        instance.name = DisplayName;
        return instance;
    }
}
