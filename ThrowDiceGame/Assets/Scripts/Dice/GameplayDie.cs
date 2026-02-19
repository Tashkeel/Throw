using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a physical die that can be rolled and reports its top-facing value.
/// Attach this to a prefab containing a Rigidbody and cube collider.
/// </summary>
public class GameplayDie : MonoBehaviour
{
    /// <summary>
    /// Represents a single face of the die with its local direction, value, and text display.
    /// </summary>
    [System.Serializable]
    public class Side
    {
        [Tooltip("The local-space direction this face points when facing outward")]
        public Vector3 localDirection;

        [Tooltip("The value shown on this face")]
        public int value;

        [Tooltip("Whether this face contributes to score or money")]
        public DieSideType sideType = DieSideType.Score;

        [Tooltip("TextMeshPro component to display the value on this face")]
        public TextMeshPro textDisplay;

        public Side(Vector3 direction, int faceValue, DieSideType type = DieSideType.Score)
        {
            localDirection = direction;
            value = faceValue;
            sideType = type;
            textDisplay = null;
        }
    }

    [Header("Die Configuration")]
    [SerializeField]
    [Tooltip("The six sides of this die. Each side has a direction, value, and text display.")]
    private Side[] _sides = new Side[6];

    [Header("Rest Detection")]
    [SerializeField]
    [Tooltip("Velocity threshold below which the die is considered at rest (used as fallback)")]
    private float _restVelocityThreshold = 0.01f;

    [SerializeField]
    [Tooltip("Angular velocity threshold below which the die is considered at rest (used as fallback)")]
    private float _restAngularVelocityThreshold = 0.01f;

    [SerializeField]
    [Tooltip("Time the die must remain at rest before confirming (seconds)")]
    private float _restConfirmationTime = 0.2f;

    [SerializeField]
    [Tooltip("How often to check rest state when monitoring (seconds)")]
    private float _restCheckInterval = 0.1f;

    private static readonly Color ScoreTextColor = new Color(0.2f, 0.6f, 1f);
    private static readonly Color MoneyTextColor = new Color(1f, 0.84f, 0f);

    private Rigidbody _rigidbody;
    private bool _isMonitoringRest;
    private bool _hasReportedRest;
    private float _restTimer;
    private float _restCheckTimer;

    /// <summary>
    /// Fires when this die has confirmed rest. Provides (die, topFaceValue).
    /// </summary>
    public event Action<GameplayDie, int> OnDieAtRest;

    /// <summary>
    /// Returns true if the die has come to rest after being rolled.
    /// </summary>
    public bool IsAtRest
    {
        get
        {
            if (_rigidbody == null) return true;

            // Primary check: Unity's built-in sleep detection
            if (_rigidbody.IsSleeping()) return true;

            // Fallback: manual velocity check
            return _rigidbody.linearVelocity.sqrMagnitude < _restVelocityThreshold * _restVelocityThreshold
                && _rigidbody.angularVelocity.sqrMagnitude < _restAngularVelocityThreshold * _restAngularVelocityThreshold;
        }
    }

    /// <summary>
    /// Gets the value of the top-facing side.
    /// Returns -1 if the die is still in motion or no valid face is found.
    /// </summary>
    public int TopFaceValue
    {
        get
        {
            if (!IsAtRest) return -1;
            return GetTopFaceValue();
        }
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogWarning($"Die '{gameObject.name}' has no Rigidbody component. Physics-based rolling will not work.");
        }

        UpdateAllTextDisplays();
    }

    /// <summary>
    /// Starts monitoring this die for rest. Called by DiceManager after throw.
    /// </summary>
    public void StartMonitoringRest()
    {
        _isMonitoringRest = true;
        _hasReportedRest = false;
        _restTimer = 0f;
        _restCheckTimer = 0f;
    }

    /// <summary>
    /// Stops monitoring this die for rest.
    /// </summary>
    public void StopMonitoringRest()
    {
        _isMonitoringRest = false;
    }

    private void Update()
    {
        if (!_isMonitoringRest || _hasReportedRest) return;

        _restCheckTimer += Time.deltaTime;
        if (_restCheckTimer < _restCheckInterval) return;
        _restCheckTimer = 0f;

        if (IsAtRest)
        {
            _restTimer += _restCheckInterval;
            if (_restTimer >= _restConfirmationTime)
            {
                _hasReportedRest = true;
                _isMonitoringRest = false;
                int value = GetTopFaceValue();
                OnDieAtRest?.Invoke(this, value);
            }
        }
        else
        {
            _restTimer = 0f;
        }
    }

    private void Reset()
    {
        // Called when component is first added or reset in editor
        InitializeDefaultSides();
    }

    private void OnValidate()
    {
        // Ensure we always have exactly 6 sides
        if (_sides == null || _sides.Length != 6)
        {
            InitializeDefaultSides();
        }
    }

    /// <summary>
    /// Initializes sides with standard die values where opposite faces sum to 7.
    /// </summary>
    private void InitializeDefaultSides()
    {
        _sides = new Side[6]
        {
            new Side(Vector3.up, 1),      // +Y = 1
            new Side(Vector3.down, 6),    // -Y = 6
            new Side(Vector3.right, 3),   // +X = 3
            new Side(Vector3.left, 4),    // -X = 4
            new Side(Vector3.forward, 2), // +Z = 2
            new Side(Vector3.back, 5)     // -Z = 5
        };
    }

    /// <summary>
    /// Updates all TextMeshPro displays to show their corresponding side values.
    /// </summary>
    public void UpdateAllTextDisplays()
    {
        if (_sides == null) return;

        foreach (var side in _sides)
        {
            UpdateTextDisplay(side);
        }
    }

    /// <summary>
    /// Updates a single side's text display to show its value.
    /// </summary>
    private void UpdateTextDisplay(Side side)
    {
        if (side?.textDisplay != null)
        {
            side.textDisplay.text = side.value.ToString();
            side.textDisplay.color = side.sideType == DieSideType.Money
                ? MoneyTextColor
                : ScoreTextColor;
        }
    }

    /// <summary>
    /// Calculates which face is currently on top by finding the face
    /// whose outward direction most closely aligns with world up.
    /// </summary>
    private int GetTopFaceValue()
    {
        if (_sides == null || _sides.Length == 0) return -1;

        float maxDot = float.MinValue;
        int topValue = -1;

        foreach (var side in _sides)
        {
            if (side == null) continue;

            // Transform the local face direction to world space
            Vector3 worldDirection = transform.TransformDirection(side.localDirection);

            // Check how closely this face points upward
            float dot = Vector3.Dot(worldDirection, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                topValue = side.value;
            }
        }

        return topValue;
    }

    /// <summary>
    /// Gets the side type of the top-facing face.
    /// Returns DieSideType.Score by default if not at rest or no valid face is found.
    /// </summary>
    public DieSideType GetTopFaceSideType()
    {
        if (_sides == null || _sides.Length == 0) return DieSideType.Score;

        float maxDot = float.MinValue;
        DieSideType topType = DieSideType.Score;

        foreach (var side in _sides)
        {
            if (side == null) continue;

            Vector3 worldDirection = transform.TransformDirection(side.localDirection);
            float dot = Vector3.Dot(worldDirection, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                topType = side.sideType;
            }
        }

        return topType;
    }

    /// <summary>
    /// Sets all side types at once.
    /// </summary>
    /// <param name="types">Array of 6 DieSideType values corresponding to: +Y, -Y, +X, -X, +Z, -Z</param>
    public void SetAllSideTypes(DieSideType[] types)
    {
        if (types == null || types.Length != 6)
        {
            Debug.LogError("SetAllSideTypes requires exactly 6 values.");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            _sides[i].sideType = types[i];
        }

        UpdateAllTextDisplays();
    }

    /// <summary>
    /// Attempts to get the top face value. Returns true if the die is at rest and a value was found.
    /// </summary>
    /// <param name="value">The top face value, or 0 if not at rest or invalid.</param>
    /// <returns>True if a valid value was retrieved.</returns>
    public bool TryGetTopFaceValue(out int value)
    {
        value = 0;

        if (!IsAtRest) return false;

        value = GetTopFaceValue();
        return value > 0;
    }

    /// <summary>
    /// Gets a copy of all side configurations.
    /// </summary>
    public Side[] GetSides()
    {
        var copy = new Side[_sides.Length];
        for (int i = 0; i < _sides.Length; i++)
        {
            copy[i] = new Side(_sides[i].localDirection, _sides[i].value, _sides[i].sideType);
        }
        return copy;
    }

    /// <summary>
    /// Sets the value for a specific side by index and updates its text display.
    /// </summary>
    /// <param name="sideIndex">Index of the side (0-5).</param>
    /// <param name="value">New value for the side.</param>
    public void SetSideValue(int sideIndex, int value)
    {
        if (sideIndex < 0 || sideIndex >= _sides.Length)
        {
            Debug.LogError($"Invalid side index: {sideIndex}. Must be 0-5.");
            return;
        }

        _sides[sideIndex].value = value;
        UpdateTextDisplay(_sides[sideIndex]);
    }

    /// <summary>
    /// Sets all side values at once and updates all text displays.
    /// </summary>
    /// <param name="values">Array of 6 values corresponding to: +Y, -Y, +X, -X, +Z, -Z</param>
    public void SetAllSideValues(int[] values)
    {
        if (values == null || values.Length != 6)
        {
            Debug.LogError("SetAllSideValues requires exactly 6 values.");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            _sides[i].value = values[i];
        }

        UpdateAllTextDisplays();
    }
}
