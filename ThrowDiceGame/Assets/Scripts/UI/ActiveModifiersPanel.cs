using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that displays equipped modifier slots.
/// Shows filled cards for active modifiers and empty placeholders for vacant slots.
/// Sell buttons appear during the shop phase.
/// </summary>
public class ActiveModifiersPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform _modifierContainer;
    [SerializeField] private TextMeshProUGUI _titleText;

    [Header("References")]
    [SerializeField] private ShopManager _shopManager;

    private List<GameObject> _slotObjects = new List<GameObject>();

    private static readonly Color EmptySlotColor = new Color(0.15f, 0.15f, 0.18f, 0.5f);
    private static readonly Color EmptySlotBorderColor = new Color(0.4f, 0.4f, 0.45f, 0.4f);

    private void Start()
    {
        EnsureContainer();

        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged += RefreshDisplay;
        }

        GameEvents.OnShopEntered += OnShopEvent;
        GameEvents.OnShopExited += OnShopEvent;

        RefreshDisplay();
    }

    private void OnDestroy()
    {
        if (ModifierManager.Instance != null)
        {
            ModifierManager.Instance.OnModifiersChanged -= RefreshDisplay;
        }

        GameEvents.OnShopEntered -= OnShopEvent;
        GameEvents.OnShopExited -= OnShopEvent;
    }

    private void OnShopEvent()
    {
        RefreshDisplay();
    }

    private void EnsureContainer()
    {
        if (_modifierContainer != null) return;

        var containerObj = new GameObject("ModifierContainer", typeof(RectTransform));
        containerObj.transform.SetParent(transform, false);

        var hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        _modifierContainer = containerObj.transform;
    }

    /// <summary>
    /// Rebuilds all modifier slots from scratch.
    /// </summary>
    public void RefreshDisplay()
    {
        ClearSlots();

        int maxSlots = 4;
        IReadOnlyList<ModifierData> activeModifiers = null;

        if (ModifierManager.Instance != null)
        {
            maxSlots = ModifierManager.Instance.MaxModifiers;
            activeModifiers = ModifierManager.Instance.ActiveModifiers;
        }

        int filledCount = activeModifiers?.Count ?? 0;

        UpdateTitle(filledCount, maxSlots);

        if (activeModifiers != null)
        {
            foreach (var modifier in activeModifiers)
            {
                CreateFilledSlot(modifier);
            }
        }

        int emptyCount = maxSlots - filledCount;
        for (int i = 0; i < emptyCount; i++)
        {
            CreateEmptySlot();
        }
    }

    private void UpdateTitle(int filled, int max)
    {
        if (_titleText == null)
        {
            var titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(transform, false);
            titleObj.transform.SetAsFirstSibling();
            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.fontSize = 14;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            _titleText.alignment = TextAlignmentOptions.Left;
            var titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 20;
        }

        _titleText.text = $"Modifiers ({filled}/{max})";
    }

    private void CreateFilledSlot(ModifierData modifier)
    {
        if (_modifierContainer == null) return;

        var slotObj = new GameObject($"Slot_{modifier.Name}", typeof(RectTransform));
        slotObj.transform.SetParent(_modifierContainer, false);

        var displayItem = slotObj.AddComponent<ModifierDisplayItem>();
        displayItem.Initialize(modifier, OnSellClicked);
        _slotObjects.Add(slotObj);
    }

    private void CreateEmptySlot()
    {
        if (_modifierContainer == null) return;

        var slotObj = new GameObject("EmptySlot", typeof(RectTransform));
        slotObj.transform.SetParent(_modifierContainer, false);

        var bg = slotObj.AddComponent<Image>();
        bg.color = EmptySlotColor;

        var outline = slotObj.AddComponent<Outline>();
        outline.effectColor = EmptySlotBorderColor;
        outline.effectDistance = new Vector2(1, 1);

        var le = slotObj.AddComponent<LayoutElement>();
        le.preferredWidth = 160;
        le.preferredHeight = 100;
        le.flexibleWidth = 0;

        var textObj = new GameObject("Placeholder", typeof(RectTransform));
        textObj.transform.SetParent(slotObj.transform, false);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Empty";
        text.fontSize = 13;
        text.fontStyle = FontStyles.Italic;
        text.color = new Color(0.5f, 0.5f, 0.55f, 0.5f);
        text.alignment = TextAlignmentOptions.Center;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _slotObjects.Add(slotObj);
    }

    private void OnSellClicked(ModifierData modifier)
    {
        if (_shopManager != null)
        {
            _shopManager.SellModifier(modifier);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in _slotObjects)
        {
            if (slot != null) Destroy(slot);
        }
        _slotObjects.Clear();
    }
}
