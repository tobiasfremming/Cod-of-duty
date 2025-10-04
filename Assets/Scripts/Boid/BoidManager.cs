using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public static BoidManager Instance;
    public int boidCount = 20;

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

    public void RegisterAgent(Boid agent)
    {
        if (!boids.Contains(agent))
        {
            boids.Add(agent);
        }
    }

    public void UnregisterAgent(Boid agent)
    {
        boids.Remove(agent);
    }

    // Get all boids within a certain radius of the given agent
    public List<Boid> GetNeighbors(Boid agent, float radius)
    {
        List<Boid> neighbors = new List<Boid>();
        float sqrRadius = radius * radius; // Use squared distance to avoid expensive sqrt
        
        foreach (var other in boids)
        {
            if (other == agent) continue; // Skip self
            
            float sqrDistance = (other.transform.position - agent.transform.position).sqrMagnitude;
            if (sqrDistance <= sqrRadius)
            {
                neighbors.Add(other);
            }
        }
        
        return neighbors;
    }
    
    
    
    
}
