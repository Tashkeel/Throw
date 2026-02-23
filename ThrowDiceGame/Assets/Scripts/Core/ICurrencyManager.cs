/// <summary>
/// Minimal interface for currency operations.
/// Allows modifiers to award money without depending on CurrencyManager directly.
/// </summary>
public interface ICurrencyManager
{
    /// <summary>Adds money to the player's balance.</summary>
    void AddMoney(int amount);
}
