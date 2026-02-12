using UnityEngine;

/// <summary>
/// Represents a single 3D die preview instance that can be rotated.
/// </summary>
public class DicePreviewInstance : MonoBehaviour
{
    // Standard face directions matching Die/DiceData order: +Y, -Y, +X, -X, +Z, -Z
    private static readonly Vector3[] FaceDirections = new Vector3[]
    {
        Vector3.up,      // 0: +Y
        Vector3.down,    // 1: -Y
        Vector3.right,   // 2: +X
        Vector3.left,    // 3: -X
        Vector3.forward, // 4: +Z
        Vector3.back     // 5: -Z
    };

    private Camera _previewCamera;
    private Vector3 _defaultRotation;
    private Renderer[] _renderers;

    /// <summary>
    /// Current rotation of the preview die.
    /// </summary>
    public Quaternion Rotation
    {
        get => transform.localRotation;
        set => transform.localRotation = value;
    }

    public void Initialize(Camera previewCamera)
    {
        _previewCamera = previewCamera;
        _defaultRotation = transform.localEulerAngles;

        // Cache all renderers for visibility toggling
        _renderers = GetComponentsInChildren<Renderer>(true);

        // Set a nice default viewing angle
        transform.localRotation = Quaternion.Euler(25f, -35f, 0f);
    }

    /// <summary>
    /// Sets visibility of all renderers on this preview die.
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (_renderers == null) return;

        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
    }

    /// <summary>
    /// Rotates the die based on drag delta.
    /// </summary>
    /// <param name="delta">Mouse/touch drag delta.</param>
    /// <param name="sensitivity">Rotation sensitivity multiplier.</param>
    public void Rotate(Vector2 delta, float sensitivity = 0.5f)
    {
        // Rotate around world up (Y) for horizontal drag
        // Rotate around camera right (X) for vertical drag
        float rotationX = -delta.y * sensitivity;
        float rotationY = delta.x * sensitivity;

        transform.Rotate(Vector3.up, rotationY, Space.World);
        transform.Rotate(Vector3.right, rotationX, Space.World);
    }

    /// <summary>
    /// Returns the index (0-5) of the face most directly facing the preview camera.
    /// Indices match DiceData face order: +Y, -Y, +X, -X, +Z, -Z.
    /// </summary>
    public int GetCameraFacingFaceIndex()
    {
        if (_previewCamera == null) return -1;

        Vector3 dieToCamera = (_previewCamera.transform.position - transform.position).normalized;

        float maxDot = float.MinValue;
        int bestIndex = 0;

        for (int i = 0; i < FaceDirections.Length; i++)
        {
            Vector3 worldDir = transform.TransformDirection(FaceDirections[i]);
            float dot = Vector3.Dot(worldDir, dieToCamera);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    /// <summary>
    /// Resets rotation to default viewing angle.
    /// </summary>
    public void ResetRotation()
    {
        transform.localRotation = Quaternion.Euler(25f, -35f, 0f);
    }

    /// <summary>
    /// Destroys this preview instance.
    /// </summary>
    public void Cleanup()
    {
        // Unregister from renderer
        if (DicePreviewRenderer.Instance != null)
        {
            DicePreviewRenderer.Instance.UnregisterPreview(this);
        }

        Destroy(gameObject);
    }
}
