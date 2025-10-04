using UnityEngine;

/// <summary>
/// Applies a material to the renderer and updates shader params
/// without editing the material asset every frame (uses MPB).
/// </summary>
[DisallowMultipleComponent]
public class FishMaterialController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Leave empty to use Renderer on this GameObject.")]
    public Renderer targetRenderer;

    [Header("Base Material (uses Custom/FishAffineLit)")]
    public Material baseMaterial;

    [Header("Material Overrides (per-object)")]
    public Texture2D albedo;
    public Texture2D normalMap;
    public Color color = Color.white;
    [Range(0,1)] public float metallic = 0f;
    [Range(0,1)] public float smoothness = 0.45f;

    [Header("Animation Params")]
    public float amplitude = 0.2f;
    public float frequency = 2.0f;
    public float speed = 2.0f;
    public float rotationAmplitude = 15f;

    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        if (!targetRenderer)
            targetRenderer = GetComponent<Renderer>();

        if (!targetRenderer)
        {
            Debug.LogError("FishMaterialController: No Renderer found.");
            enabled = false;
            return;
        }

        if (baseMaterial)
        {
            // Apply the chosen material to this renderer (shared reference).
            // We'll override per-object values via MPB so we don't dirty the asset.
            targetRenderer.sharedMaterial = baseMaterial;
        }

        _mpb = new MaterialPropertyBlock();
        ApplyToRenderer();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            ApplyToRenderer();
        }
    }

    void Update()
    {
        // Update every frame during Play (for live tweaking)
        ApplyToRenderer();
    }

    private void ApplyToRenderer()
    {
        if (!targetRenderer) return;

        targetRenderer.GetPropertyBlock(_mpb);

        // PBR inputs
        if (albedo)    _mpb.SetTexture("_MainTex", albedo);
        if (normalMap) _mpb.SetTexture("_NormalMap", normalMap);
        _mpb.SetColor("_Color", color);
        _mpb.SetFloat("_Metallic", metallic);
        _mpb.SetFloat("_Smoothness", smoothness);

        // Animation params (names must match the shader)
        _mpb.SetFloat("_Amplitude", amplitude);
        _mpb.SetFloat("_Frequency", frequency);
        _mpb.SetFloat("_Speed", speed);
        _mpb.SetFloat("_RotationAmplitude", rotationAmplitude);

        targetRenderer.SetPropertyBlock(_mpb);
    }
}
