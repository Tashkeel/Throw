using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel for hand setup phase where players can discard and redraw dice.
/// </summary>
public class HandSetupPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _discardsRemainingText;
    [SerializeField] private TextMeshProUGUI _throwsRemainingText;
    [SerializeField] private Transform _diceDisplayContainer;
    [SerializeField] private Button _throwButton;
    [SerializeField] private Button _throwAllButton;
    [SerializeField] private Button _discardButton;

    [Header("Dice Display Prefab")]
    [SerializeField] private DiceDisplayItem _diceDisplayPrefab;

    private RoundManager _roundManager;
    private List<DiceDisplayItem> _diceDisplayItems = new List<DiceDisplayItem>();
    private List<int> _selectedIndices = new List<int>();

    public void Initialize(RoundManager roundManager)
    {
        _roundManager = roundManager;

        if (_throwButton != null)
        {
            _throwButton.onClick.AddListener(OnThrowClicked);
        }

        if (_throwAllButton != null)
        {
            _throwAllButton.onClick.AddListener(OnThrowAllClicked);
        }

        if (_discardButton != null)
        {
            _discardButton.onClick.AddListener(OnDiscardClicked);
        }
    }

    protected override void OnShow()
    {
        Refresh();
    }

    public void Refresh()
    {
        _selectedIndices.Clear();
        UpdateDiceDisplay();
        UpdateUI();
    }

    private void UpdateDiceDisplay()
    {
        // Clear existing displays
        foreach (var item in _diceDisplayItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        _diceDisplayItems.Clear();

        if (_roundManager?.Hand == null || _diceDisplayContainer == null) return;

        // Create display for each die in hand
        var hand = _roundManager.Hand;
        for (int i = 0; i < hand.Count; i++)
        {
            var diceData = hand.GetAt(i);
            if (diceData == null) continue;

            if (_diceDisplayPrefab != null)
            {
                var item = Instantiate(_diceDisplayPrefab, _diceDisplayContainer);
                int index = i; // Capture for closure
                item.Initialize(diceData, index, () => OnDiceClicked(index));
                _diceDisplayItems.Add(item);
            }
        }
    }

    private void OnDiceClicked(int index)
    {
        var phase = _roundManager?.HandSetupPhase;
        if (phase == null || !phase.WaitingForInput) return;

        // Toggle selection
        if (_selectedIndices.Contains(index))
        {
            _selectedIndices.Remove(index);
            UpdateItemSelection(index, false);
        }
        else
        {
            int discardsRemaining = _roundManager?.DiscardsRemainingThisRound ?? 0;

            // When discards are available, limit selection to max discard count
            // When no discards remain, allow selecting any number (for throwing)
            if (discardsRemaining > 0 && _selectedIndices.Count >= phase.MaxDiscardCount)
            {
                // At max selection for discard
            }
            else
            {
                _selectedIndices.Add(index);
                UpdateItemSelection(index, true);
            }
        }

        UpdateUI();
    }

    private void UpdateItemSelection(int index, bool selected)
    {
        if (index >= 0 && index < _diceDisplayItems.Count)
        {
            _diceDisplayItems[index]?.SetSelected(selected);
        }
    }

    private void UpdateUI()
    {
        var phase = _roundManager?.HandSetupPhase;
        int discardsRemaining = _roundManager?.DiscardsRemainingThisRound ?? 0;

        if (_discardsRemainingText != null)
        {
            _discardsRemainingText.text = $"{discardsRemaining}";
        }

        if (_throwsRemainingText != null)
        {
            int throwsRemaining = _roundManager.MaxThrows - _roundManager.CurrentThrow + 1;
            _throwsRemainingText.text = $"{throwsRemaining}";
        }

        if (_discardButton != null)
        {
            _discardButton.interactable = _selectedIndices.Count > 0 && discardsRemaining > 0;
        }

        if(_throwButton != null)
        {
            // Throw button requires at least one die selected
            _throwButton.interactable = _selectedIndices.Count > 0;
        }

        if (_throwAllButton != null)
        {
            _throwAllButton.interactable = _roundManager?.Hand != null && _roundManager.Hand.Count > 0;
        }
    }

    private void OnDiscardClicked()
    {
        if (_selectedIndices.Count == 0) return;

        _roundManager?.PerformDiscard(new List<int>(_selectedIndices));
        _selectedIndices.Clear();
        Refresh();
    }

    private void OnThrowClicked()
    {
        if (_selectedIndices.Count == 0) return;

        _roundManager?.ConfirmHandSetup(new List<int>(_selectedIndices));
    }

    private void OnThrowAllClicked()
    {
        if (_roundManager?.Hand == null || _roundManager.Hand.Count == 0) return;

        var allIndices = new List<int>();
        for (int i = 0; i < _roundManager.Hand.Count; i++)
            allIndices.Add(i);

        _roundManager.ConfirmHandSetup(allIndices);
    }

    private void OnDestroy()
    {
        if (_throwButton != null)
        {
            _throwButton.onClick.RemoveListener(OnThrowClicked);
        }

        if (_throwAllButton != null)
        {
            _throwAllButton.onClick.RemoveListener(OnThrowAllClicked);
        }

        if (_discardButton != null)
        {
            _discardButton.onClick.RemoveListener(OnDiscardClicked);
        }
    }
}
