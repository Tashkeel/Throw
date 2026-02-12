using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI item representing a single die in the hand display.
/// Displays a 3D rotatable preview of the die model.
/// </summary>
public class DiceDisplayItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private RawImage _previewImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _selectionBorder;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSensitivity = 0.5f;
    [SerializeField] private float _autoRotateSpeed = 15f;
    [SerializeField] private bool _autoRotateWhenIdle = true;
    [SerializeField] private float _idleTimeBeforeAutoRotate = 2f;

    [Header("Face Value Display")]
    [SerializeField]
    [Tooltip("Six TextMeshProUGUI elements displaying each face value (order: +Y, -Y, +X, -X, +Z, -Z)")]
    private TextMeshProUGUI[] _faceValueTexts = new TextMeshProUGUI[6];

    [SerializeField]
    [Tooltip("Background images behind each face value text (same order as texts)")]
    private Image[] _faceValueBackgrounds = new Image[6];

    [Header("Face Highlight Colors")]
    [SerializeField] private Color _faceNormalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color _faceHighlightColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color _moneyFaceColor = new Color(1f, 0.84f, 0f, 1f);

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.8f);
    [SerializeField] private Color _borderNormalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _borderSelectedColor = new Color(1f, 0.3f, 0.3f, 1f);

    public InventoryDie _inventoryDie;
    private int _index;
    private Action _onClick;
    private bool _isSelected;

    private DicePreviewInstance _previewInstance;
    private RenderTexture _renderTexture;
    private bool _isDragging;
    private float _idleTimer;

    // Maps original face index (0-5) to sorted display position (0-5)
    private int[] _faceIndexToDisplayPosition;
    // Tracks which display positions are money sides (for highlight restore)
    private bool[] _isMoneyDisplayPosition;

    public void Initialize(InventoryDie die, int index, Action onClick)
    {
        _inventoryDie = die;
        _index = index;
        _onClick = onClick;
        _isSelected = false;
        _idleTimer = 0f;

        CreatePreview();
        PopulateFaceValues();
        UpdateDisplay();
    }

    private void CreatePreview()
    {
        var renderer = DicePreviewRenderer.Instance;
        if (renderer == null)
        {
            Debug.LogWarning("DiceDisplayItem: No DicePreviewRenderer found in scene.");
            return;
        }

        // Get die prefab from DiceData
        GameplayDie diePrefab = _inventoryDie?._dieType.DiePrefab;
        if (diePrefab == null)
        {
            Debug.LogWarning("DiceDisplayItem: DiceData has no die prefab assigned.");
            return;
        }

        // Create render texture
        _renderTexture = renderer.CreateRenderTexture();

        // Create preview instance
        _previewInstance = renderer.CreatePreviewDie(diePrefab, _inventoryDie);

        // Assign render texture to image
        if (_previewImage != null)
        {
            _previewImage.texture = _renderTexture;
        }

        // Initial render
        RenderPreview();
    }

    private void Update()
    {
        if (_previewInstance == null) return;

        // Auto-rotate when idle
        if (_autoRotateWhenIdle && !_isDragging)
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _idleTimeBeforeAutoRotate)
            {
                _previewInstance.Rotate(new Vector2(_autoRotateSpeed * Time.deltaTime, 0f), 1f);
                RenderPreview();
            }
        }
    }

    private void RenderPreview()
    {
        var renderer = DicePreviewRenderer.Instance;
        if (renderer != null && _previewInstance != null && _renderTexture != null)
        {
            renderer.RenderPreview(_previewInstance, _renderTexture);
            UpdateFaceHighlight();
        }
    }

    private void PopulateFaceValues()
    {
        if (_inventoryDie == null) return;

        int[] values = _inventoryDie.GetFaceValues();
        DieSideType[] types = _inventoryDie.GetFaceTypes();

        // Build sorted indices (ascending by value)
        int[] sortedIndices = new int[values.Length];
        for (int i = 0; i < sortedIndices.Length; i++)
            sortedIndices[i] = i;

        System.Array.Sort(sortedIndices, (a, b) => values[a].CompareTo(values[b]));

        // Build reverse mapping: original face index -> display position
        _faceIndexToDisplayPosition = new int[values.Length];
        for (int displayPos = 0; displayPos < sortedIndices.Length; displayPos++)
        {
            _faceIndexToDisplayPosition[sortedIndices[displayPos]] = displayPos;
        }

        // Track which display positions are money sides
        _isMoneyDisplayPosition = new bool[6];

        // Populate texts in sorted order, with $ prefix for money sides
        for (int displayPos = 0; displayPos < 6 && displayPos < _faceValueTexts.Length; displayPos++)
        {
            int originalIndex = sortedIndices[displayPos];
            bool isMoney = types != null && originalIndex < types.Length && types[originalIndex] == DieSideType.Money;
            _isMoneyDisplayPosition[displayPos] = isMoney;

            if (_faceValueTexts[displayPos] != null)
            {
                _faceValueTexts[displayPos].text = isMoney
                    ? $"${values[originalIndex]}"
                    : values[originalIndex].ToString();
            }
        }

        // Set backgrounds — money faces get gold color, score faces get normal
        for (int displayPos = 0; displayPos < _faceValueBackgrounds.Length; displayPos++)
        {
            if (_faceValueBackgrounds[displayPos] != null)
            {
                int originalIndex = displayPos < sortedIndices.Length ? sortedIndices[displayPos] : 0;
                bool isMoney = types != null && originalIndex < types.Length && types[originalIndex] == DieSideType.Money;
                _faceValueBackgrounds[displayPos].color = isMoney ? _moneyFaceColor : _faceNormalColor;
            }
        }
    }

    private void UpdateFaceHighlight()
    {
        if (_previewInstance == null || _faceIndexToDisplayPosition == null) return;

        int facingIndex = _previewInstance.GetCameraFacingFaceIndex();
        if (facingIndex < 0 || facingIndex >= _faceIndexToDisplayPosition.Length) return;

        int highlightPosition = _faceIndexToDisplayPosition[facingIndex];

        for (int i = 0; i < _faceValueBackgrounds.Length; i++)
        {
            if (_faceValueBackgrounds[i] != null)
            {
                if (i == highlightPosition)
                {
                    _faceValueBackgrounds[i].color = _faceHighlightColor;
                }
                else
                {
                    bool isMoney = _isMoneyDisplayPosition != null && i < _isMoneyDisplayPosition.Length && _isMoneyDisplayPosition[i];
                    _faceValueBackgrounds[i].color = isMoney ? _moneyFaceColor : _faceNormalColor;
                }
            }
        }
    }

    private void UpdateDisplay()
    {

        UpdateVisuals();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = _isSelected ? _selectedColor : _normalColor;
        }

        if (_selectionBorder != null)
        {
            _selectionBorder.color = _isSelected ? _borderSelectedColor : _borderNormalColor;
        }
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only trigger click if we weren't dragging
        if (!_isDragging || eventData.dragging == false)
        {
            _onClick?.Invoke();
        }
    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = false;
    }

    // IPointerUpHandler
    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _idleTimer = 0f;
    }

    // IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _idleTimer = 0f;

        if (_previewInstance != null)
        {
            _previewInstance.Rotate(eventData.delta, _rotationSensitivity);
            RenderPreview();
        }
    }

    private void OnDestroy()
    {
        // Cleanup preview instance
        if (_previewInstance != null)
        {
            _previewInstance.Cleanup();
            _previewInstance = null;
        }

        // Cleanup render texture
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }
    }
}
