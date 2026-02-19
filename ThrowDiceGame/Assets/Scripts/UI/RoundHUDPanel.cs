using TMPro;
using UnityEngine;

/// <summary>
/// Persistent HUD showing round info, score, and throws remaining.
/// Score display interpolates smoothly toward the target value.
/// </summary>
public class RoundHUDPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _goalText;
    [SerializeField] private TextMeshProUGUI _throwsText;
    [SerializeField] private TextMeshProUGUI _inventoryText;
    [SerializeField] private TextMeshProUGUI _modifiersText;
    [SerializeField] private TextMeshProUGUI _moneyText;

    [Header("Score Animation")]
    [SerializeField]
    [Tooltip("Speed of the score count-up interpolation")]
    private float _scoreAnimationSpeed = 8f;

    [SerializeField]
    [Tooltip("Scale multiplier at the peak of the score bump")]
    private float _scoreBumpScale = 1.3f;

    [SerializeField]
    [Tooltip("How fast the score bump decays back to normal")]
    private float _scoreBumpDecaySpeed = 5f;

    private GameManager _gameManager;
    private RoundManager _roundManager;

    private int _targetScore;
    private float _displayedScore;
    private bool _animatingScore;

    private RectTransform _scoreRect;
    private float _scoreBumpT;

    private int _targetMoney;
    private float _displayedMoney;
    private bool _animatingMoney;
    private RectTransform _moneyRect;
    private float _moneyBumpT;

    public void Initialize(GameManager gameManager, RoundManager roundManager)
    {
        _gameManager = gameManager;
        _roundManager = roundManager;
    }

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleScoreChanged;
        GameEvents.OnMoneyChanged += HandleMoneyChanged;
        GameEvents.OnModifiersChanged += UpdateModifierCount;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
        GameEvents.OnMoneyChanged -= HandleMoneyChanged;
        GameEvents.OnModifiersChanged -= UpdateModifierCount;
    }

    private void Update()
    {
        UpdateScoreAnimation();
        UpdateScoreBump();
        UpdateMoneyAnimation();
        UpdateMoneyBump();
    }

    private void UpdateScoreAnimation()
    {
        if (!_animatingScore || _scoreText == null) return;

        _displayedScore = Mathf.MoveTowards(_displayedScore, _targetScore,
            _scoreAnimationSpeed * Time.deltaTime * Mathf.Max(1f, Mathf.Abs(_targetScore - _displayedScore)));

        int shown = Mathf.RoundToInt(_displayedScore);
        _scoreText.text = $"{shown}";

        if (shown == _targetScore)
            _animatingScore = false;
    }

    private void UpdateScoreBump()
    {
        if (_scoreRect == null || _scoreBumpT <= 0f) return;

        _scoreBumpT = Mathf.MoveTowards(_scoreBumpT, 0f, _scoreBumpDecaySpeed * Time.deltaTime);
        float scale = 1f + (_scoreBumpScale - 1f) * _scoreBumpT * _scoreBumpT;
        _scoreRect.localScale = Vector3.one * scale;
    }

    protected override void OnShow()
    {
        UpdateRoundInfo();
    }

    public void UpdateRoundInfo()
    {
        if (_gameManager == null || _roundManager == null) return;

        if (_roundText != null)
        {
            _roundText.text = $"{_gameManager.CurrentRound}";
        }

        if (_scoreText != null && _gameManager.ScoreTracker != null)
        {
            // Snap displayed score to current value when refreshing full HUD
            int current = _gameManager.ScoreTracker.CurrentScore;
            _displayedScore = current;
            _targetScore = current;
            _animatingScore = false;
            _scoreText.text = $"{current}";
        }

        if (_goalText != null && _gameManager.ScoreTracker != null)
        {
            _goalText.text = $"{_gameManager.ScoreTracker.ScoreGoal}";
        }

        if (_throwsText != null)
        {
            _throwsText.text = $"{_roundManager.CurrentThrow}/{_roundManager.MaxThrows}";
        }

        if (_inventoryText != null && _gameManager.Inventory != null)
        {
            _inventoryText.text = $"{_gameManager.Inventory.TotalDiceCount}";
        }

        if (_moneyText != null && _gameManager.CurrencyManager != null)
        {
            int currentMoney = _gameManager.CurrencyManager.CurrentMoney;
            _displayedMoney = currentMoney;
            _targetMoney = currentMoney;
            _animatingMoney = false;
            _moneyText.text = $"${currentMoney}";
        }

        UpdateModifierCount();
    }

    private void UpdateModifierCount()
    {
        if (_modifiersText != null && ModifierManager.Instance != null)
        {
            _modifiersText.text = $"{ModifierManager.Instance.ActiveModifiers.Count}/{ModifierManager.Instance.MaxModifiers}";
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        _targetScore = newScore;
        _animatingScore = true;

        // Trigger scale bump on each new score increment
        if (_scoreRect == null && _scoreText != null)
            _scoreRect = _scoreText.rectTransform;

        _scoreBumpT = 1f;
    }

    private void HandleMoneyChanged(int newMoney)
    {
        _targetMoney = newMoney;
        _animatingMoney = true;

        if (_moneyRect == null && _moneyText != null)
            _moneyRect = _moneyText.rectTransform;

        _moneyBumpT = 1f;
    }

    private void UpdateMoneyAnimation()
    {
        if (!_animatingMoney || _moneyText == null) return;

        _displayedMoney = Mathf.MoveTowards(_displayedMoney, _targetMoney,
            _scoreAnimationSpeed * Time.deltaTime * Mathf.Max(1f, Mathf.Abs(_targetMoney - _displayedMoney)));

        int shown = Mathf.RoundToInt(_displayedMoney);
        _moneyText.text = $"${shown}";

        if (shown == _targetMoney)
            _animatingMoney = false;
    }

    private void UpdateMoneyBump()
    {
        if (_moneyRect == null || _moneyBumpT <= 0f) return;

        _moneyBumpT = Mathf.MoveTowards(_moneyBumpT, 0f, _scoreBumpDecaySpeed * Time.deltaTime);
        float scale = 1f + (_scoreBumpScale - 1f) * _moneyBumpT * _moneyBumpT;
        _moneyRect.localScale = Vector3.one * scale;
    }
}
