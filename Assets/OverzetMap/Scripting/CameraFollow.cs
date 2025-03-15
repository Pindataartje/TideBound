using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform boat; // The boat the camera follows
    public float followSpeed = 5f; // Speed at which the camera follows the boat
    public float rotationSpeed = 5f; // Speed at which the camera aligns with the boat's Y-axis rotation
    public Vector3 offset = new Vector3(0, 5, -10); // Offset from the boat position

    private Vector3 targetPosition;

    private void LateUpdate()
    {
        if (boat == null) return;

        // Calculate the target position
        targetPosition = boat.position + offset;

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Only follow the Y-axis rotation of the boat
        Quaternion targetRotation = Quaternion.Euler(0, boat.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
