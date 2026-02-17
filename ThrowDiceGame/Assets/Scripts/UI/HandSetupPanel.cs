using System.Collections;
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

    [Header("Draw Animation")]
    [SerializeField]
    [Tooltip("Delay between each die appearing during the draw sequence")]
    private float _drawStaggerDelay = 0.08f;

    [SerializeField]
    [Tooltip("Scale multiplier at the peak of each die's appear bump")]
    private float _drawBumpScale = 1.3f;

    [SerializeField]
    [Tooltip("Duration of each die's scale bump animation")]
    private float _drawBumpDuration = 0.2f;

    private RoundManager _roundManager;
    private List<DiceDisplayItem> _diceDisplayItems = new List<DiceDisplayItem>();
    private List<GameObject> _slotObjects = new List<GameObject>();
    private List<int> _selectedIndices = new List<int>();
    private Coroutine _drawSequence;
    private bool _drawAnimating;

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
        if (_drawSequence != null)
            StopCoroutine(_drawSequence);

        // Clear existing displays and slots
        foreach (var item in _diceDisplayItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _diceDisplayItems.Clear();

        foreach (var slot in _slotObjects)
        {
            if (slot != null)
                Destroy(slot);
        }
        _slotObjects.Clear();

        if (_roundManager?.Hand == null || _diceDisplayContainer == null) return;

        _drawSequence = StartCoroutine(DrawDiceSequence());
    }

    private IEnumerator DrawDiceSequence()
    {
        _drawAnimating = true;
        SetButtonsInteractable(false);

        var hand = _roundManager.Hand;
        if (_diceDisplayPrefab == null) yield break;

        // Pre-create empty slots so the layout group allocates space up front
        Vector2 slotSize = ((RectTransform)_diceDisplayPrefab.transform).sizeDelta;
        for (int i = 0; i < hand.Count; i++)
        {
            var slot = CreateLayoutSlot(slotSize);
            _slotObjects.Add(slot);
        }

        // Force layout to rebuild immediately so slots are positioned
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_diceDisplayContainer);

        // Stagger-instantiate actual dice displays inside each slot
        for (int i = 0; i < hand.Count; i++)
        {
            var diceData = hand.GetAt(i);
            if (diceData == null) continue;

            var item = Instantiate(_diceDisplayPrefab, _slotObjects[i].transform);
            StretchToParent((RectTransform)item.transform);

            int index = i;
            item.Initialize(diceData, index, () => OnDiceClicked(index));
            _diceDisplayItems.Add(item);

            StartCoroutine(AnimateScaleBump(item.transform));

            if (i < hand.Count - 1)
                yield return new WaitForSeconds(_drawStaggerDelay);
        }

        // Wait for the last bump to finish before enabling interaction
        yield return new WaitForSeconds(_drawBumpDuration);

        _drawAnimating = false;
        _drawSequence = null;
        UpdateUI();
    }

    private GameObject CreateLayoutSlot(Vector2 size)
    {
        var slot = new GameObject("DiceSlot", typeof(RectTransform), typeof(LayoutElement));
        slot.transform.SetParent(_diceDisplayContainer, false);
        var le = slot.GetComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
        return slot;
    }

    private static void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private IEnumerator AnimateScaleBump(Transform target)
    {
        Vector3 baseScale = target.localScale;
        target.localScale = Vector3.zero;

        float elapsed = 0f;
        float halfDuration = _drawBumpDuration * 0.4f;

        // Scale up to bump peak
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            // Ease-out quad for the upswing
            float scale = _drawBumpScale * (1f - (1f - t) * (1f - t));
            target.localScale = baseScale * scale;
            yield return null;
        }

        // Scale back down to normal
        elapsed = 0f;
        float settleTime = _drawBumpDuration - halfDuration;
        while (elapsed < settleTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / settleTime);
            // Ease-out quad for settle
            float scale = Mathf.Lerp(_drawBumpScale, 1f, t * t);
            target.localScale = baseScale * scale;
            yield return null;
        }

        target.localScale = baseScale;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (_throwButton != null) _throwButton.interactable = interactable;
        if (_throwAllButton != null) _throwAllButton.interactable = interactable;
        if (_discardButton != null) _discardButton.interactable = interactable;
    }

    private void OnDiceClicked(int index)
    {
        if (_drawAnimating) return;

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
        if (_drawAnimating) return;

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
