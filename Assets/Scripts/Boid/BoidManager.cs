using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public static BoidManager Instance;
    
    [Header("Spawning")]
    public GameObject boidPrefab;
    public int boidCount = 20;
    public float spawnRadius = 10f;
    public bool spawnOnStart = true;

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
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            GameObject boidObj = Instantiate(boidPrefab, randomPos, Quaternion.identity);
            boidObj.name = $"Boid_{i}";
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
