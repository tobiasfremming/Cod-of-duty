using UnityEngine;

[ExecuteAlways]
public class FishShaderController : MonoBehaviour
{
    public Material fishMaterial;

    [ExecuteAlways]
    void Update()
    {
        if (fishMaterial != null)
        {
            fishMaterial.SetFloat("_TimeY", Time.time);
        }
    }
}
