using TMPro;
using UnityEngine;

/// <summary>
/// Panel shown while dice are being thrown.
/// </summary>
public class ThrowingPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _statusText;

    public void Initialize()
    {
        // No dependencies needed
    }

    protected override void OnShow()
    {
        if (_statusText != null)
        {
            _statusText.text = "Rolling...";
        }
    }
}
