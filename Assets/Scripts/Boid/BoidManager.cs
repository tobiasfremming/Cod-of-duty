using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public static BoidManager Instance;
    
    [Header("Spawning")]
    public GameObject boidPrefab;
    public int boidCount = 20;
    public bool spawnOnStart = true;
    public Vector3 spawnArea = new Vector3(10, 5, 10); // Size of spawn box
    public bool randomRotation = true;
    public bool randomScale = false;
    public Vector2 scaleRange = new Vector2(1f, 2f);

    private List<Boid> boids = new List<Boid>();

    public List<Boid> Boids => boids;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (spawnOnStart && boidPrefab != null)
        {
            SpawnBoids();
        }
    }

    void SpawnBoids()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            Quaternion rotation = randomRotation ? Random.rotation : Quaternion.identity;

            GameObject obj = Instantiate(boidPrefab, randomPos, rotation);

            if (randomScale)
            {
                float scale = Random.Range(scaleRange.x, scaleRange.y);
                obj.transform.localScale = Vector3.one * scale;
                
                var creature = obj.GetComponent<EatableEntity>();
                if (creature != null)
                {
                    creature.SetSize(scale);
                }
            }
        }
        
        Debug.Log($"Spawned {boidCount} boids");
    }

    public void RegisterAgent(Boid agent)
    {
        if (!boids.Contains(agent))
        {
            boids.Add(agent);
            Debug.Log($"Registered boid {agent.name}. Total boids: {boids.Count}");
        }
    }

    public void UnregisterAgent(Boid agent)
    {
        boids.Remove(agent);
    }

    // Get all boids within different radii in a single loop
    public void GetNeighbors(Boid agent, float alignmentRadius, float cohesionRadius, float separationRadius,
        out List<Boid> alignmentNeighbors, out List<Boid> cohesionNeighbors, out List<Boid> separationNeighbors)
    {
        alignmentNeighbors = new List<Boid>();
        cohesionNeighbors = new List<Boid>();
        separationNeighbors = new List<Boid>();
        
        // Calculate squared radii to avoid expensive sqrt
        float sqrAlignmentRadius = alignmentRadius * alignmentRadius;
        float sqrCohesionRadius = cohesionRadius * cohesionRadius;
        float sqrSeparationRadius = separationRadius * separationRadius;
        
        // Single loop through all boids
        foreach (var other in boids)
        {
            if (other == agent) continue; // Skip self
            
            float sqrDistance = (other.transform.position - agent.transform.position).sqrMagnitude;
            
            // Check against each radius and add to appropriate lists
            if (sqrDistance <= sqrAlignmentRadius)
            {
                alignmentNeighbors.Add(other);
            }
            if (sqrDistance <= sqrCohesionRadius)
            {
                cohesionNeighbors.Add(other);
            }
            if (sqrDistance <= sqrSeparationRadius)
            {
                separationNeighbors.Add(other);
            }
        }
    }
    
    // Keep old method for backwards compatibility if needed
    public List<Boid> GetNeighbors(Boid agent, float radius)
    {
        List<Boid> neighbors = new List<Boid>();
        float sqrRadius = radius * radius;
        
        foreach (var other in boids)
        {
            if (other == agent) continue;
            
            float sqrDistance = (other.transform.position - agent.transform.position).sqrMagnitude;
            if (sqrDistance <= sqrRadius)
            {
                neighbors.Add(other);
            }
        }
        
        return neighbors;
    }
    
    
    
    
}
