using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 0.3f;
    public float maxPitch = 0.3f;
    
    [Header("Perception Radii")]
    public float alignmentRadius = 30f;
    public float cohesionRadius = 300f;
    public float separationRadius = 3f;
    
    [Header("Behavior Weights")]
    [Range(0f, 5f)] public float alignmentWeight = 1.0f;
    [Range(0f, 5f)] public float cohesionWeight = 1.0f;
    [Range(0f, 5f)] public float separationWeight = 1.5f;
    
    [SerializeField] public float size = 1f;
    [SerializeField] private float sizeThreshold = 0.5f;
    [SerializeField] private float fleeWeight = 2f;
    [SerializeField] private float sameSizeCohesionWeight = 1f;
    [SerializeField] private float huntWeight = 1.5f;
    
    private BoidManager manager;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    // Cached neighbors for each behavior
    private List<Boid> alignmentNeighbors;
    private List<Boid> cohesionNeighbors;
    private List<Boid> separationNeighbors;
    
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
        size = this.
        // Fetch all neighbors in a single loop
        if (manager != null)
        {
            manager.GetNeighbors(this, alignmentRadius, cohesionRadius, separationRadius,
                out alignmentNeighbors, out cohesionNeighbors, out separationNeighbors);
            
            // Debug: Print neighbor counts (comment out after debugging)
            /*if (Time.frameCount % 60 == 0) // Only print once per second
            {
                Debug.Log($"Boid {name}: Alignment={alignmentNeighbors?.Count ?? 0}, Cohesion={cohesionNeighbors?.Count ?? 0}, Separation={separationNeighbors?.Count ?? 0}");
            }*/
        }
        
        var biggerFish = new List<Boid>();
        var sameSizeFish = new List<Boid>();
        var smallerFish = new List<Boid>();

        foreach (var neighbor in manager.Boids)
        {
            if (neighbor == this) continue;
            if (neighbor.size > size + sizeThreshold)
                biggerFish.Add(neighbor);
            else if (Mathf.Abs(neighbor.size - size) <= sizeThreshold)
                sameSizeFish.Add(neighbor);
            else if (neighbor.size < size - sizeThreshold)
                smallerFish.Add(neighbor);
        }
        
        
        // Flee from bigger fish
        Vector3 flee = Vector3.zero;
        foreach (var big in biggerFish)
            flee += (Position - big.Position).normalized;
        if (flee != Vector3.zero)
            flee = flee.normalized * maxSpeed - velocity;
        
        Vector3 sameSizeCohesion = Vector3.zero;
        if (sameSizeFish.Count > 0)
        {
            Vector3 center = Vector3.zero;
            foreach (var same in sameSizeFish)
                center += same.Position;
            center /= sameSizeFish.Count;
            sameSizeCohesion = (center - Position).normalized * maxSpeed - velocity;
        }
        
        Vector3 hunt = Vector3.zero;
        if (smallerFish.Count > 0)
        {
            Boid closest = smallerFish[0];
            float minDist = (closest.Position - Position).sqrMagnitude;
            foreach (var small in smallerFish)
            {
                float dist = (small.Position - Position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = small;
                }
            }
            hunt = (closest.Position - Position).normalized * maxSpeed - velocity;
        }
        
        // Calculate and apply boid behaviors
        Vector3 alignment = CalculateAlignment() * alignmentWeight;
        Vector3 cohesion = CalculateCohesion() * cohesionWeight;
        Vector3 separation = CalculateSeparation() * separationWeight;
        
        // Debug: Print forces (comment out after debugging)
        /*if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Forces - A:{alignment.magnitude:F2}, C:{cohesion.magnitude:F2}, S:{separation.magnitude:F2}, Vel:{velocity.magnitude:F2}");
        }*/
        
        ApplyForce(flee * fleeWeight);
        ApplyForce(sameSizeCohesion * sameSizeCohesionWeight);
        ApplyForce(hunt * huntWeight);
        ApplyForce(alignment);
        ApplyForce(cohesion);
        ApplyForce(separation);
        
        // Update movement
        UpdateMovement();
    }

    void UpdateMovement()
    {
        
        acceleration.y = Mathf.Clamp(acceleration.y, -maxPitch, maxPitch);
        
        // Update velocity based on acceleration
        velocity += acceleration * Time.deltaTime;
        
        // Limit velocity to max speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        
        // Update position based on velocity
        transform.position += velocity * Time.deltaTime;
        
        // Rotate to face direction of acceleration (if significant), else velocity
        Vector3 direction = acceleration.magnitude > 0.1f ? acceleration.normalized : velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = 5f;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
