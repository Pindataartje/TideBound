using UnityEngine;

public class ConstrainCameraRotation : MonoBehaviour
{
    public Transform boat; // Reference point the camera will look at
    public Vector3 offset = new Vector3(0, 5, -10); // Initial offset
    public float rotationSpeed = 3f; // Speed of camera rotation
    public float zoomSpeed = 5f; // Speed of zooming
    public float minRadius = 5f; // Minimum zoom radius
    public float maxRadius = 20f; // Maximum zoom radius

    private float currentRadius; // Current distance from the boat
    private float desiredRadius; // Target distance for smooth zooming
    private float yaw; // Horizontal rotation angle
    private float pitch = 20f; // Vertical rotation angle (default to a slight angle)

    private void Start()
    {
        // Initialize the radius based on the initial offset
        currentRadius = offset.magnitude;
        desiredRadius = currentRadius;
    }

    private void LateUpdate()
    {
        if (boat == null) return;

        // Handle rotation with mouse input
        yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        pitch = Mathf.Clamp(pitch, 5f, 80f); // Clamp pitch to prevent flipping

        // Handle zooming with the scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            desiredRadius = Mathf.Clamp(desiredRadius - scroll * zoomSpeed, minRadius, maxRadius);
        }

        // Smoothly transition the radius to the desired value
        currentRadius = Mathf.Lerp(currentRadius, desiredRadius, Time.deltaTime * zoomSpeed);

        // Calculate the new offset based on yaw, pitch, and current radius
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 newOffset = rotation * Vector3.back * currentRadius;

        // Update the camera's position and rotation
        transform.position = boat.position + newOffset;
        transform.LookAt(boat.position); // Always look at the boat
    }
}
