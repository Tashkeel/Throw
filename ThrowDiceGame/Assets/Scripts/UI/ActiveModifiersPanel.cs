using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that displays currently active modifiers.
/// Visible during both rounds and in the shop.
/// Shows sell buttons during shop phase.
/// </summary>
public class ActiveModifiersPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform _modifierContainer;
    [SerializeField] private GameObject _modifierItemPrefab;
    [SerializeField] private TextMeshProUGUI _noModifiersText;

    [Header("Settings")]
    [SerializeField] private bool _showDescriptions = true;

    [Header("References")]
    [SerializeField] private ShopManager _shopManager;

    private List<GameObject> _modifierItems = new List<GameObject>();
    private List<Button> _sellButtons = new List<Button>();
    private bool _sellEnabled;

    private void Start()
    {
        // Subscribe to modifier changes
        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged += RefreshDisplay;
            RefreshDisplay();
        }

        // Subscribe to shop events for sell button visibility
        GameEvents.OnShopEntered += OnShopEntered;
        GameEvents.OnShopExited += OnShopExited;
    }

    private void OnDestroy()
    {
        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged -= RefreshDisplay;
        }

        GameEvents.OnShopEntered -= OnShopEntered;
        GameEvents.OnShopExited -= OnShopExited;
    }

    private void OnShopEntered()
    {
        _sellEnabled = true;
        RefreshDisplay();
    }

    private void OnShopExited()
    {
        _sellEnabled = false;
        RefreshDisplay();
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
        _sellButtons.Clear();

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
            int sellPrice = ModifierManager.Instance.GetSellPrice(modifier);
            string sellInfo = _sellEnabled && sellPrice > 0 ? $" <color=#FFCC00>(Sell: ${sellPrice})</color>" : "";
            text.text = _showDescriptions
                ? $"<b>{modifier.Name}</b>{sellInfo}\n<size=80%>{modifier.Description}</size>"
                : $"{modifier.Name}{sellInfo}";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;

            var layoutElement = item.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = _showDescriptions ? 50 : 25;
        }

        // Try to populate prefab components
        var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null && _modifierItemPrefab != null)
        {
            int sellPrice = ModifierManager.Instance.GetSellPrice(modifier);
            string sellInfo = _sellEnabled && sellPrice > 0 ? $" <color=#FFCC00>(Sell: ${sellPrice})</color>" : "";
            nameText.text = _showDescriptions
                ? $"<b>{modifier.Name}</b>{sellInfo}\n<size=80%>{modifier.Description}</size>"
                : $"{modifier.Name}{sellInfo}";
        }

        // Add sell button during shop phase
        if (_sellEnabled)
        {
            int sellPrice = ModifierManager.Instance.GetSellPrice(modifier);
            if (sellPrice > 0)
            {
                var sellButtonObj = new GameObject("SellButton");
                sellButtonObj.transform.SetParent(item.transform, false);

                var sellButton = sellButtonObj.AddComponent<Button>();
                var buttonText = sellButtonObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = $"Sell (${sellPrice})";
                buttonText.fontSize = 12;
                buttonText.alignment = TextAlignmentOptions.Center;

                var buttonLayout = sellButtonObj.AddComponent<LayoutElement>();
                buttonLayout.preferredHeight = 25;
                buttonLayout.preferredWidth = 80;

                IScoreModifier capturedModifier = modifier;
                sellButton.onClick.AddListener(() => OnSellClicked(capturedModifier));
                _sellButtons.Add(sellButton);
            }
        }

        _modifierItems.Add(item);
    }

    private void OnSellClicked(IScoreModifier modifier)
    {
        if (_shopManager != null)
        {
            _shopManager.SellModifier(modifier);
            // RefreshDisplay will be called via OnModifiersChanged
        }
    }

    private void ShowNoModifiers(bool show)
    {
        if (_noModifiersText != null)
        {
            _noModifiersText.gameObject.SetActive(show);
        }
    }
}
