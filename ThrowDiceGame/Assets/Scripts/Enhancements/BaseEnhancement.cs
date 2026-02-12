using UnityEngine;

/// <summary>
/// Base class for enhancement implementations.
/// Inherit from this to create new enhancement types.
/// </summary>
public abstract class BaseEnhancement : MonoBehaviour, IEnhancement
{
    [Header("Base Enhancement Settings")]
    [SerializeField]
    protected string _name = "Base Enhancement";

    [SerializeField]
    [TextArea(2, 4)]
    protected string _description = "Base enhancement description.";

    [SerializeField]
    [Tooltip("Number of dice this enhancement applies to")]
    protected int _maxDiceCount = 1;

    public virtual string Name => _name;
    public virtual string Description => _description;
    public virtual int MaxDiceCount => _maxDiceCount;

    /// <summary>
    /// Override this to implement the enhancement's modification logic.
    /// </summary>
    public abstract int[] ApplyToDie(int[] currentValues);
}
