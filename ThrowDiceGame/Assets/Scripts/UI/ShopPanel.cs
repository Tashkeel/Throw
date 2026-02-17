using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a shop item that can be either a modifier or enhancement.
/// </summary>
public class ShopItem
{
    public ModifierData Modifier;
    public EnhancementData Enhancement;
    public bool IsModifier => Modifier != null;
    public bool IsEnhancement => Enhancement != null;

    public string DisplayName => IsModifier ? Modifier.DisplayName : Enhancement?.DisplayName ?? "";
    public string Description => IsModifier ? Modifier.Description : Enhancement?.Description ?? "";
    public int Cost => IsModifier ? Modifier.Cost : Enhancement?.Cost ?? 0;
    public Sprite Icon => IsModifier ? Modifier.Icon : Enhancement?.Icon;
    public int RequiredDiceCount => IsEnhancement ? Enhancement.RequiredDiceCount : 0;
}

/// <summary>
/// Panel for the shop between rounds.
/// Displays randomly selected modifiers and enhancements for purchase.
/// </summary>
public class ShopPanel : UIPanel
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private Button _continueButton;

    [Header("Shop Items")]
    [SerializeField] private Transform _shopItemsContainer;
    [SerializeField] private ShopItemDisplay _shopItemPrefab;
    [SerializeField] private int _numberOfShopSlots = 2;

    [Header("Dice Selection (for Enhancements)")]
    [SerializeField] private Transform _diceSelectionContainer;
    [SerializeField] private DiceDisplayItem _diceDisplayPrefab;
    [SerializeField] private TextMeshProUGUI _selectionInstructionText;
    [SerializeField] private Button _applyEnhancementButton;
    [SerializeField] private Button _cancelSelectionButton;

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

    [Header("References")]
    [SerializeField] private RoundManager _roundManager;

    private ShopManager _shopManager;
    private List<InventoryDie> _displayedDice = new List<InventoryDie>();
    private List<ShopItem> _currentShopItems = new List<ShopItem>();
    private List<GameObject> _shopItemDisplays = new List<GameObject>();
    private List<DiceDisplayItem> _diceDisplayItems = new List<DiceDisplayItem>();
    private List<GameObject> _slotObjects = new List<GameObject>();
    private List<DiceDisplayItem> _selectedDice = new List<DiceDisplayItem>();
    private EnhancementData _pendingEnhancement;
    private bool _isSelectingDice;
    private Coroutine _drawSequence;
    private bool _drawAnimating;

    public void Initialize(ShopManager shopManager)
    {
        _shopManager = shopManager;

        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (_applyEnhancementButton != null)
        {
            _applyEnhancementButton.onClick.AddListener(OnApplyEnhancementClicked);
            _applyEnhancementButton.gameObject.SetActive(false);
        }

        if (_cancelSelectionButton != null)
        {
            _cancelSelectionButton.onClick.AddListener(OnCancelSelectionClicked);
            _cancelSelectionButton.gameObject.SetActive(false);
        }

        if (_shopManager != null)
        {
            _shopManager.OnPurchaseMade += OnPurchaseMade;
            if (_shopManager.CurrencyManager != null)
            {
                _shopManager.CurrencyManager.OnMoneyChanged += UpdateMoneyDisplay;
            }
        }
    }

    protected override void OnShow()
    {
        if (_titleText != null)
        {
            _titleText.text = "SHOP";
        }

        _isSelectingDice = false;
        _pendingEnhancement = null;
        _selectedDice.Clear();

        GenerateRandomShopItems();
        PopulateDiceDisplay(); // Always populate dice when shop opens
        RefreshDisplay();
    }

    private void GenerateRandomShopItems()
    {
        _currentShopItems.Clear();

        // Collect all available items (only unowned modifiers)
        var allItems = new List<ShopItem>();

        if (_shopManager != null)
        {
            foreach (var modifier in _shopManager.GetUnownedModifiers())
            {
                allItems.Add(new ShopItem { Modifier = modifier });
            }

            foreach (var enhancement in _shopManager.AvailableEnhancements)
            {
                allItems.Add(new ShopItem { Enhancement = enhancement });
            }
        }

        // Shuffle and pick items for slots
        ShuffleList(allItems);

        int itemCount = Mathf.Min(_numberOfShopSlots, allItems.Count);
        for (int i = 0; i < itemCount; i++)
        {
            _currentShopItems.Add(allItems[i]);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void RefreshDisplay()
    {
        UpdateMoneyDisplay(_shopManager?.CurrencyManager?.CurrentMoney ?? 0);
        PopulateShopItems();
        UpdateDiceSelectionUI();
    }

    private void OnPurchaseMade()
    {
        // Refresh affordability of remaining items
        foreach (var display in _shopItemDisplays)
        {
            var item = display.GetComponent<ShopItemDisplay>();
            item?.UpdateAffordability();
        }
    }

    private void UpdateMoneyDisplay(int money)
    {
        if (_moneyText != null)
        {
            _moneyText.text = $"${money}";
        }

        // Update affordability on all items
        foreach (var display in _shopItemDisplays)
        {
            var item = display.GetComponent<ShopItemDisplay>();
            item?.UpdateAffordability();
        }
    }

    private void PopulateShopItems()
    {
        // Clear existing displays
        foreach (var display in _shopItemDisplays)
        {
            if (display != null) Destroy(display);
        }
        _shopItemDisplays.Clear();

        if (_shopItemsContainer == null) return;

        foreach (var shopItem in _currentShopItems)
        {
            CreateShopItemDisplay(shopItem);
        }
    }

    private void CreateShopItemDisplay(ShopItem shopItem)
    {
        if (_shopItemPrefab == null)
        {
            Debug.LogWarning("ShopPanel: No shop item prefab assigned.");
            return;
        }

        var display = Instantiate(_shopItemPrefab, _shopItemsContainer);

        if (shopItem.IsModifier)
        {
            display.Initialize(
                shopItem.Modifier,
                () => PurchaseModifier(shopItem.Modifier, display),
                () => _shopManager.CanPurchaseModifier(shopItem.Modifier)
            );
        }
        else if (shopItem.IsEnhancement)
        {
            display.Initialize(
                shopItem.Enhancement,
                () => StartDiceSelection(shopItem.Enhancement),
                () => _shopManager.CanAffordEnhancement(shopItem.Enhancement)
            );
        }

        _shopItemDisplays.Add(display.gameObject);
    }

    private void PurchaseModifier(ModifierData modifier, ShopItemDisplay display)
    {
        if (_shopManager != null && _shopManager.PurchaseModifier(modifier))
        {
            Debug.Log($"Purchased modifier: {modifier.DisplayName}");

            // Mark the display as sold out
            if (display != null)
            {
                display.MarkSoldOut();
            }
        }
    }

    private void StartDiceSelection(EnhancementData enhancement)
    {
        if (!_shopManager.CanAffordEnhancement(enhancement)) return;

        _pendingEnhancement = enhancement;
        _isSelectingDice = true;
        _selectedDice.Clear();

        // Clear any existing selections visually
        foreach (var item in _diceDisplayItems)
        {
            item?.SetSelected(false);
        }

        UpdateDiceSelectionUI();
    }

    /// <summary>
    /// Populates the dice display with random dice from inventory up to hand limit.
    /// Called when shop opens to show a hand-sized sample of the player's dice.
    /// </summary>
    private void PopulateDiceDisplay()
    {
        if (_drawSequence != null)
            StopCoroutine(_drawSequence);

        // Clear existing dice displays and slots
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
        _displayedDice.Clear();

        if (_diceSelectionContainer == null)
        {
            Debug.LogWarning("ShopPanel: _diceSelectionContainer is null");
            return;
        }

        if (_diceDisplayPrefab == null)
        {
            Debug.LogWarning("ShopPanel: _diceDisplayPrefab is null");
            return;
        }

        if (_shopManager?.Inventory == null)
        {
            Debug.LogWarning("ShopPanel: _shopManager or Inventory is null");
            return;
        }

        // Make sure container is active
        _diceSelectionContainer.gameObject.SetActive(true);

        // Get all dice from inventory as a flat list
        var allDice = _shopManager.Inventory.GetAllDiceAsList();

        // Determine hand size limit
        int handSize = _roundManager != null ? _roundManager.HandSize : 5;
        int diceToShow = Mathf.Min(handSize, allDice.Count);

        // Shuffle and take up to hand size
        ShuffleList(allDice);
        for (int i = 0; i < diceToShow; i++)
        {
            _displayedDice.Add(allDice[i]);
        }

        Debug.Log($"ShopPanel: Displaying {_displayedDice.Count} of {allDice.Count} dice (hand limit: {handSize})");

        _drawSequence = StartCoroutine(DrawDiceSequence());
    }

    private IEnumerator DrawDiceSequence()
    {
        _drawAnimating = true;

        // Pre-create empty slots so the layout group allocates space up front
        Vector2 slotSize = ((RectTransform)_diceDisplayPrefab.transform).sizeDelta;
        for (int i = 0; i < _displayedDice.Count; i++)
        {
            var slot = CreateLayoutSlot(slotSize);
            _slotObjects.Add(slot);
        }

        // Force layout to rebuild immediately so slots are positioned
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_diceSelectionContainer);

        // Stagger-instantiate actual dice displays inside each slot
        for (int i = 0; i < _displayedDice.Count; i++)
        {
            var diceData = _displayedDice[i];
            var item = Instantiate(_diceDisplayPrefab, _slotObjects[i].transform);
            StretchToParent((RectTransform)item.transform);

            int index = i;
            item.Initialize(diceData, index, () => OnDiceClicked(item, index));
            _diceDisplayItems.Add(item);

            StartCoroutine(AnimateScaleBump(item.transform));

            if (i < _displayedDice.Count - 1)
                yield return new WaitForSeconds(_drawStaggerDelay);
        }

        yield return new WaitForSeconds(_drawBumpDuration);

        _drawAnimating = false;
        _drawSequence = null;
    }

    private GameObject CreateLayoutSlot(Vector2 size)
    {
        var slot = new GameObject("DiceSlot", typeof(RectTransform), typeof(LayoutElement));
        slot.transform.SetParent(_diceSelectionContainer, false);
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
            float scale = Mathf.Lerp(_drawBumpScale, 1f, t * t);
            target.localScale = baseScale * scale;
            yield return null;
        }

        target.localScale = baseScale;
    }

    private void OnDiceClicked(DiceDisplayItem die, int index)
    {
        if (_drawAnimating) return;

        // Only allow selection when actively selecting for an enhancement
        if (!_isSelectingDice || _pendingEnhancement == null)
        {
            Debug.Log("ShopPanel: Dice clicked but not in selection mode. Click an enhancement first.");
            return;
        }

        int requiredCount = _pendingEnhancement.RequiredDiceCount;

        // Find if this specific dice instance is already selected
        // Use the displayed dice list for proper tracking
        bool isSelected = _selectedDice.Contains(die);

        if (isSelected)
        {
            // Deselect
            Debug.Log("Deselected a Die");
            _selectedDice.Remove(die);
            UpdateItemSelection(index, false);
        }
        else if (_selectedDice.Count < requiredCount)
        {
            // Select
            Debug.Log("Selected a Die");
            _selectedDice.Add(die);
            UpdateItemSelection(index, true);
        }

        UpdateDiceSelectionUI();
    }

    private void UpdateItemSelection(int index, bool selected)
    {
        if (index >= 0 && index < _diceDisplayItems.Count)
        {
            _diceDisplayItems[index]?.SetSelected(selected);
        }
    }

    private void UpdateDiceSelectionUI()
    {
        bool isSelectingForEnhancement = _isSelectingDice && _pendingEnhancement != null;

        // Dice container is ALWAYS visible - don't hide it
        // Only the instruction/buttons change based on selection mode

        if (_selectionInstructionText != null)
        {
            if (isSelectingForEnhancement)
            {
                int required = _pendingEnhancement.RequiredDiceCount;
                _selectionInstructionText.text = $"Select {required} dice for {_pendingEnhancement.DisplayName}\n({_selectedDice.Count}/{required} selected)";
                _selectionInstructionText.gameObject.SetActive(true);
            }
            else
            {
                _selectionInstructionText.gameObject.SetActive(false);
            }
        }

        if (_applyEnhancementButton != null)
        {
            bool canApply = isSelectingForEnhancement && _selectedDice.Count == _pendingEnhancement?.RequiredDiceCount;
            _applyEnhancementButton.gameObject.SetActive(isSelectingForEnhancement);
            _applyEnhancementButton.interactable = canApply;
        }

        if (_cancelSelectionButton != null)
        {
            _cancelSelectionButton.gameObject.SetActive(isSelectingForEnhancement);
        }

        // Hide continue button during enhancement selection
        if (_continueButton != null)
        {
            _continueButton.gameObject.SetActive(!isSelectingForEnhancement);
        }

        // Keep shop items visible always (user can still see what they're buying)
    }

    private void OnApplyEnhancementClicked()
    {
        if (_pendingEnhancement == null || _selectedDice.Count != _pendingEnhancement.RequiredDiceCount)
            return;

        if (_shopManager.ApplyEnhancement(_pendingEnhancement, new List<DiceDisplayItem>(_selectedDice)))
        {
            Debug.Log($"Applied enhancement: {_pendingEnhancement.DisplayName}");

            // Reset selection state
            _isSelectingDice = false;
            _pendingEnhancement = null;
            _selectedDice.Clear();

            // Re-populate dice display since inventory changed
            PopulateDiceDisplay();
            RefreshDisplay();
        }
    }

    private void OnCancelSelectionClicked()
    {
        CancelDiceSelection();
    }

    private void CancelDiceSelection()
    {
        _isSelectingDice = false;
        _pendingEnhancement = null;
        _selectedDice.Clear();

        // Clear visual selection on dice (don't destroy them - they stay visible)
        foreach (var item in _diceDisplayItems)
        {
            item?.SetSelected(false);
        }

        UpdateDiceSelectionUI();
    }

    private void OnContinueClicked()
    {
        if (_shopManager != null)
        {
            _shopManager.Continue();
        }
    }

    protected override void OnHide()
    {
        if (_drawSequence != null)
        {
            StopCoroutine(_drawSequence);
            _drawSequence = null;
            _drawAnimating = false;
        }

        // Clean up dice displays and slots when hiding
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
        _displayedDice.Clear();
        _selectedDice.Clear();

        // Clean up shop item displays
        foreach (var display in _shopItemDisplays)
        {
            if (display != null) Destroy(display);
        }
        _shopItemDisplays.Clear();
    }

    private void OnDestroy()
    {
        if (_continueButton != null)
        {
            _continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        if (_applyEnhancementButton != null)
        {
            _applyEnhancementButton.onClick.RemoveListener(OnApplyEnhancementClicked);
        }

        if (_cancelSelectionButton != null)
        {
            _cancelSelectionButton.onClick.RemoveListener(OnCancelSelectionClicked);
        }

        if (_shopManager != null)
        {
            _shopManager.OnPurchaseMade -= OnPurchaseMade;
            if (_shopManager.CurrencyManager != null)
            {
                _shopManager.CurrencyManager.OnMoneyChanged -= UpdateMoneyDisplay;
            }
        }
    }
}
