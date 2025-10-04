using System.Collections;
using System.Data;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerFollower : MonoBehaviour
{
    public Transform playerTransform;

    [SerializeField] private float smoothTime = 0.3F;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 2, -5);
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 0);
    private Vector3 currentVelocity;

    private void FixedUpdate()
    {
        Vector3 targetEuler = playerTransform.rotation.eulerAngles;

        // Rotate camera in same direction as playerObject
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            playerTransform.rotation * Quaternion.Euler(rotationOffset),
            Time.deltaTime * (1f / smoothTime)
        );

        // Move camera behind playerObject
        Vector3 targetPosition = playerTransform.position;
        targetPosition += playerTransform.rotation * (positionOffset * playerTransform.localScale.z/2);
        targetPosition += playerTransform.rotation * (positionOffset * playerTransform.localScale.y/4);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
