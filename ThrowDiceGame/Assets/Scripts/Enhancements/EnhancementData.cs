using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract ScriptableObject base for all dice enhancements.
/// Subclass this and add [CreateAssetMenu] to create an enhancement asset directly —
/// no separate prefab or MonoBehaviour required.
/// </summary>
public abstract class EnhancementData : ScriptableObject, IEnhancement
{
    [Header("Shop")]
    [SerializeField] private Sprite _icon;

    [SerializeField]
    [Tooltip("Cost to purchase this enhancement in the shop")]
    private int _cost = 30;

    [SerializeField]
    [Tooltip("Rarity tier — affects draw probability in the shop")]
    private ItemRarity _rarity = ItemRarity.Common;

    [SerializeField]
    [TextArea(2, 4)]
    [Tooltip("Override the description shown in the shop. Leave blank to use the built-in default.")]
    private string _description = "";

    // ── IEnhancement ──────────────────────────────────────────────────────────
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

    public abstract int MaxDiceCount { get; }

    public virtual bool CreatesDuplicateDie => false;

    public virtual void PreProcess(List<int[]> allSelectedDiceFaceValues) { }

    public abstract int[] ApplyToDie(int[] currentValues);

    // ── Shop properties ───────────────────────────────────────────────────────
    /// <summary>Alias for Name — used by UI and shop code.</summary>
    public string DisplayName => Name;

    /// <summary>Alias for MaxDiceCount — used by ShopManager and UI.</summary>
    public int RequiredDiceCount => MaxDiceCount;

    public Sprite Icon => _icon;
    public int Cost => _cost;
    public ItemRarity Rarity => _rarity;
}
