using UnityEngine;

/// <summary>
/// Abstract ScriptableObject base for all score modifiers.
/// Subclass this and add [CreateAssetMenu] to create a modifier asset directly —
/// no separate prefab or MonoBehaviour required.
/// </summary>
public abstract class ModifierData : ScriptableObject, IScoreModifier
{
    [Header("Shop")]
    [SerializeField] private Sprite _icon;

    [SerializeField]
    [Tooltip("Cost to purchase this modifier in the shop")]
    private int _cost = 50;

    [SerializeField]
    [Tooltip("Rarity tier — affects draw probability in the shop")]
    private ItemRarity _rarity = ItemRarity.Common;

    [SerializeField]
    [TextArea(2, 4)]
    [Tooltip("Override the description shown in the shop. Leave blank to use the built-in default.")]
    private string _description = "";

    // ── IScoreModifier ────────────────────────────────────────────────────────
    public abstract string Name { get; }

    /// <summary>
    /// Description shown in the shop. Editable per-asset in the Inspector.
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// Built-in description used to seed the Inspector field on first creation.
    /// Override in subclasses to provide the default text.
    /// </summary>
    protected virtual string DefaultDescription => "";

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(_description))
            _description = DefaultDescription;
    }

    public abstract ScoreModifierTiming Timing { get; }

    public abstract int ModifyScore(ScoreModifierContext context);

    // ── Shop properties ───────────────────────────────────────────────────────
    /// <summary>Alias for Name — used by UI and shop code.</summary>
    public string DisplayName => Name;
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public ItemRarity Rarity => _rarity;

    // ── Lifecycle hooks ───────────────────────────────────────────────────────
    /// <summary>
    /// Called by ModifierManager when this modifier is purchased and made active.
    /// Override to subscribe to GameEvents or reset per-run state.
    /// </summary>
    public virtual void OnActivated() { }

    /// <summary>
    /// Called by ModifierManager when this modifier is sold or cleared.
    /// Override to unsubscribe from GameEvents.
    /// </summary>
    public virtual void OnDeactivated() { }
}
