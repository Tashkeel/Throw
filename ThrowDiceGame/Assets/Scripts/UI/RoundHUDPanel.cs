using TMPro;
using UnityEngine;

/// <summary>
/// Persistent HUD showing round info, score, and throws remaining.
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

    private GameManager _gameManager;
    private RoundManager _roundManager;

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
            _scoreText.text = $"{_gameManager.ScoreTracker.CurrentScore}";
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
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {newScore}";
        }
    }
}
