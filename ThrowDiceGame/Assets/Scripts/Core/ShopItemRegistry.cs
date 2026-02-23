#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject registry of all shop items (modifiers and enhancements).
/// Assign to ShopManager instead of manually wiring two separate lists.
/// Use the "Auto-Discover All Items" context menu to populate automatically.
/// </summary>
[CreateAssetMenu(menuName = "Dice Game/Shop Item Registry", fileName = "ShopItemRegistry")]
public class ShopItemRegistry : ScriptableObject
{
    [SerializeField]
    [Tooltip("All modifier assets available for purchase")]
    private List<ModifierData> _modifiers = new List<ModifierData>();

    [SerializeField]
    [Tooltip("All enhancement assets available for purchase")]
    private List<EnhancementData> _enhancements = new List<EnhancementData>();

    public IReadOnlyList<ModifierData> Modifiers => _modifiers;
    public IReadOnlyList<EnhancementData> Enhancements => _enhancements;

#if UNITY_EDITOR
    [ContextMenu("Auto-Discover All Items")]
    private void DiscoverAllItems()
    {
        _modifiers.Clear();
        var modifierGuids = AssetDatabase.FindAssets("t:ModifierData");
        foreach (var guid in modifierGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ModifierData>(path);
            if (asset != null)
                _modifiers.Add(asset);
        }

        _enhancements.Clear();
        var enhancementGuids = AssetDatabase.FindAssets("t:EnhancementData");
        foreach (var guid in enhancementGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<EnhancementData>(path);
            if (asset != null)
                _enhancements.Add(asset);
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[ShopItemRegistry] Discovered {_modifiers.Count} modifiers and {_enhancements.Count} enhancements.");
    }
#endif
}
