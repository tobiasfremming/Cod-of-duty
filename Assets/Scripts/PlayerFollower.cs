using System.Collections;
using System.Data;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerFollower : MonoBehaviour
{
    public GameObject playerObject;

    [SerializeField] private float smoothTime = 0.3F;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 2, -5);
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 0);
    private Vector3 currentVelocity;

    private void FixedUpdate()
    {
        Vector3 targetEuler = playerObject.transform.rotation.eulerAngles;

        // Rotate camera in same direction as playerObject
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            playerObject.transform.rotation * Quaternion.Euler(rotationOffset),
            Time.deltaTime * (1f / smoothTime)
        );

        // Move camera behind playerObject
        Vector3 targetPosition = playerObject.transform.TransformPoint(positionOffset);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
