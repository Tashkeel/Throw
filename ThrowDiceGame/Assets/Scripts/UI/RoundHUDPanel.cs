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

    public void Initialize(GameManager gameManager, RoundManager roundManager)
    {
        _gameManager = gameManager;
        _roundManager = roundManager;
    }

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleScoreChanged;
        GameEvents.OnModifiersChanged += UpdateModifierCount;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
        GameEvents.OnModifiersChanged -= UpdateModifierCount;
    }

    private void Update()
    {
        UpdateScoreAnimation();
        UpdateScoreBump();
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
}
