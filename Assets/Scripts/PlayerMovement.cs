using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rigidBody;
    public Camera cam;

    // Variables for rotating player model
    [SerializeField] private float rotationSpeed = 5f;

    // Variables for moving forward
    [SerializeField] private float maxForwardVelocity = 2f;
    [SerializeField] private float timeToReachMaxVelocity = 2f;
    private float currentForwardVelocity = 0f;
    private InputAction moveForwardAction;

    void Start()
    {
        moveForwardAction = InputSystem.actions.FindAction("MoveForward");
    }
    
    void Update()
    {
        
    }
    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private void FixedUpdate()

    {
        // Mouse position in screen space with depth
        Vector3 screenPoint = Input.mousePosition;
        float distanceFromCamera = Vector3.Distance(Camera.main.transform.position, transform.position);
        screenPoint.z = (distanceFromCamera+30)*3;

        // Convert to world space
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(screenPoint);

        // Direction from object to mouse world position
        Vector3 direction = mouseWorldPos - transform.position;
        if (direction.sqrMagnitude < 0.001f) return;

        // Rotate towards mouse world position
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Clamp to avoid weird spinning
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = NormalizeAngle(euler.x);
        euler.x = Mathf.Clamp(euler.x, -45f, 45f); // Setting this any higher will make the controls wonky
        targetRotation = Quaternion.Euler(euler);
        
        float distance = direction.magnitude;
        if (distance > 0.2f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
            



        // Check if we should be moving forward
        bool moveForward = moveForwardAction.ReadValue<float>() > 0 ? true : false;

        // Increase or decrease velocity forward based on input
        if (moveForward)
        {
            transform.localScale += Vector3.forward * Time.deltaTime;
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
