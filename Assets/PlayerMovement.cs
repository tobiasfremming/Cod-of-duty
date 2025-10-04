using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rigidBody;
    public Camera cam;

    // Variables for rotating player model
    public float rotationSpeed = 5f;
    
    // Variables for moving forward
    public float maxForwardVelocity = 2f;
    public float timeToReachMaxVelocity = 2f;
    private float currentForwardVelocity = 0f;

    InputAction moveForwardAction;
    void Start()
    {
        moveForwardAction = InputSystem.actions.FindAction("MoveForward");
    }
    
    void Update()
    {
        // Mouse position in screen space with depth
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10f;

        // Convert to world space
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(screenPoint);

        // Direction from object to mouse world position
        Vector3 direction = mouseWorldPos - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            // Rotate towards mouse world position
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Check if we should be moving forward
        bool moveForward = moveForwardAction.ReadValue<float>() > 0 ? true : false;

        // Increase or decrease velocity forward based on input
        if (moveForward)
        {
            if (currentForwardVelocity < maxForwardVelocity)
            {
                currentForwardVelocity += (maxForwardVelocity / timeToReachMaxVelocity) * Time.deltaTime;
            }
            else
            {
                currentForwardVelocity = maxForwardVelocity;
            }
        }
        else
        {            
            if (currentForwardVelocity < 0f)
            {
                currentForwardVelocity = 0f;
            }
            else
            {
                currentForwardVelocity -= (maxForwardVelocity / timeToReachMaxVelocity) * Time.deltaTime;
            }

        }

        // Move forward with set velocity
        rigidBody.linearVelocity = transform.forward * currentForwardVelocity;
    }
}
