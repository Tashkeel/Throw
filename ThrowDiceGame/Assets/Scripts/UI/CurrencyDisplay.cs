using TMPro;
using UnityEngine;

/// <summary>
/// UI component that displays the player's current money.
/// </summary>
public class CurrencyDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private string _prefix = "$";

    private CurrencyManager _currencyManager;

    /// <summary>
    /// Initializes the display with a currency manager.
    /// </summary>
    public void Initialize(CurrencyManager currencyManager)
    {
        _currencyManager = currencyManager;

        if (_currencyManager != null)
        {
            _currencyManager.OnMoneyChanged += UpdateDisplay;
            UpdateDisplay(_currencyManager.CurrentMoney);
        }
    }

    private void Start()
    {
        // Try to find currency manager from GameManager if not initialized
        if (_currencyManager == null)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null && gameManager.CurrencyManager != null)
            {
                Initialize(gameManager.CurrencyManager);
            }
        }
    }

    private void UpdateDisplay(int money)
    {
        if (_moneyText != null)
        {
            _moneyText.text = $"{_prefix}{money}";
        }
    }

    private void OnDestroy()
    {
        if (_currencyManager != null)
        {
            _currencyManager.OnMoneyChanged -= UpdateDisplay;
        }
    }
}
