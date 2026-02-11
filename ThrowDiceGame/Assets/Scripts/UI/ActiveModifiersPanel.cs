using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that displays currently active modifiers.
/// Visible during both rounds and in the shop.
/// </summary>
public class ActiveModifiersPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform _modifierContainer;
    [SerializeField] private GameObject _modifierItemPrefab;
    [SerializeField] private TextMeshProUGUI _noModifiersText;

    [Header("Settings")]
    [SerializeField] private bool _showDescriptions = true;

    private List<GameObject> _modifierItems = new List<GameObject>();

    private void Start()
    {
        // Subscribe to modifier changes
        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged += RefreshDisplay;
            RefreshDisplay();
        }
    }

    private void OnDestroy()
    {
        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged -= RefreshDisplay;
        }
    }

    /// <summary>
    /// Refreshes the display of active modifiers.
    /// </summary>
    public void RefreshDisplay()
    {
        // Clear existing items
        foreach (var item in _modifierItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _modifierItems.Clear();

        if (ModifierManager.Instance == null)
        {
            ShowNoModifiers(true);
            return;
        }

        var activeModifiers = ModifierManager.Instance.ActiveModifiers;

        if (activeModifiers.Count == 0)
        {
            ShowNoModifiers(true);
            return;
        }

        ShowNoModifiers(false);

        // Create display for each modifier
        foreach (var modifier in activeModifiers)
        {
            CreateModifierItem(modifier);
        }
    }

    private void CreateModifierItem(IScoreModifier modifier)
    {
        if (_modifierContainer == null) return;

        GameObject item;

        if (_modifierItemPrefab != null)
        {
            item = Instantiate(_modifierItemPrefab, _modifierContainer);
        }
        else
        {
            // Create a simple text display if no prefab
            item = new GameObject(modifier.Name);
            item.transform.SetParent(_modifierContainer, false);

            var text = item.AddComponent<TextMeshProUGUI>();
            text.text = _showDescriptions
                ? $"<b>{modifier.Name}</b>\n<size=80%>{modifier.Description}</size>"
                : modifier.Name;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;

            var layoutElement = item.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = _showDescriptions ? 50 : 25;
        }

        // Try to populate prefab components
        var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = _showDescriptions
                ? $"<b>{modifier.Name}</b>\n<size=80%>{modifier.Description}</size>"
                : modifier.Name;
        }

        _modifierItems.Add(item);
    }

    private void ShowNoModifiers(bool show)
    {
        if (_noModifiersText != null)
        {
            _noModifiersText.gameObject.SetActive(show);
        }
    }
}
