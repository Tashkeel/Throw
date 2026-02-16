using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// State-driven camera controller that responds to game events.
/// Supports orbit, throw-tracking, and die-focus modes.
/// Attach to Main Camera.
/// </summary>
public class CameraController : MonoBehaviour
{
    private enum CameraMode
    {
        Orbit,
        ThrowTracking,
        DieFocus
    }

    [Header("References")]
    [SerializeField]
    [Tooltip("DiceManager to read tracked dice positions from")]
    private DiceManager _diceManager;

    [Header("Orbit Settings")]
    [SerializeField] private Vector3 _orbitTarget = Vector3.zero;
    [SerializeField] private float _orbitDistance = 12f;
    [SerializeField] private float _orbitHeight = 8f;
    [SerializeField] private float _orbitSensitivity = 2f;

    [Header("Throw Tracking")]
    [SerializeField] private float _trackingSmoothTime = 0.3f;
    [SerializeField] private float _trackingPadding = 1.5f;
    [SerializeField] private float _trackingMinDistance = 5f;
    [SerializeField] private float _trackingMaxDistance = 20f;
    [SerializeField] private float _trackingHeightOffset = 4f;
    [SerializeField]
    [Range(15f, 75f)]
    private float _trackingViewAngle = 45f;

    [Header("Die Focus")]
    [SerializeField] private float _dieFocusRotationSpeed = 5f;
    [SerializeField] private float _dieFocusDuration = 0.6f;
    [SerializeField] private float _dieFocusMinDuration = 0.15f;
    [SerializeField] private float _dieFocusMaxDisplacement = 2f;

    [Header("Transitions")]
    [SerializeField] private float _modeTransitionSmoothTime = 0.5f;

    private Camera _cam;
    private CameraMode _currentMode = CameraMode.Orbit;

    // Orbit state
    private float _orbitAngle;
    private bool _isLeftDragging;

    // Tracking state
    private Vector3 _trackingVelocity;
    private float _trackingHorizontalAngle;

    // Die focus state
    private readonly Queue<int> _focusQueue = new Queue<int>();
    private int _currentFocusDieIndex = -1;
    private float _focusTimer;
    private bool _allDiceSettled;
    private Vector3 _focusAnchorPosition;

    // Shared smoothing
    private Vector3 _positionVelocity;
    private Vector3 _currentPosition;
    private Quaternion _currentRotation;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _currentPosition = transform.position;
        _currentRotation = transform.rotation;

        Vector3 offset = _currentPosition - _orbitTarget;
        _orbitAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
    }

    private void OnEnable()
    {
        GameEvents.OnHandSetupStarted += HandleOrbitMode;
        GameEvents.OnThrowStarted += HandleThrowStarted;
        GameEvents.OnDieScored += HandleDieScored;
        GameEvents.OnAllDiceAtRest += HandleAllDiceAtRest;
        GameEvents.OnShopEntered += HandleOrbitMode;
        GameEvents.OnGameOver += HandleOrbitMode;
        GameEvents.OnGameStarted += HandleOrbitMode;
    }

    private void OnDisable()
    {
        GameEvents.OnHandSetupStarted -= HandleOrbitMode;
        GameEvents.OnThrowStarted -= HandleThrowStarted;
        GameEvents.OnDieScored -= HandleDieScored;
        GameEvents.OnAllDiceAtRest -= HandleAllDiceAtRest;
        GameEvents.OnShopEntered -= HandleOrbitMode;
        GameEvents.OnGameOver -= HandleOrbitMode;
        GameEvents.OnGameStarted -= HandleOrbitMode;
    }

    private void LateUpdate()
    {
        switch (_currentMode)
        {
            case CameraMode.Orbit:
                UpdateOrbit();
                break;
            case CameraMode.ThrowTracking:
                UpdateThrowTracking();
                break;
            case CameraMode.DieFocus:
                UpdateDieFocus();
                break;
        }
    }

    #region Mode Transitions

    private void SetMode(CameraMode newMode)
    {
        if (_currentMode == newMode) return;

        // When returning to orbit, recalculate angle from current camera position
        // so the transition is seamless rather than snapping to the old angle
        if (newMode == CameraMode.Orbit)
        {
            Vector3 offset = _currentPosition - _orbitTarget;
            _orbitAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        }

        // When returning to tracking from die focus, preserve horizontal angle
        // from current position for a seamless transition
        if (newMode == CameraMode.ThrowTracking && _currentMode == CameraMode.DieFocus)
        {
            Vector3 center;
            float maxRadius;
            if (ComputeDiceBounds(out center, out maxRadius))
            {
                Vector3 offset = _currentPosition - center;
                _trackingHorizontalAngle = Mathf.Atan2(offset.x, offset.z);
            }
        }

        _currentMode = newMode;
        _positionVelocity = Vector3.zero;
        _trackingVelocity = Vector3.zero;
        _isLeftDragging = false;
    }

    private void HandleOrbitMode()
    {
        SetMode(CameraMode.Orbit);
    }

    private void HandleThrowStarted(int throwNumber)
    {
        _focusQueue.Clear();
        _currentFocusDieIndex = -1;
        _allDiceSettled = false;

        // Preserve horizontal viewing angle so tracking continues from same direction
        Vector3 offset = _currentPosition - _orbitTarget;
        _trackingHorizontalAngle = Mathf.Atan2(offset.x, offset.z);

        SetMode(CameraMode.ThrowTracking);
    }

    private void HandleDieScored(int dieIndex, int rawValue, int modifiedValue)
    {
        _focusQueue.Enqueue(dieIndex);

        if (_currentMode == CameraMode.DieFocus)
        {
            // Already focusing on a die; if it's visible in the frustum,
            // interrupt and move to the next queued die immediately
            if (_currentFocusDieIndex >= 0)
            {
                GameplayDie currentDie = GetTrackedDie(_currentFocusDieIndex);
                if (currentDie != null && IsDieInFrustum(currentDie))
                {
                    DequeueNextDie();
                }
            }
        }
        else
        {
            // Anchor where tracking left us so die focus mostly just rotates
            _focusAnchorPosition = _currentPosition;
            SetMode(CameraMode.DieFocus);
            DequeueNextDie();
        }
    }

    private void HandleAllDiceAtRest()
    {
        _allDiceSettled = true;
    }

    #endregion

    #region Orbit Mode

    private void UpdateOrbit()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Use drag flags so UI panels with background images don't block orbit.
        // We skip the IsPointerOverGameObject check entirely for left-click orbit
        // because full-screen semi-transparent panel backgrounds would always block it.
        // Button clicks produce near-zero mouse delta so orbit barely moves during clicks.
        if (mouse.leftButton.wasPressedThisFrame)
            _isLeftDragging = true;
        if (mouse.leftButton.wasReleasedThisFrame)
            _isLeftDragging = false;

        if (_isLeftDragging)
        {
            _orbitAngle += mouse.delta.x.ReadValue() * _orbitSensitivity;
        }

        // Calculate orbit position
        float rad = _orbitAngle * Mathf.Deg2Rad;
        Vector3 targetPos = _orbitTarget + new Vector3(
            Mathf.Sin(rad) * _orbitDistance,
            _orbitHeight,
            Mathf.Cos(rad) * _orbitDistance
        );

        _currentPosition = Vector3.SmoothDamp(_currentPosition, targetPos, ref _positionVelocity, _modeTransitionSmoothTime);
        transform.position = _currentPosition;

        // Always look at the orbit target
        Quaternion lookAtTarget = Quaternion.LookRotation(_orbitTarget - _currentPosition);
        _currentRotation = Quaternion.Slerp(_currentRotation, lookAtTarget, 1f - Mathf.Exp(-5f * Time.deltaTime));
        transform.rotation = _currentRotation;
    }

    #endregion

    #region Throw Tracking Mode

    private void UpdateThrowTracking()
    {
        if (_diceManager == null || _diceManager.TrackedDice.Count == 0)
            return;

        Vector3 center;
        float maxRadius;
        if (!ComputeDiceBounds(out center, out maxRadius))
            return;

        // Calculate required distance to frame all dice within the frustum
        float paddedRadius = maxRadius + _trackingPadding;
        float halfFov = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float requiredDistance = paddedRadius / Mathf.Tan(halfFov);
        requiredDistance = Mathf.Clamp(requiredDistance, _trackingMinDistance, _trackingMaxDistance);

        // Position camera using the preserved horizontal angle from orbit
        // so tracking continues from the same general viewing direction
        float vAngle = _trackingViewAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Sin(_trackingHorizontalAngle) * Mathf.Cos(vAngle),
            Mathf.Sin(vAngle),
            Mathf.Cos(_trackingHorizontalAngle) * Mathf.Cos(vAngle)
        ) * requiredDistance;
        offset.y += _trackingHeightOffset;

        Vector3 targetPos = center + offset;

        _currentPosition = Vector3.SmoothDamp(_currentPosition, targetPos, ref _trackingVelocity, _trackingSmoothTime);
        transform.position = _currentPosition;

        Quaternion targetRot = Quaternion.LookRotation(center - _currentPosition);
        float rotT = 1f - Mathf.Exp(-5f * Time.deltaTime);
        _currentRotation = Quaternion.Slerp(_currentRotation, targetRot, rotT);
        transform.rotation = _currentRotation;
    }

    private bool ComputeDiceBounds(out Vector3 center, out float maxRadius)
    {
        center = Vector3.zero;
        maxRadius = 0f;
        int validCount = 0;

        for (int i = 0; i < _diceManager.TrackedDice.Count; i++)
        {
            var die = _diceManager.TrackedDice[i];
            if (die != null)
            {
                center += die.transform.position;
                validCount++;
            }
        }

        if (validCount == 0) return false;
        center /= validCount;

        for (int i = 0; i < _diceManager.TrackedDice.Count; i++)
        {
            var die = _diceManager.TrackedDice[i];
            if (die != null)
            {
                float dist = Vector3.Distance(center, die.transform.position);
                if (dist > maxRadius) maxRadius = dist;
            }
        }

        return true;
    }

    #endregion

    #region Die Focus Mode

    private void DequeueNextDie()
    {
        if (_focusQueue.Count > 0)
        {
            _currentFocusDieIndex = _focusQueue.Dequeue();
            _focusTimer = 0f;
        }
        else
        {
            _currentFocusDieIndex = -1;
        }
    }

    private bool IsDieInFrustum(GameplayDie die)
    {
        Vector3 viewportPoint = _cam.WorldToViewportPoint(die.transform.position);
        // Check that it's in front of the camera and within viewport bounds
        return viewportPoint.z > 0f
            && viewportPoint.x >= 0f && viewportPoint.x <= 1f
            && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }

    private void UpdateDieFocus()
    {
        if (_currentFocusDieIndex < 0)
        {
            if (_focusQueue.Count > 0)
            {
                DequeueNextDie();
            }
            else if (!_allDiceSettled)
            {
                // No dice queued for focus but more dice are still rolling;
                // return to throw tracking to keep following them
                SetMode(CameraMode.ThrowTracking);
                UpdateThrowTracking();
                return;
            }
            else
            {
                return; // all dice settled, hold position until mode change
            }
        }

        GameplayDie targetDie = GetTrackedDie(_currentFocusDieIndex);
        if (targetDie == null)
        {
            DequeueNextDie();
            return;
        }

        // If the current focus die is already visible and there are more in the queue,
        // skip ahead so the camera doesn't linger unnecessarily
        if (_focusQueue.Count > 0 && _focusTimer >= _dieFocusMinDuration && IsDieInFrustum(targetDie))
        {
            DequeueNextDie();
            // Update anchor from current position so the next focus transitions smoothly
            _focusAnchorPosition = _currentPosition;
            return;
        }

        // Adaptive focus duration: shorten when queue is long
        float effectiveDuration = _focusQueue.Count > 2
            ? _dieFocusMinDuration
            : Mathf.Lerp(_dieFocusDuration, _dieFocusMinDuration, _focusQueue.Count / 3f);

        // Stay near the anchor position; only drift slightly toward the die if far away
        Vector3 diePos = targetDie.transform.position;
        Vector3 toDie = diePos - _focusAnchorPosition;
        Vector3 desiredPos = _focusAnchorPosition;
        if (toDie.magnitude > _dieFocusMaxDisplacement)
        {
            desiredPos = _focusAnchorPosition + toDie.normalized * _dieFocusMaxDisplacement;
        }

        _currentPosition = Vector3.SmoothDamp(_currentPosition, desiredPos, ref _positionVelocity, 0.3f);
        transform.position = _currentPosition;

        // Primarily rotate to look at the die
        Quaternion targetRot = Quaternion.LookRotation(diePos - _currentPosition);
        float rotT = 1f - Mathf.Exp(-_dieFocusRotationSpeed * Time.deltaTime);
        _currentRotation = Quaternion.Slerp(_currentRotation, targetRot, rotT);
        transform.rotation = _currentRotation;

        _focusTimer += Time.deltaTime;
        if (_focusTimer >= effectiveDuration)
        {
            DequeueNextDie();
        }
    }

    private GameplayDie GetTrackedDie(int dieIndex)
    {
        if (_diceManager == null) return null;
        var dice = _diceManager.TrackedDice;
        if (dieIndex < 0 || dieIndex >= dice.Count) return null;
        return dice[dieIndex];
    }

    #endregion
}
