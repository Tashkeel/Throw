using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel shown when the game is over.
/// </summary>
public class GameOverPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _roundReachedText;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private Button _restartButton;

    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    protected override void OnShow()
    {
        if (_titleText != null)
        {
            _titleText.text = "GAME OVER";
        }

        if (_gameManager != null)
        {
            if (_roundReachedText != null)
            {
                _roundReachedText.text = $"Reached Round {_gameManager.CurrentRound}";
            }

            if (_finalScoreText != null && _gameManager.ScoreTracker != null)
            {
                _finalScoreText.text = $"Final Score: {_gameManager.ScoreTracker.CurrentScore} / {_gameManager.ScoreTracker.ScoreGoal}";
            }
        }
    }

    private void OnRestartClicked()
    {
        if (_gameManager != null)
        {
            _gameManager.RequestRestart();
        }
    }

    private void OnDestroy()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }
    }
}
