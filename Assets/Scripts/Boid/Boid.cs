using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 0.5f;
    
    [Header("Perception Radii")]
    public float alignmentRadius = 30f;
    public float cohesionRadius = 300f;
    public float separationRadius = 3f;
    
    [Header("Behavior Weights")]
    [Range(0f, 5f)] public float alignmentWeight = 1.0f;
    [Range(0f, 5f)] public float cohesionWeight = 1.0f;
    [Range(0f, 5f)] public float separationWeight = 1.5f;
    
    private BoidManager manager;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    // Cached neighbors for each behavior
    private System.Collections.Generic.List<Boid> alignmentNeighbors;
    private System.Collections.Generic.List<Boid> cohesionNeighbors;
    private System.Collections.Generic.List<Boid> separationNeighbors;
    
    // Public properties
    public Vector3 Position => transform.position;
    public Vector3 Velocity => velocity;

    void Start()
    {
        manager = BoidManager.Instance;
        if (manager == null)
        {
            manager = FindObjectOfType<BoidManager>();
        }
        
        // Register this agent with the manager
        if (manager != null)
        {
            manager.RegisterAgent(this);
        }
        
        // Initialize with random velocity
        velocity = Random.insideUnitSphere * maxSpeed * 0.5f;
        acceleration = Vector3.zero;
    }

    void Update()
    {
        // Fetch all neighbors in a single loop
        if (manager != null)
        {
            manager.GetNeighbors(this, alignmentRadius, cohesionRadius, separationRadius,
                out alignmentNeighbors, out cohesionNeighbors, out separationNeighbors);
            
            // Debug: Print neighbor counts (comment out after debugging)
            if (Time.frameCount % 60 == 0) // Only print once per second
            {
                Debug.Log($"Boid {name}: Alignment={alignmentNeighbors?.Count ?? 0}, Cohesion={cohesionNeighbors?.Count ?? 0}, Separation={separationNeighbors?.Count ?? 0}");
            }
        }
        
        // Calculate and apply boid behaviors
        Vector3 alignment = CalculateAlignment() * alignmentWeight;
        Vector3 cohesion = CalculateCohesion() * cohesionWeight;
        Vector3 separation = CalculateSeparation() * separationWeight;
        
        // Debug: Print forces (comment out after debugging)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Forces - A:{alignment.magnitude:F2}, C:{cohesion.magnitude:F2}, S:{separation.magnitude:F2}, Vel:{velocity.magnitude:F2}");
        }
        
        ApplyForce(alignment);
        ApplyForce(cohesion);
        ApplyForce(separation);
        
        // Update movement
        UpdateMovement();
    }

    void UpdateMovement()
    {
        // Update velocity based on acceleration
        velocity += acceleration * Time.deltaTime;
        
        // Limit velocity to max speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        
        // Update position based on velocity
        transform.position += velocity * Time.deltaTime;
        
        // Rotate to face direction of movement
        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
        
        // Reset acceleration for next frame
        acceleration = Vector3.zero;
    }

    // Apply a force to the boid's acceleration
    public void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }

    // ALIGNMENT: Steer towards the average heading of local flockmates
    Vector3 CalculateAlignment()
    {
        if (alignmentNeighbors == null || alignmentNeighbors.Count == 0) return Vector3.zero;
        
        Vector3 averageVelocity = Vector3.zero;
        foreach (var neighbor in alignmentNeighbors)
        {
            averageVelocity += neighbor.Velocity;
        }
        averageVelocity /= alignmentNeighbors.Count;
        
        // Calculate steering force
        Vector3 desired = averageVelocity.normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        
        // Limit steering force
        if (steer.magnitude > maxForce)
        {
            steer = steer.normalized * maxForce;
        }
        
        return steer;
    }

    // COHESION: Steer towards the average position of local flockmates
    Vector3 CalculateCohesion()
    {
        if (cohesionNeighbors == null || cohesionNeighbors.Count == 0) return Vector3.zero;
        
        Vector3 centerOfMass = Vector3.zero;
        foreach (var neighbor in cohesionNeighbors)
        {
            centerOfMass += neighbor.Position;
        }
        centerOfMass /= cohesionNeighbors.Count;
        
        // Calculate steering force towards center of mass
        Vector3 desired = (centerOfMass - Position).normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        
        // Limit steering force
        if (steer.magnitude > maxForce)
        {
            steer = steer.normalized * maxForce;
        }
        
        return steer;
    }

    // SEPARATION: Steer to avoid crowding local flockmates
    Vector3 CalculateSeparation()
    {
        if (separationNeighbors == null || separationNeighbors.Count == 0) return Vector3.zero;
        
        Vector3 steer = Vector3.zero;
        foreach (var neighbor in separationNeighbors)
        {
            Vector3 diff = Position - neighbor.Position;
            float distance = diff.magnitude;
            
            // Weight by distance (closer = stronger push)
            if (distance > 0)
            {
                diff = diff.normalized / distance;
                steer += diff;
            }
        }
        
        if (separationNeighbors.Count > 0)
        {
            steer /= separationNeighbors.Count;
        }
        
        // Make it a steering force
        if (steer.magnitude > 0)
        {
            steer = steer.normalized * maxSpeed - velocity;
            
            // Limit steering force
            if (steer.magnitude > maxForce)
            {
                steer = steer.normalized * maxForce;
            }
        }
        
        return steer;
    }

    void OnDestroy()
    {
        // Unregister when destroyed
        if (manager != null)
        {
            manager.UnregisterAgent(this);
        }
    }
}
