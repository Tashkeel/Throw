using System;
using System.Collections.Generic;

/// <summary>
/// Base class for enhancements that must analyze multiple dice before applying.
/// Enforces the contract that PrepareApplication is called before ApplyToDie
/// (Template Method pattern — fixes LSP violation in MirrorEnhancement/CascadeEnhancement).
/// </summary>
public abstract class MultiDiceEnhancementData : EnhancementData
{
    private bool _isPrepared;

    /// <summary>
    /// Called by ShopManager before the per-die apply loop.
    /// Passes all selected dice face values so the enhancement can analyse them.
    /// </summary>
    public void PrepareApplication(int[][] allSelectedFaceValues)
    {
        _isPrepared = false;
        OnPrepared(allSelectedFaceValues);
        _isPrepared = true;
    }

    /// <summary>
    /// Override to analyse all selected dice before per-die application.
    /// </summary>
    protected abstract void OnPrepared(int[][] allValues);

    /// <summary>
    /// Sealed — cannot be called without PrepareApplication first.
    /// Throws InvalidOperationException if PrepareApplication was skipped.
    /// </summary>
    public sealed override int[] ApplyToDie(int[] currentValues)
    {
        if (!_isPrepared)
            throw new InvalidOperationException(
                $"Call PrepareApplication before ApplyToDie on {GetType().Name}");

        return ApplyWithContext(currentValues);
    }

    /// <summary>
    /// Override this instead of ApplyToDie.
    /// Called with context established by PrepareApplication.
    /// </summary>
    protected abstract int[] ApplyWithContext(int[] values);

    /// <summary>
    /// Resets the prepared state. Call after the apply loop so this enhancement
    /// can be used again without stale context.
    /// </summary>
    public void ResetPrepared()
    {
        _isPrepared = false;
    }

    /// <summary>
    /// Backward-compat bridge: old ShopManager path calling PreProcess
    /// will correctly trigger PrepareApplication on multi-dice enhancements.
    /// </summary>
    public override void PreProcess(List<int[]> allSelectedDiceFaceValues)
    {
        PrepareApplication(allSelectedDiceFaceValues?.ToArray() ?? Array.Empty<int[]>());
    }
}
