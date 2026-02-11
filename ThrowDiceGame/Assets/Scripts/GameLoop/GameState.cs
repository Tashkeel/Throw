/// <summary>
/// Represents the high-level state of the game.
/// </summary>
public enum GameState
{
    /// <summary>
    /// Initial state before game starts.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Player is in an active round.
    /// </summary>
    InRound,

    /// <summary>
    /// Player is in the shop between rounds.
    /// </summary>
    InShop,

    /// <summary>
    /// Game has ended (player lost).
    /// </summary>
    GameOver
}

/// <summary>
/// Represents the phases within a round.
/// </summary>
public enum RoundPhase
{
    /// <summary>
    /// Not currently in a round phase.
    /// </summary>
    None,

    /// <summary>
    /// Player is setting up their hand (discard/redraw).
    /// </summary>
    HandSetup,

    /// <summary>
    /// Dice are being thrown.
    /// </summary>
    Throwing,

    /// <summary>
    /// Scoring the throw and checking win condition.
    /// </summary>
    Scoring
}
