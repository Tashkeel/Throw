using System;

/// <summary>
/// Central event hub for game-wide events.
/// Using events allows loose coupling between systems.
/// </summary>
public static class GameEvents
{
    // Game State Events
    public static event Action OnGameStarted;
    public static event Action OnGameOver;
    public static event Action<int> OnRoundStarted; // round number
    public static event Action<int> OnRoundCompleted; // round number
    public static event Action<int> OnRoundFailed; // round number

    // Round Phase Events
    public static event Action OnHandSetupStarted;
    public static event Action OnHandSetupCompleted;
    public static event Action<int> OnThrowStarted; // throw number (1-3)
    public static event Action<int, int> OnThrowCompleted; // throw number, score
    public static event Action<int> OnScoringStarted; // accumulated score
    public static event Action<int, bool> OnScoringCompleted; // total score, goal reached

    // Shop Events
    public static event Action OnShopEntered;
    public static event Action OnShopExited;

    // Score Events
    public static event Action<int> OnScoreChanged; // new total score
    public static event Action<int, int> OnScoreGoalSet; // goal, round number

    // Dice Events
    public static event Action<int> OnDiceRolled; // roll total
    public static event Action OnAllDiceAtRest;
    public static event Action<int, int, int> OnDieScored; // dieIndex, rawValue, modifiedValue
    public static event Action<int, int> OnMoneyDieScored; // dieIndex, value

    // Currency Events
    public static event Action<int> OnMoneyChanged; // new money amount
    public static event Action<int> OnMoneyEarned; // amount earned

    // Modifier Events
    public static event Action OnModifiersChanged;

    // Invoke methods (internal use by managers)
    public static void RaiseGameStarted() => OnGameStarted?.Invoke();
    public static void RaiseGameOver() => OnGameOver?.Invoke();
    public static void RaiseRoundStarted(int round) => OnRoundStarted?.Invoke(round);
    public static void RaiseRoundCompleted(int round) => OnRoundCompleted?.Invoke(round);
    public static void RaiseRoundFailed(int round) => OnRoundFailed?.Invoke(round);

    public static void RaiseHandSetupStarted() => OnHandSetupStarted?.Invoke();
    public static void RaiseHandSetupCompleted() => OnHandSetupCompleted?.Invoke();
    public static void RaiseThrowStarted(int throwNumber) => OnThrowStarted?.Invoke(throwNumber);
    public static void RaiseThrowCompleted(int throwNumber, int score) => OnThrowCompleted?.Invoke(throwNumber, score);
    public static void RaiseScoringStarted(int accumulatedScore) => OnScoringStarted?.Invoke(accumulatedScore);
    public static void RaiseScoringCompleted(int totalScore, bool goalReached) => OnScoringCompleted?.Invoke(totalScore, goalReached);

    public static void RaiseShopEntered() => OnShopEntered?.Invoke();
    public static void RaiseShopExited() => OnShopExited?.Invoke();

    public static void RaiseScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
    public static void RaiseScoreGoalSet(int goal, int round) => OnScoreGoalSet?.Invoke(goal, round);

    public static void RaiseDiceRolled(int total) => OnDiceRolled?.Invoke(total);
    public static void RaiseAllDiceAtRest() => OnAllDiceAtRest?.Invoke();
    public static void RaiseDieScored(int dieIndex, int rawValue, int modifiedValue) => OnDieScored?.Invoke(dieIndex, rawValue, modifiedValue);
    public static void RaiseMoneyDieScored(int dieIndex, int value) => OnMoneyDieScored?.Invoke(dieIndex, value);

    public static void RaiseMoneyChanged(int newAmount) => OnMoneyChanged?.Invoke(newAmount);
    public static void RaiseMoneyEarned(int amount) => OnMoneyEarned?.Invoke(amount);
    public static void RaiseModifiersChanged() => OnModifiersChanged?.Invoke();
}
