using UnityEngine;

/// <summary>
/// Base class for all UI panels. Provides show/hide functionality.
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The root GameObject of this panel (defaults to this object)")]
    protected GameObject _panelRoot;

    protected virtual void Awake()
    {
        if (_panelRoot == null)
        {
            _panelRoot = gameObject;
        }
    }

    /// <summary>
    /// Shows this panel.
    /// </summary>
    public virtual void Show()
    {
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(true);
        }
        OnShow();
    }

    /// <summary>
    /// Hides this panel.
    /// </summary>
    public virtual void Hide()
    {
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
        OnHide();
    }

    /// <summary>
    /// Returns true if the panel is currently visible.
    /// </summary>
    public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

    /// <summary>
    /// Called when the panel is shown. Override for custom behavior.
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// Called when the panel is hidden. Override for custom behavior.
    /// </summary>
    protected virtual void OnHide() { }
}
