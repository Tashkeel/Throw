using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI display for a single shop item (modifier or enhancement).
/// </summary>
public class ShopItemDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private Image _backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(0.7f, 0.7f, 0.7f);

    private Action _onPurchase;
    private Func<bool> _canAfford;

    private void Awake()
    {
        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
    }

    /// <summary>
    /// Initializes the display for a modifier.
    /// </summary>
    public void Initialize(ModifierData modifier, Action onPurchase, Func<bool> canAfford)
    {
        _onPurchase = onPurchase;
        _canAfford = canAfford;

        if (_nameText != null)
            _nameText.text = modifier.DisplayName;

        if (_descriptionText != null)
            _descriptionText.text = modifier.Description;

        if (_costText != null)
            _costText.text = $"${modifier.Cost}";

        if (_iconImage != null && modifier.Icon != null)
            _iconImage.sprite = modifier.Icon;

        UpdateAffordability();
    }

    /// <summary>
    /// Initializes the display for an enhancement.
    /// </summary>
    public void Initialize(EnhancementData enhancement, Action onPurchase, Func<bool> canAfford)
    {
        _onPurchase = onPurchase;
        _canAfford = canAfford;

        if (_nameText != null)
            _nameText.text = enhancement.DisplayName;

        if (_descriptionText != null)
        {
            string diceReq = enhancement.RequiredDiceCount > 1
                ? $" (Select {enhancement.RequiredDiceCount} dice)"
                : "";
            _descriptionText.text = enhancement.Description + diceReq;
        }

        if (_costText != null)
            _costText.text = $"${enhancement.Cost}";

        if (_iconImage != null && enhancement.Icon != null)
            _iconImage.sprite = enhancement.Icon;

        UpdateAffordability();
    }

    /// <summary>
    /// Updates the visual state based on affordability.
    /// </summary>
    public void UpdateAffordability()
    {
        bool canAfford = _canAfford?.Invoke() ?? false;

        if (_purchaseButton != null)
            _purchaseButton.interactable = canAfford;

        if (_backgroundImage != null)
            _backgroundImage.color = canAfford ? _affordableColor : _unaffordableColor;
    }

    private void OnPurchaseClicked()
    {
        _onPurchase?.Invoke();
    }

    private void OnDestroy()
    {
        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
        }
    }
}
