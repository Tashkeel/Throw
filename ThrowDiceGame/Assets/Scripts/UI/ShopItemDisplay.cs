using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI display for a single shop item (modifier or enhancement).
/// Handles click, hover-enter, and hover-exit events.
/// </summary>
public class ShopItemDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _rarityText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color _soldOutColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Action _onPurchase;
    private Func<bool> _canAfford;
    private Action<string, string, ItemRarity> _onHoverEnter;
    private Action _onHoverExit;
    private bool _isAffordable;
    private bool _isSoldOut;

    // Stored item data for hover callbacks
    private string _storedName;
    private string _storedDescription;
    private ItemRarity _storedRarity;

    /// <summary>
    /// Initializes the display for a modifier.
    /// </summary>
    public void Initialize(ModifierData modifier, Action onPurchase, Func<bool> canAfford,
        Action<string, string, ItemRarity> onHoverEnter = null, Action onHoverExit = null)
    {
        _onPurchase = onPurchase;
        _canAfford = canAfford;
        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;

        _storedName = modifier.DisplayName;
        _storedDescription = modifier.Description;
        _storedRarity = modifier.Rarity;

        if (_nameText != null)
            _nameText.text = modifier.DisplayName;

        if (_descriptionText != null)
            _descriptionText.text = modifier.Description;

        if (_costText != null)
            _costText.text = $"${modifier.Cost}";

        if (_iconImage != null && modifier.Icon != null)
            _iconImage.sprite = modifier.Icon;

        ApplyRarityLabel(modifier.Rarity);
        UpdateAffordability();
    }

    /// <summary>
    /// Initializes the display for an enhancement.
    /// </summary>
    public void Initialize(EnhancementData enhancement, Action onPurchase, Func<bool> canAfford,
        Action<string, string, ItemRarity> onHoverEnter = null, Action onHoverExit = null)
    {
        _onPurchase = onPurchase;
        _canAfford = canAfford;
        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;

        _storedName = enhancement.DisplayName;
        _storedRarity = enhancement.Rarity;

        string diceReq = enhancement.RequiredDiceCount > 1
            ? $" (Select {enhancement.RequiredDiceCount} dice)"
            : "";
        _storedDescription = enhancement.Description + diceReq;

        if (_nameText != null)
            _nameText.text = enhancement.DisplayName;

        if (_descriptionText != null)
            _descriptionText.text = _storedDescription;

        if (_costText != null)
            _costText.text = $"${enhancement.Cost}";

        if (_iconImage != null && enhancement.Icon != null)
            _iconImage.sprite = enhancement.Icon;

        ApplyRarityLabel(enhancement.Rarity);
        UpdateAffordability();
    }

    /// <summary>
    /// Marks this item as sold out, preventing further interaction.
    /// </summary>
    public void MarkSoldOut()
    {
        _isSoldOut = true;
        _isAffordable = false;

        if (_backgroundImage != null)
            _backgroundImage.color = _soldOutColor;

        if (_costText != null)
            _costText.text = "SOLD";
    }

    /// <summary>
    /// Updates the visual state based on affordability.
    /// </summary>
    public void UpdateAffordability()
    {
        if (_isSoldOut) return;

        _isAffordable = _canAfford?.Invoke() ?? false;

        if (_backgroundImage != null)
            _backgroundImage.color = _isAffordable ? _affordableColor : _unaffordableColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isAffordable || _isSoldOut) return;

        _onPurchase?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _onHoverEnter?.Invoke(_storedName, _storedDescription, _storedRarity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _onHoverExit?.Invoke();
    }

    private void ApplyRarityLabel(ItemRarity rarity)
    {
        if (_rarityText == null) return;

        _rarityText.text = rarity.ToString();
        _rarityText.color = GetRarityColor(rarity);
    }

    /// <summary>
    /// Returns the display colour for a given rarity tier.
    /// </summary>
    public static Color GetRarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon  => new Color(0.33f, 0.80f, 0.33f),
        ItemRarity.Rare      => new Color(0.40f, 0.60f, 1.00f),
        ItemRarity.Legendary => new Color(1.00f, 0.65f, 0.00f),
        _                    => new Color(0.75f, 0.75f, 0.75f),
    };
}
