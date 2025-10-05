using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Weight Class")]
    [Range(1, 5)] public int weightClass = 1;
    public float baseScale = 0.5f;
    public float scalePerWeightClass = 0.3f;
    
    [Header("Movement")]
    public float maxSpeed = 10f;
    public float maxForce = 1f;
    
    [Header("Perception Radii")]
    public float alignmentRadius = 15f;
    public float cohesionRadius = 50f;
    public float separationRadius = 1f;
    public float predatorDetectionRadius = 5f;
    public float preyDetectionRadius = 10f;
    
    [Header("Behavior Weights")]
    [Range(0f, 5f)] public float alignmentWeight = 1.0f;
    [Range(0f, 5f)] public float cohesionWeight = 1.0f;
    [Range(0f, 5f)] public float separationWeight = 1.5f;
    [Range(0f, 10f)] public float fearWeight = 3.0f;
    [Range(0f, 5f)] public float huntWeight = 2.0f;
    
    private BoidManager manager;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    // Cached neighbors for each behavior
    private System.Collections.Generic.List<Boid> alignmentNeighbors;
    private System.Collections.Generic.List<Boid> cohesionNeighbors;
    private System.Collections.Generic.List<Boid> separationNeighbors;
    private System.Collections.Generic.List<Boid> predators;
    private System.Collections.Generic.List<Boid> prey;
    
    // Public properties
    public Vector3 Position => transform.position;
    public Vector3 Velocity => velocity;
    public int WeightClass => weightClass;

    void Start()
    {
        manager = BoidManager.Instance;
        if (manager == null)
        {
            manager = FindObjectOfType<BoidManager>();
        }
        
        // Set scale based on weight class using logarithmic growth
        // scale = baseScale * (1 + log(weightClass))
        float scale = baseScale * (1 + Mathf.Log(weightClass, 2) * scalePerWeightClass);
        transform.localScale = Vector3.one * scale;
        
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
            
            // Filter alignment and cohesion to same weight class only
            alignmentNeighbors = FilterByWeightClass(alignmentNeighbors, weightClass);
            cohesionNeighbors = FilterByWeightClass(cohesionNeighbors, weightClass);
            
            // Get predators and prey
            predators = manager.GetPredators(this, predatorDetectionRadius);
            prey = manager.GetPrey(this, preyDetectionRadius);
            
            // Debug output (remove after testing)
            if (Time.frameCount % 60 == 0) // Every 60 frames
            {
                Debug.Log($"Boid {name} - Total boids: {manager.Boids.Count}, Alignment neighbors: {alignmentNeighbors.Count}, Cohesion: {cohesionNeighbors.Count}, Separation: {separationNeighbors.Count}");
            }
        }
        
        // Calculate and apply boid behaviors
        Vector3 alignment = CalculateAlignment().normalized * alignmentWeight;
        Vector3 cohesion = CalculateCohesion().normalized * cohesionWeight;
        Vector3 separation = CalculateSeparation().normalized * separationWeight;
        Vector3 fear = CalculateFear().normalized * fearWeight;
        Vector3 hunt = CalculateHunt().normalized * huntWeight;
        
        // React to player
        Vector3 playerReaction = CalculatePlayerReaction() * fearWeight; // Use fear weight for player reactions
        
        // Calculate boundary force to keep fish within bounds
        Vector3 boundary = Vector3.zero;
        if (manager != null)
        {
            boundary = manager.CalculateBoundaryForce(Position);
        }
        
        ApplyForce(alignment);
        ApplyForce(cohesion);
        ApplyForce(separation);
        ApplyForce(fear);
        ApplyForce(hunt);
        ApplyForce(playerReaction);
        ApplyForce(boundary);
        
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

    // FEAR: Flee from larger fish (predators)
    Vector3 CalculateFear()
    {
        if (predators == null || predators.Count == 0) return Vector3.zero;
        
        Vector3 steer = Vector3.zero;
        foreach (var predator in predators)
        {
            Vector3 diff = Position - predator.Position;
            float distance = diff.magnitude;
            
            // Weight by distance and weight class difference (closer and bigger = stronger fear)
            if (distance > 0)
            {
                int weightDiff = predator.WeightClass - weightClass;
                float fearMultiplier = 1.0f + (weightDiff * 0.5f); // More fear for much bigger fish
                diff = diff.normalized / distance * fearMultiplier;
                steer += diff;
            }
        }
        
        if (predators.Count > 0)
        {
            steer /= predators.Count;
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

    // HUNT: Chase smaller fish (prey)
    Vector3 CalculateHunt()
    {
        if (prey == null || prey.Count == 0) return Vector3.zero;
        
        // Find closest prey
        Boid closestPrey = null;
        float closestDistance = float.MaxValue;
        
        foreach (var p in prey)
        {
            float distance = (p.Position - Position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPrey = p;
            }
        }
        
        if (closestPrey == null) return Vector3.zero;
        
        // Chase the closest prey
        Vector3 desired = (closestPrey.Position - Position).normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        
        // Limit steering force
        if (steer.magnitude > maxForce)
        {
            steer = steer.normalized * maxForce;
        }
        
        return steer;
    }

    // PLAYER REACTION: Flee from or chase the player depending on size
    Vector3 CalculatePlayerReaction()
    {
        if (manager == null) return Vector3.zero;
        
        Vector3 playerPos;
        float playerSize;
        
        // Check if player is a predator (should flee)
        if (manager.IsPlayerPredator(this, out playerPos, out playerSize))
        {
            float distance = (playerPos - Position).magnitude;
            
            // Only react if player is within detection radius
            if (distance <= predatorDetectionRadius && distance > 0)
            {
                Vector3 diff = Position - playerPos;
                
                // Weight by distance (closer = stronger fear)
                diff = diff.normalized / distance;
                
                // Make it a steering force
                Vector3 steer = diff.normalized * maxSpeed - velocity;
                
                // Limit steering force
                if (steer.magnitude > maxForce)
                {
                    steer = steer.normalized * maxForce;
                }
                
                return steer;
            }
        }
        // Check if player is prey (should chase)
        else if (manager.IsPlayerPrey(this, out playerPos, out playerSize))
        {
            float distance = (playerPos - Position).magnitude;
            
            // Only react if player is within detection radius
            if (distance <= preyDetectionRadius && distance > 0)
            {
                // Chase the player
                Vector3 desired = (playerPos - Position).normalized * maxSpeed;
                Vector3 steer = desired - velocity;
                
                // Limit steering force
                if (steer.magnitude > maxForce)
                {
                    steer = steer.normalized * maxForce;
                }
                
                // Use hunt weight instead of fear weight
                return steer * (huntWeight / fearWeight); // Adjust for proper weighting
            }
        }
        
        return Vector3.zero;
    }

    // Filter list to only include boids of the same weight class
    System.Collections.Generic.List<Boid> FilterByWeightClass(System.Collections.Generic.List<Boid> boids, int targetWeightClass)
    {
        if (boids == null) return new System.Collections.Generic.List<Boid>();
        
        var filtered = new System.Collections.Generic.List<Boid>();
        foreach (var boid in boids)
        {
            if (boid.WeightClass == targetWeightClass)
            {
                filtered.Add(boid);
            }
        }
        return filtered;
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
