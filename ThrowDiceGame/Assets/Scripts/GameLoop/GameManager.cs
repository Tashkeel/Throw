using System;
using UnityEngine;

/// <summary>
/// Top-level manager that controls the overall game flow.
/// Coordinates rounds, shop, and game over states.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    [Tooltip("Starting number of dice in inventory")]
    private int _startingDiceCount = 15;

    [SerializeField]
    [Tooltip("Base score goal for round 1")]
    private int _baseScoreGoal = 20;

    [SerializeField]
    [Tooltip("Score goal increase per round")]
    private int _scoreIncreasePerRound = 10;

    [Header("Currency")]
    [SerializeField]
    [Tooltip("Money earned per remaining die (hand + inventory) after a successful round")]
    private int _moneyPerDieRemaining = 10;

    [Header("References")]
    [SerializeField]
    private RoundManager _roundManager;

    [SerializeField]
    private ShopManager _shopManager;

    [SerializeField]
    private ModifierManager _modifierManager;

    [SerializeField]
    [Tooltip("Default dice type for starting inventory")]
    private DiceData _defaultDiceData;

    private GameState _currentState = GameState.NotStarted;
    private DiceInventory _inventory;
    private ScoreTracker _scoreTracker;
    private CurrencyManager _currencyManager;
    private int _currentRound;

    /// <summary>
    /// Current state of the game.
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// Current round number (1-based).
    /// </summary>
    public int CurrentRound => _currentRound;

    /// <summary>
    /// The player's dice inventory.
    /// </summary>
    public DiceInventory Inventory => _inventory;

    /// <summary>
    /// The score tracker.
    /// </summary>
    public ScoreTracker ScoreTracker => _scoreTracker;

    /// <summary>
    /// The currency manager.
    /// </summary>
    public CurrencyManager CurrencyManager => _currencyManager;

    /// <summary>
    /// The modifier manager.
    /// </summary>
    public ModifierManager ModifierManager => _modifierManager;

    /// <summary>
    /// Event fired when game state changes.
    /// </summary>
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        InitializeSystems();
    }

    private void Start()
    {
        // Wait for UI to trigger StartGame()
    }

    private void InitializeSystems()
    {
        // Create inventory
        _inventory = new DiceInventory();

        // Create score tracker
        _scoreTracker = new ScoreTracker(_baseScoreGoal, _scoreIncreasePerRound);

        // Create currency manager
        _currencyManager = new CurrencyManager(_moneyPerDieRemaining);

        // Initialize managers
        if (_roundManager != null)
        {
            _roundManager.Initialize(_inventory, _scoreTracker, _currencyManager);
            _roundManager.OnRoundEnded += HandleRoundEnded;
        }

        if (_shopManager != null)
        {
            _shopManager.Initialize(_inventory, _currencyManager, _modifierManager);
            _shopManager.OnShopClosed += HandleShopClosed;
        }
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("========== GAME START ==========");

        _currentRound = 0;

        // Initialize inventory with starting dice
        if (_defaultDiceData == null)
        {
            _defaultDiceData = DiceData.CreateDefaultDie();
        }
        _inventory.Initialize(DiceData.CreateDie(_defaultDiceData), _startingDiceCount);

        Debug.Log($"Starting with {_inventory.TotalDiceCount} dice");

        SetState(GameState.InRound);
        GameEvents.RaiseGameStarted();

        StartNextRound();
    }

    /// <summary>
    /// Restarts the game after game over.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("========== GAME RESTART ==========");
        StartGame();
    }

    private void StartNextRound()
    {
        _currentRound++;
        SetState(GameState.InRound);

        if (_roundManager != null)
        {
            _roundManager.StartRound(_currentRound);
        }
    }

    private void HandleRoundEnded(bool won)
    {
        if (won)
        {
            // Award money for remaining dice (hand + inventory), captured before hand was returned
            int remainingDice = _roundManager.RemainingDiceAtRoundEnd;
            if (remainingDice > 0)
            {
                _currencyManager.AwardRoundBonus(remainingDice);
            }

            // Go to shop before next round
            EnterShop();
        }
        else
        {
            // Game over
            GameOver();
        }
    }

    private void EnterShop()
    {
        SetState(GameState.InShop);

        if (_shopManager != null)
        {
            _shopManager.OpenShop();
        }
        else
        {
            // No shop manager, proceed directly
            HandleShopClosed();
        }
    }

    private void HandleShopClosed()
    {
        StartNextRound();
    }

    private void GameOver()
    {
        SetState(GameState.GameOver);
        GameEvents.RaiseGameOver();

        Debug.Log("========== GAME OVER ==========");
        Debug.Log($"Reached round {_currentRound}");
        Debug.Log($"Final score this round: {_scoreTracker.CurrentScore}/{_scoreTracker.ScoreGoal}");

        // Reset currency (money doesn't persist across games)
        _currencyManager?.Reset();

        // Clear all modifiers
        if (_modifierManager != null)
        {
            _modifierManager.ClearAllModifiers();
        }
    }

    private void SetState(GameState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Game State: {newState}");
    }

    /// <summary>
    /// Called by UI to continue from shop.
    /// </summary>
    public void ContinueFromShop()
    {
        if (_currentState == GameState.InShop && _shopManager != null)
        {
            _shopManager.Continue();
        }
    }

    /// <summary>
    /// Called by UI to restart after game over.
    /// </summary>
    public void RequestRestart()
    {
        if (_currentState == GameState.GameOver)
        {
            RestartGame();
        }
    }

    private void OnDestroy()
    {
        if (_roundManager != null)
        {
            _roundManager.OnRoundEnded -= HandleRoundEnded;
        }

        if (_shopManager != null)
        {
            _shopManager.OnShopClosed -= HandleShopClosed;
        }
    }
}
