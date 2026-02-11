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
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSensitivity = 0.5f;
    [SerializeField] private float _autoRotateSpeed = 15f;
    [SerializeField] private bool _autoRotateWhenIdle = true;
    [SerializeField] private float _idleTimeBeforeAutoRotate = 2f;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.8f);
    [SerializeField] private Color _borderNormalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _borderSelectedColor = new Color(1f, 0.3f, 0.3f, 1f);

    private DiceData _diceData;
    private int _index;
    private Action _onClick;
    private bool _isSelected;

    private DicePreviewInstance _previewInstance;
    private RenderTexture _renderTexture;
    private bool _isDragging;
    private float _idleTimer;

    public void Initialize(DiceData diceData, int index, Action onClick)
    {
        _diceData = diceData;
        _index = index;
        _onClick = onClick;
        _isSelected = false;
        _idleTimer = 0f;

        CreatePreview();
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
        Die diePrefab = _diceData?.DiePrefab;
        if (diePrefab == null)
        {
            Debug.LogWarning("DiceDisplayItem: DiceData has no die prefab assigned.");
            return;
        }

        // Create render texture
        _renderTexture = renderer.CreateRenderTexture();

        // Create preview instance
        _previewInstance = renderer.CreatePreviewDie(diePrefab, _diceData);

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
        }
    }

    private void UpdateDisplay()
    {
        if (_nameText != null)
        {
            _nameText.text = _diceData?.DisplayName ?? "Die";
        }

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
