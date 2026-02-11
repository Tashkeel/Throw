using UnityEngine;

/// <summary>
/// Base class for modifier implementations.
/// Inherit from this to create new modifier types.
/// </summary>
public abstract class BaseModifier : MonoBehaviour, IScoreModifier
{
    [Header("Base Modifier Settings")]
    [SerializeField]
    protected string _name = "Base Modifier";

    [SerializeField]
    [TextArea(2, 4)]
    protected string _description = "Base modifier description.";

    [SerializeField]
    protected ScoreModifierTiming _timing = ScoreModifierTiming.PerDie;

    public virtual string Name => _name;
    public virtual string Description => _description;
    public virtual ScoreModifierTiming Timing => _timing;

    /// <summary>
    /// Override this to implement the modifier's scoring logic.
    /// </summary>
    public abstract int ModifyScore(ScoreModifierContext context);
}
