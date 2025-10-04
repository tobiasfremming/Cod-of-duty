using UnityEngine;

public class Boid : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float maxForce = 0.5f;
    public float neighborRadius = 3f;
    public float separationRadius = 1.5f;
    public float perceptionRadius = 5f;
    
    private BoidManager manager;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    // Public property for easy access to position
    public Vector3 Position => transform.position;

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

    void OnDestroy()
    {
        // Unregister when destroyed
        if (manager != null)
        {
            manager.UnregisterAgent(this);
        }
    }
}
