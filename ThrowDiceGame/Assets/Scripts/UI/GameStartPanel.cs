using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel shown at game start with the start button.
/// </summary>
public class GameStartPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private Button _startButton;

    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartClicked);
        }
    }

    private void OnStartClicked()
    {
        if (_gameManager != null)
        {
            _gameManager.StartGame();
        }
    }

    private void OnDestroy()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
        }
    }
}
