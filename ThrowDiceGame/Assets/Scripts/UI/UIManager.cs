using UnityEngine;

/// <summary>
/// Top-level UI manager that shows/hides panels based on game state.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameStartPanel _gameStartPanel;
    [SerializeField] private RoundHUDPanel _roundHUDPanel;
    [SerializeField] private HandSetupPanel _handSetupPanel;
    [SerializeField] private ThrowingPanel _throwingPanel;
    [SerializeField] private ScoringPanel _scoringPanel;
    [SerializeField] private ShopPanel _shopPanel;
    [SerializeField] private GameOverPanel _gameOverPanel;

    [Header("References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private RoundManager _roundManager;
    [SerializeField] private ShopManager _shopManager;

    private void Awake()
    {
        // Initialize all panels with their dependencies
        if (_gameStartPanel != null) _gameStartPanel.Initialize(_gameManager);
        if (_roundHUDPanel != null) _roundHUDPanel.Initialize(_gameManager, _roundManager);
        if (_handSetupPanel != null) _handSetupPanel.Initialize(_roundManager);
        if (_throwingPanel != null) _throwingPanel.Initialize();
        if (_scoringPanel != null) _scoringPanel.Initialize(_roundManager);
        if (_shopPanel != null) _shopPanel.Initialize(_shopManager);
        if (_gameOverPanel != null) _gameOverPanel.Initialize(_gameManager);

        HideAllPanels();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // Game state events
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnGameOver += HandleGameOver;

        // Round events
        GameEvents.OnRoundStarted += HandleRoundStarted;
        GameEvents.OnRoundCompleted += HandleRoundCompleted;
        GameEvents.OnRoundFailed += HandleRoundFailed;

        // Phase events
        GameEvents.OnHandSetupStarted += HandleHandSetupStarted;
        GameEvents.OnHandSetupCompleted += HandleHandSetupCompleted;
        GameEvents.OnThrowStarted += HandleThrowStarted;
        GameEvents.OnScoringStarted += HandleScoringStarted;
        GameEvents.OnScoringCompleted += HandleScoringCompleted;

        // Shop events
        GameEvents.OnShopEntered += HandleShopEntered;
        GameEvents.OnShopExited += HandleShopExited;
    }

    private void UnsubscribeFromEvents()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnRoundStarted -= HandleRoundStarted;
        GameEvents.OnRoundCompleted -= HandleRoundCompleted;
        GameEvents.OnRoundFailed -= HandleRoundFailed;
        GameEvents.OnHandSetupStarted -= HandleHandSetupStarted;
        GameEvents.OnHandSetupCompleted -= HandleHandSetupCompleted;
        GameEvents.OnThrowStarted -= HandleThrowStarted;
        GameEvents.OnScoringStarted -= HandleScoringStarted;
        GameEvents.OnScoringCompleted -= HandleScoringCompleted;
        GameEvents.OnShopEntered -= HandleShopEntered;
        GameEvents.OnShopExited -= HandleShopExited;
    }

    private void Start()
    {
        // Show start panel initially
        ShowPanel(_gameStartPanel);
    }

    private void HideAllPanels()
    {
        if (_gameStartPanel != null) _gameStartPanel.Hide();
        if (_roundHUDPanel != null) _roundHUDPanel.Hide();
        if (_handSetupPanel != null) _handSetupPanel.Hide();
        if (_throwingPanel != null) _throwingPanel.Hide();
        if (_scoringPanel != null) _scoringPanel.Hide();
        if (_shopPanel != null) _shopPanel.Hide();
        if (_gameOverPanel != null) _gameOverPanel.Hide();
    }

    private void ShowPanel(UIPanel panel)
    {
        if (panel != null) panel.Show();
    }

    private void HidePanel(UIPanel panel)
    {
        if (panel != null) panel.Hide();
    }

    // Event handlers
    private void HandleGameStarted()
    {
        HidePanel(_gameStartPanel);
        ShowPanel(_roundHUDPanel);
    }

    private void HandleGameOver()
    {
        HideAllPanels();
        ShowPanel(_gameOverPanel);
    }

    private void HandleRoundStarted(int round)
    {
        HidePanel(_shopPanel);
        ShowPanel(_roundHUDPanel);
        _roundHUDPanel?.UpdateRoundInfo();
    }

    private void HandleRoundCompleted(int round)
    {
        HidePanel(_scoringPanel);
    }

    private void HandleRoundFailed(int round)
    {
        HidePanel(_scoringPanel);
    }

    private void HandleHandSetupStarted()
    {
        HidePanel(_scoringPanel);
        ShowPanel(_handSetupPanel);
        _handSetupPanel?.Refresh();
    }

    private void HandleHandSetupCompleted()
    {
        HidePanel(_handSetupPanel);
    }

    private void HandleThrowStarted(int throwNumber)
    {
        ShowPanel(_throwingPanel);
        _roundHUDPanel?.UpdateRoundInfo();
    }

    private void HandleScoringStarted(int accumulatedScore)
    {
        HidePanel(_throwingPanel);
        ShowPanel(_scoringPanel);
    }

    private void HandleScoringCompleted(int totalScore, bool goalReached)
    {
        _scoringPanel?.ShowResult(totalScore, goalReached);
        _roundHUDPanel?.UpdateRoundInfo();
    }

    private void HandleShopEntered()
    {
        HideAllPanels();
        ShowPanel(_shopPanel);
    }

    private void HandleShopExited()
    {
        HidePanel(_shopPanel);
    }
}
