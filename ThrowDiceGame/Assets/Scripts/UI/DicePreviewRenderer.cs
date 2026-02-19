using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a camera and render texture for displaying 3D dice previews in UI.
/// Create one instance in the scene to handle all dice preview rendering.
/// </summary>
public class DicePreviewRenderer : MonoBehaviour
{
    [Header("Rendering Setup")]
    [SerializeField]
    [Tooltip("Resolution of the render texture")]
    private int _renderTextureSize = 256;

    [SerializeField]
    [Tooltip("Layer for preview dice (should be unique, e.g., 'DicePreview')")]
    private LayerMask _previewLayer;

    [SerializeField]
    [Tooltip("Background color for the preview")]
    private Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0f);

    [Header("Camera Settings")]
    [SerializeField]
    [Tooltip("Distance from camera to dice")]
    private float _cameraDistance = 2f;

    [SerializeField]
    [Tooltip("Field of view for preview camera")]
    private float _fieldOfView = 30f;

    [Header("Die Settings")]
    [SerializeField]
    [Tooltip("Scale of the preview die (adjust to fit within UI bounds)")]
    [Range(0.1f, 2f)]
    private float _previewScale = 1f;

    private Camera _previewCamera;
    private Transform _previewRoot;
    private int _previewLayerIndex;
    private List<DicePreviewInstance> _activeInstances = new List<DicePreviewInstance>();

    /// <summary>
    /// Singleton instance for easy access.
    /// </summary>
    public static DicePreviewRenderer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        // Find the preview layer index
        _previewLayerIndex = GetLayerIndex(_previewLayer);
        if (_previewLayerIndex == -1)
        {
            // Default to layer 31 if not set
            _previewLayerIndex = 31;
            Debug.LogWarning("DicePreviewRenderer: Preview layer not set. Using layer 31. Create a 'DicePreview' layer for best results.");
        }

        // Create preview root (holds all preview dice, positioned far away)
        _previewRoot = new GameObject("DicePreviewRoot").transform;
        _previewRoot.SetParent(transform);
        _previewRoot.position = new Vector3(1000f, 1000f, 1000f);

        // Create preview camera
        var cameraObj = new GameObject("DicePreviewCamera");
        cameraObj.transform.SetParent(_previewRoot);
        cameraObj.transform.localPosition = new Vector3(0f, 0f, -_cameraDistance);
        cameraObj.transform.localRotation = Quaternion.identity;

        _previewCamera = cameraObj.AddComponent<Camera>();
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = _backgroundColor;
        _previewCamera.cullingMask = 1 << _previewLayerIndex;
        _previewCamera.fieldOfView = _fieldOfView;
        _previewCamera.nearClipPlane = 0.1f;
        _previewCamera.farClipPlane = 100f;
        _previewCamera.enabled = false; // We'll render manually

        // Create preview light for illuminating the dice
        var lightObj = new GameObject("DicePreviewLight");
        lightObj.transform.SetParent(_previewRoot);
        lightObj.transform.localPosition = new Vector3(1f, 2f, -1f);
        lightObj.transform.LookAt(_previewRoot.position);
        lightObj.layer = _previewLayerIndex;

        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.cullingMask = 1 << _previewLayerIndex;
    }

    private int GetLayerIndex(LayerMask mask)
    {
        int layerMaskValue = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((layerMaskValue & (1 << i)) != 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Creates a render texture for a dice preview.
    /// </summary>
    public RenderTexture CreateRenderTexture()
    {
        var rt = new RenderTexture(_renderTextureSize, _renderTextureSize, 16);
        rt.antiAliasing = 4;
        rt.Create();
        return rt;
    }

    /// <summary>
    /// Creates a preview die instance from the given prefab.
    /// </summary>
    /// <param name="diePrefab">The die prefab to instantiate.</param>
    /// <param name="diceData">The dice data to configure face values.</param>
    /// <returns>The preview die instance.</returns>
    public DicePreviewInstance CreatePreviewDie(GameplayDie diePrefab, InventoryDie inventoryDie)
    {
        if (diePrefab == null) return null;

        // Instantiate the die
        var dieObj = Instantiate(diePrefab.gameObject, _previewRoot);
        dieObj.name = "PreviewDie";

        // Reset local position/rotation to center the die in the preview
        // (prefabs may have non-zero positions from scene editing)
        dieObj.transform.localPosition = Vector3.zero;
        dieObj.transform.localRotation = Quaternion.identity;
        dieObj.transform.localScale = Vector3.one * _previewScale;

        // Set layer recursively
        SetLayerRecursively(dieObj, _previewLayerIndex);

        // Disable physics
        var rb = dieObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable colliders
        foreach (var collider in dieObj.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        // Configure face values if dice data provided
        var currentDie = dieObj.GetComponent<GameplayDie>();
        if (currentDie != null && inventoryDie != null)
        {
            currentDie.SetAllSideValues(inventoryDie.GetFaceValues());
            currentDie.SetAllSideTypes(inventoryDie.GetFaceTypes());
        }

        // Create and return the preview instance wrapper
        var preview = dieObj.AddComponent<DicePreviewInstance>();
        preview.Initialize(_previewCamera);

        // Track this instance
        _activeInstances.Add(preview);

        return preview;
    }

    /// <summary>
    /// Unregisters a preview instance when it's destroyed.
    /// </summary>
    public void UnregisterPreview(DicePreviewInstance preview)
    {
        _activeInstances.Remove(preview);
    }

    /// <summary>
    /// Renders a preview die to the given render texture.
    /// Only the specified preview will be visible during rendering.
    /// </summary>
    public void RenderPreview(DicePreviewInstance preview, RenderTexture targetTexture)
    {
        if (preview == null || targetTexture == null || _previewCamera == null) return;

        // Hide all other preview instances
        foreach (var instance in _activeInstances)
        {
            if (instance != null && instance != preview)
            {
                instance.SetVisible(false);
            }
        }

        // Ensure the target preview is visible
        preview.SetVisible(true);

        // Position camera to look at this specific die
        _previewCamera.transform.position = preview.transform.position + new Vector3(0f, 0f, -_cameraDistance);
        _previewCamera.transform.LookAt(preview.transform.position);

        // Render to texture
        _previewCamera.targetTexture = targetTexture;
        _previewCamera.Render();
        _previewCamera.targetTexture = null;

        // Re-enable all preview instances
        foreach (var instance in _activeInstances)
        {
            if (instance != null)
            {
                instance.SetVisible(true);
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
