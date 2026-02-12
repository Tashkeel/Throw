using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel showing the score result after a throw.
/// </summary>
public class ScoringPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _continueButtonText;

    private RoundManager _roundManager;
    private bool _goalReached;

    public void Initialize(RoundManager roundManager)
    {
        _roundManager = roundManager;

        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    protected override void OnShow()
    {
        // Initial state before result is shown
        if (_resultText != null) _resultText.text = "Calculating...";
        if (_continueButton != null) _continueButton.gameObject.SetActive(false);
    }

    public void ShowResult(int totalScore, bool goalReached)
    {
        _goalReached = goalReached;

        var scoreTracker = _roundManager?.ScoreTracker;
        int goal = scoreTracker?.ScoreGoal ?? 0;
        int remaining = scoreTracker?.ScoreRemaining ?? 0;
        int currentThrow = _roundManager?.CurrentThrow ?? 0;
        int maxThrows = _roundManager?.MaxThrows ?? 3;

        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {totalScore} / {goal}";
        }

        if (goalReached)
        {
            if (_resultText != null) _resultText.text = "GOAL REACHED!";
            if (_messageText != null) _messageText.text = "Round Complete!";
            if (_continueButtonText != null) _continueButtonText.text = "Shop";
        }
        else if (currentThrow >= maxThrows)
        {
            if (_resultText != null) _resultText.text = "OUT OF THROWS";
            if (_messageText != null) _messageText.text = $"Needed {remaining} more points";
            if (_continueButtonText != null) _continueButtonText.text = "Game Over";
        }
        else
        {
            if (_resultText != null) _resultText.text = "Keep Going!";
            if (_messageText != null) _messageText.text = $"Need {remaining} more points\n{maxThrows - currentThrow} throws remaining";
            if (_continueButtonText != null) _continueButtonText.text = "Next Throw";
        }

        if (_continueButton != null)
        {
            _continueButton.gameObject.SetActive(true);
        }
    }

    private void OnContinueClicked()
    {
        // Tell the round manager to proceed
        if (_roundManager != null)
        {
            _roundManager.ConfirmScoring();
        }
        Hide();
    }

    private void OnDestroy()
    {
        if (_continueButton != null)
        {
            _continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}
