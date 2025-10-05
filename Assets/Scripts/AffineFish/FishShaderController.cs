using UnityEngine;

[ExecuteAlways]
public class FishShaderController : MonoBehaviour
{
    public Material fishMaterial;
    private Boid boid;
    private Renderer meshRenderer;
    
    [Header("Velocity Scaling")]
    public float velocityMultiplier = 1.0f; // Adjust how much velocity affects the shader
    
    void Start()
    {
        // Get the Boid component
        boid = GetComponent<Boid>();
        
        // Get the renderer to access material instance
        meshRenderer = GetComponent<Renderer>();
        
        // If no material assigned, try to get it from the renderer
        if (fishMaterial == null && meshRenderer != null)
        {
            fishMaterial = meshRenderer.material; // This creates a material instance
        }
    }

    [ExecuteAlways]
    void Update()
    {
        // Use renderer's material if available, otherwise use assigned material
        Material mat = (meshRenderer != null) ? meshRenderer.material : fishMaterial;
        
        if (mat != null)
        {
            mat.SetFloat("_TimeY", Time.time);
            
            // Pass velocity to shader if boid exists
            if (boid != null && Application.isPlaying)
            {
                float velocityMagnitude = boid.Velocity.magnitude * velocityMultiplier;
                mat.SetFloat("_VelocityScale", velocityMagnitude);
            }
            else
            {
                // Default value when not playing or no boid
                mat.SetFloat("_VelocityScale", 1.0f);
            }
        }
    }
}
