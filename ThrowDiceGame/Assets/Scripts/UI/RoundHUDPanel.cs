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
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
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
            _roundText.text = $"Round {_gameManager.CurrentRound}";
        }

        if (_scoreText != null && _gameManager.ScoreTracker != null)
        {
            _scoreText.text = $"Score: {_gameManager.ScoreTracker.CurrentScore}";
        }

        if (_goalText != null && _gameManager.ScoreTracker != null)
        {
            _goalText.text = $"Goal: {_gameManager.ScoreTracker.ScoreGoal}";
        }

        if (_throwsText != null)
        {
            _throwsText.text = $"Throw: {_roundManager.CurrentThrow}/{_roundManager.MaxThrows}";
        }

        if (_inventoryText != null && _gameManager.Inventory != null)
        {
            _inventoryText.text = $"Dice: {_gameManager.Inventory.TotalDiceCount}";
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
