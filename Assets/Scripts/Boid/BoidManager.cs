using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public static BoidManager Instance;
    
    [Header("Spawning")]
    public GameObject boidPrefab;
    public int boidCount = 100;
    public bool spawnOnStart = true;
    public bool randomRotation = true;
    public bool randomScale = false;
    public Vector2 scaleRange = new Vector2(1f, 2f);
    
    [Header("Boundary Box")]
    public Vector3 boundaryCenter = Vector3.zero;
    public Vector3 boundarySize = new Vector3(1000, 500, 1000);
    public float boundaryTurnForce = 5.0f;
    public Color boundaryColor = new Color(0, 1, 1, 0.3f);
    public bool showBoundaryGizmo = true;
    
    [Header("Player")]
    public GameObject playerFish; // Reference to the player fish
    private Fish playerFishComponent;

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
        // Try to find player fish if not assigned
        if (playerFish == null)
        {
            playerFish = GameObject.Find("playerfish");
        }
        
        // Get the Fish component from the player
        if (playerFish != null)
        {
            playerFishComponent = playerFish.GetComponent<Fish>();
        }
        
        if (spawnOnStart && boidPrefab != null)
        {
            SpawnBoids();
        }
    }

    void SpawnBoids()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 randomPos = boundaryCenter + new Vector3(
                Random.Range(-boundarySize.x / 2, boundarySize.x / 2),
                Random.Range(-boundarySize.y / 2, boundarySize.y / 2),
                Random.Range(-boundarySize.z / 2, boundarySize.z / 2)
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

    // Get all predators (boids with higher weight class) within radius
    public List<Boid> GetPredators(Boid agent, float radius)
    {
        List<Boid> predators = new List<Boid>();
        float sqrRadius = radius * radius;
        
        foreach (var other in boids)
        {
            if (other == agent) continue;
            
            // Only consider fish with higher weight class as predators
            if (other.WeightClass > agent.WeightClass)
            {
                float sqrDistance = (other.transform.position - agent.transform.position).sqrMagnitude;
                if (sqrDistance <= sqrRadius)
                {
                    predators.Add(other);
                }
            }
        }
        
        return predators;
    }
    
    // Check if player is a threat to this boid (player is bigger)
    public bool IsPlayerPredator(Boid agent, out Vector3 playerPosition, out float playerSize)
    {
        playerPosition = Vector3.zero;
        playerSize = 0f;
        
        if (playerFishComponent == null || playerFish == null) return false;
        
        playerPosition = playerFish.transform.position;
        playerSize = playerFishComponent.Size;
        
        // Convert boid weight class to approximate size for comparison
        float boidSize = agent.transform.localScale.x; // Use actual scale
        
        // Player is a predator if they're significantly larger
        return playerSize > boidSize * 1.2f; // 20% larger threshold
    }
    
    // Check if player is prey for this boid (boid is bigger)
    public bool IsPlayerPrey(Boid agent, out Vector3 playerPosition, out float playerSize)
    {
        playerPosition = Vector3.zero;
        playerSize = 0f;
        
        if (playerFishComponent == null || playerFish == null) return false;
        
        playerPosition = playerFish.transform.position;
        playerSize = playerFishComponent.Size;
        
        // Convert boid weight class to approximate size for comparison
        float boidSize = agent.transform.localScale.x; // Use actual scale
        
        // Player is prey if boid is significantly larger
        return boidSize > playerSize * 1.2f; // 20% larger threshold
    }

    // Get all prey (boids with lower weight class) within radius
    public List<Boid> GetPrey(Boid agent, float radius)
    {
        List<Boid> prey = new List<Boid>();
        float sqrRadius = radius * radius;
        
        foreach (var other in boids)
        {
            if (other == agent) continue;
            
            // Only consider fish with lower weight class as prey
            if (other.WeightClass < agent.WeightClass)
            {
                float sqrDistance = (other.transform.position - agent.transform.position).sqrMagnitude;
                if (sqrDistance <= sqrRadius)
                {
                    prey.Add(other);
                }
            }
        }
        
        return prey;
    }

    // Calculate force to keep boid within boundary box
    public Vector3 CalculateBoundaryForce(Vector3 position)
    {
        Vector3 force = Vector3.zero;
        Vector3 halfSize = boundarySize / 2;
        Vector3 min = boundaryCenter - halfSize;
        Vector3 max = boundaryCenter + halfSize;
        
        // Check each axis and apply force to turn back if near boundary
        if (position.x < min.x)
        {
            force.x = (min.x - position.x) * boundaryTurnForce;
        }
        else if (position.x > max.x)
        {
            force.x = (max.x - position.x) * boundaryTurnForce;
        }
        
        if (position.y < min.y)
        {
            force.y = (min.y - position.y) * boundaryTurnForce;
        }
        else if (position.y > max.y)
        {
            force.y = (max.y - position.y) * boundaryTurnForce;
        }
        
        if (position.z < min.z)
        {
            force.z = (min.z - position.z) * boundaryTurnForce;
        }
        else if (position.z > max.z)
        {
            force.z = (max.z - position.z) * boundaryTurnForce;
        }
        
        return force;
    }

    // Visualize the boundary box in the Scene view
    void OnDrawGizmos()
    {
        if (showBoundaryGizmo)
        {
            Gizmos.color = boundaryColor;
            Gizmos.DrawWireCube(boundaryCenter, boundarySize);
            
            // Draw a semi-transparent box
            Color fillColor = boundaryColor;
            fillColor.a = 0.1f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(boundaryCenter, boundarySize);
        }
    }
}
