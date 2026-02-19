using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Individual modifier card UI element displayed in the ActiveModifiersPanel.
/// Shows modifier info and a sell button during shop phase.
/// </summary>
public class ModifierDisplayItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _backgroundImage;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descriptionText;
    private TextMeshProUGUI _timingText;
    private Image _iconImage;
    private Button _sellButton;
    private TextMeshProUGUI _sellButtonText;
    private GameObject _sellButtonObj;

    private IScoreModifier _modifier;
    private ModifierData _data;
    private Action<IScoreModifier> _onSellClicked;
    private bool _isInShop;

    private static readonly Color NormalColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
    private static readonly Color HoverColor = new Color(0.3f, 0.3f, 0.38f, 0.95f);
    private static readonly Color SellHoverColor = new Color(0.5f, 0.2f, 0.2f, 0.95f);

    /// <summary>
    /// Builds the card UI hierarchy programmatically and initializes with modifier data.
    /// </summary>
    public void Initialize(IScoreModifier modifier, ModifierData data, Action<IScoreModifier> onSellClicked)
    {
        _modifier = modifier;
        _data = data;
        _onSellClicked = onSellClicked;

        BuildUI();
        PopulateData();

        GameEvents.OnShopEntered += OnShopEntered;
        GameEvents.OnShopExited += OnShopExited;
    }

    private void OnDestroy()
    {
        GameEvents.OnShopEntered -= OnShopEntered;
        GameEvents.OnShopExited -= OnShopExited;
    }

    private void BuildUI()
    {
        // Root setup
        var rootRect = GetComponent<RectTransform>();
        if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();

        _backgroundImage = gameObject.AddComponent<Image>();
        _backgroundImage.color = NormalColor;

        var layoutElement = gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 160;
        layoutElement.preferredHeight = 100;
        layoutElement.flexibleWidth = 0;

        var vertLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        vertLayout.padding = new RectOffset(6, 6, 4, 4);
        vertLayout.spacing = 2;
        vertLayout.childAlignment = TextAnchor.UpperLeft;
        vertLayout.childControlWidth = true;
        vertLayout.childControlHeight = false;
        vertLayout.childForceExpandWidth = true;
        vertLayout.childForceExpandHeight = false;

        // Header row (icon + name)
        var header = CreateChild("Header", gameObject.transform);
        var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 4;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childControlWidth = false;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;
        var headerLE = header.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 22;

        // Icon
        var iconObj = CreateChild("Icon", header.transform);
        _iconImage = iconObj.AddComponent<Image>();
        _iconImage.color = new Color(1, 1, 1, 0.6f);
        var iconLE = iconObj.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 20;
        iconLE.preferredHeight = 20;

        // Name
        var nameObj = CreateChild("Name", header.transform);
        _nameText = nameObj.AddComponent<TextMeshProUGUI>();
        _nameText.fontSize = 13;
        _nameText.fontStyle = FontStyles.Bold;
        _nameText.color = Color.white;
        _nameText.alignment = TextAlignmentOptions.Left;
        _nameText.textWrappingMode = TextWrappingModes.NoWrap;
        _nameText.overflowMode = TextOverflowModes.Ellipsis;
        var nameLE = nameObj.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1;
        nameLE.preferredHeight = 22;

        // Description
        var descObj = CreateChild("Description", gameObject.transform);
        _descriptionText = descObj.AddComponent<TextMeshProUGUI>();
        _descriptionText.fontSize = 10;
        _descriptionText.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
        _descriptionText.alignment = TextAlignmentOptions.TopLeft;
        _descriptionText.textWrappingMode = TextWrappingModes.Normal;
        _descriptionText.overflowMode = TextOverflowModes.Truncate;
        var descLE = descObj.AddComponent<LayoutElement>();
        descLE.preferredHeight = 32;
        descLE.flexibleHeight = 1;

        // Timing badge
        var timingObj = CreateChild("Timing", gameObject.transform);
        _timingText = timingObj.AddComponent<TextMeshProUGUI>();
        _timingText.fontSize = 9;
        _timingText.fontStyle = FontStyles.Italic;
        _timingText.color = new Color(0.6f, 0.75f, 1f, 0.8f);
        _timingText.alignment = TextAlignmentOptions.Left;
        var timingLE = timingObj.AddComponent<LayoutElement>();
        timingLE.preferredHeight = 14;

        // Sell button (hidden by default)
        _sellButtonObj = CreateChild("SellButton", gameObject.transform);
        var sellBg = _sellButtonObj.AddComponent<Image>();
        sellBg.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);
        _sellButton = _sellButtonObj.AddComponent<Button>();
        _sellButton.targetGraphic = sellBg;
        var sellBtnLE = _sellButtonObj.AddComponent<LayoutElement>();
        sellBtnLE.preferredHeight = 20;

        var sellTextObj = CreateChild("Text", _sellButtonObj.transform);
        _sellButtonText = sellTextObj.AddComponent<TextMeshProUGUI>();
        _sellButtonText.fontSize = 11;
        _sellButtonText.fontStyle = FontStyles.Bold;
        _sellButtonText.color = Color.white;
        _sellButtonText.alignment = TextAlignmentOptions.Center;
        var sellTextRect = sellTextObj.GetComponent<RectTransform>();
        sellTextRect.anchorMin = Vector2.zero;
        sellTextRect.anchorMax = Vector2.one;
        sellTextRect.offsetMin = Vector2.zero;
        sellTextRect.offsetMax = Vector2.zero;

        _sellButton.onClick.AddListener(OnSellPressed);
        _sellButtonObj.SetActive(false);
    }

    private void PopulateData()
    {
        if (_modifier == null) return;

        _nameText.text = _modifier.Name;
        _descriptionText.text = _modifier.Description;
        _timingText.text = _modifier.Timing == ScoreModifierTiming.PerDie ? "Per Die" : "After Throw";

        if (_data != null && _data.Icon != null)
        {
            _iconImage.sprite = _data.Icon;
            _iconImage.color = Color.white;
        }
        else
        {
            _iconImage.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
        }

        UpdateSellButton();
    }

    private void UpdateSellButton()
    {
        if (_sellButtonObj == null) return;

        _sellButtonObj.SetActive(_isInShop);

        if (_isInShop && _data != null)
        {
            int sellPrice = _data.Cost / 2;
            _sellButtonText.text = sellPrice > 0 ? $"Sell ${sellPrice}" : "Sell";
        }
    }

    private void OnShopEntered()
    {
        _isInShop = true;
        UpdateSellButton();
    }

    private void OnShopExited()
    {
        _isInShop = false;
        UpdateSellButton();
    }

    private void OnSellPressed()
    {
        _onSellClicked?.Invoke(_modifier);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _isInShop ? SellHoverColor : HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = NormalColor;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }
}
