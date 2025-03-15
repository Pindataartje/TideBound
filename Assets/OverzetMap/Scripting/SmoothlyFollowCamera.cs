using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform boat; // Reference to the boat
    public Vector3 offset = new Vector3(0, 5, -10); // Offset relative to the boat
    public float followSpeed = 5f; // Speed at which the camera catches up
    public float rotationSpeed = 5f; // Speed at which the camera rotates to follow the boat

    private void LateUpdate()
    {
        if (boat == null) return;

        // Desired position based on boat's Y-axis rotation and offset
        Vector3 targetPosition = boat.position + Quaternion.Euler(0, boat.eulerAngles.y, 0) * offset;

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Desired rotation to look at the boat
        Quaternion targetRotation = Quaternion.LookRotation(boat.position + Vector3.up * 2f - transform.position);

        // Smoothly rotate the camera to the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
