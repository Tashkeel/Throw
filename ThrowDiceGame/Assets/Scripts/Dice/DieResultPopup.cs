using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a world-space UI popup above a gameplay die showing its score or money result.
/// The popup is hidden until the die settles and scores, then appears with the result.
/// Attach to the GameplayDie prefab alongside a world-space Canvas child.
/// </summary>
public class DieResultPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    [Tooltip("The world-space Canvas that holds the popup (set inactive by default)")]
    private Canvas _popupCanvas;

    [SerializeField]
    [Tooltip("Background image whose color indicates score vs money")]
    private Image _backgroundImage;

    [SerializeField]
    [Tooltip("Text displaying the result value")]
    private TextMeshProUGUI _valueText;

    [Header("Colors")]
    [SerializeField] private Color _scoreColor = new Color(0.2f, 0.6f, 1f, 0.85f);
    [SerializeField] private Color _moneyColor = new Color(1f, 0.84f, 0f, 0.85f);

    [Header("Scale Bump")]
    [SerializeField]
    [Tooltip("Scale multiplier at the peak of the bump")]
    private float _bumpScale = 1.4f;

    [SerializeField]
    [Tooltip("How fast the scale bump decays back to normal")]
    private float _bumpDecaySpeed = 6f;

    [Header("Billboard")]
    [SerializeField]
    [Tooltip("Offset above the die center in local space")]
    private Vector3 _offset = new Vector3(0f, 1.2f, 0f);

    private GameplayDie _die;
    private Transform _cameraTransform;
    private Vector3 _baseScale;
    private float _bumpT;

    private void Awake()
    {
        _die = GetComponent<GameplayDie>();

        if (_popupCanvas != null)
        {
            _baseScale = _popupCanvas.transform.localScale;
            _popupCanvas.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (_die != null)
            _die.OnDieAtRest += HandleDieAtRest;
    }

    private void OnDisable()
    {
        if (_die != null)
            _die.OnDieAtRest -= HandleDieAtRest;
    }

    private void HandleDieAtRest(GameplayDie die, int topFaceValue)
    {
        if (_popupCanvas == null || _valueText == null) return;

        DieSideType sideType = die.GetTopFaceSideType();
        bool isMoney = sideType == DieSideType.Money;

        _valueText.text = isMoney ? $"${topFaceValue}" : $"+{topFaceValue}";

        if (_backgroundImage != null)
            _backgroundImage.color = isMoney ? _moneyColor : _scoreColor;

        _popupCanvas.gameObject.SetActive(true);

        // Start scale bump: begin at peak, will decay in LateUpdate
        _bumpT = 1f;
        _popupCanvas.transform.localScale = _baseScale * _bumpScale;
    }

    private void LateUpdate()
    {
        if (_popupCanvas == null || !_popupCanvas.gameObject.activeSelf) return;

        // Position above the die
        _popupCanvas.transform.position = transform.position + _offset;

        // Billboard: face the camera
        if (_cameraTransform != null)
        {
            _popupCanvas.transform.rotation = Quaternion.LookRotation(
                _popupCanvas.transform.position - _cameraTransform.position);
        }

        // Animate scale bump decay
        if (_bumpT > 0f)
        {
            _bumpT = Mathf.MoveTowards(_bumpT, 0f, _bumpDecaySpeed * Time.deltaTime);
            // Ease-out: overshoot decays smoothly to 1x
            float scale = 1f + (_bumpScale - 1f) * _bumpT * _bumpT;
            _popupCanvas.transform.localScale = _baseScale * scale;
        }
    }

    /// <summary>
    /// Hides the popup. Called externally when dice are cleared between throws.
    /// </summary>
    public void Hide()
    {
        if (_popupCanvas != null)
            _popupCanvas.gameObject.SetActive(false);
    }
}
