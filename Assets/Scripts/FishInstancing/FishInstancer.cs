using UnityEngine;
using System.Collections.Generic;

public class FishInstancer : MonoBehaviour
{
    [Header("Mesh & Material")]
    public Mesh fishMesh;
    public Material fishMaterial;

    [Header("Settings")]
    public int instanceCount = 50;
    public float spawnRadius = 20f;

    [Header("Wave Animation")]
    [Range(0f, 1f)] public float amplitude = 0.1f;
    [Range(0f, 10f)] public float frequency = 1.0f;
    [Range(0f, 10f)] public float speed = 1.0f;

    private Matrix4x4[] matrices;
    private List<Matrix4x4> batch = new List<Matrix4x4>();

    private const int MAX_BATCH_SIZE = 1023;

    void Start()
    {
        // Precompute fish positions and transforms
        matrices = new Matrix4x4[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Random.insideUnitSphere * spawnRadius;
            Quaternion rot = Random.rotation;
            Vector3 scale = Vector3.one * Random.Range(0.8f, 1.2f);

            matrices[i] = Matrix4x4.TRS(pos, rot, scale);
        }
    }

    void Update()
    {
        // Update shader uniforms
        fishMaterial.SetFloat("_Amplitude", amplitude);
        fishMaterial.SetFloat("_Frequency", frequency);
        fishMaterial.SetFloat("_Speed", speed);
        fishMaterial.SetFloat("_TimeY", Time.time);

        // Draw in batches (Unity's limit is 1023 per batch)
        for (int i = 0; i < instanceCount; i += MAX_BATCH_SIZE)
        {
            int batchSize = Mathf.Min(MAX_BATCH_SIZE, instanceCount - i);
            batch.Clear();

            for (int j = 0; j < batchSize; j++)
            {
                batch.Add(matrices[i + j]);
            }

            Graphics.DrawMeshInstanced(fishMesh, 0, fishMaterial, batch);
        }
    }
}

