using UnityEngine;

public class InventoryCameraPivot : MonoBehaviour
{
    [Header("Object to Rotate")]
    // If left empty, the script will rotate the GameObject it's attached to.
    public Transform objectToRotate;

    [Header("Mouse Settings")]
    public float mouseSensitivityX = 1f;
    public float mouseSensitivityY = 1f;
    public float damping = 5f; // How quickly the rotation interpolates to the target

    [Header("Axis Control")]
    public bool enableHorizontalRotation = true;
    public bool enableVerticalRotation = true;

    [Header("Horizontal Clamping (Yaw)")]
    // Rotation around the Y axis (yaw) in degrees
    public bool clampHorizontal = false;
    public float minHorizontalAngle = 0f;
    public float maxHorizontalAngle = 360f;

    [Header("Vertical Clamping (Pitch)")]
    // Rotation around the X axis (pitch) in degrees
    public bool clampVertical = false;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 30f;

    // Internal target angles (in degrees)
    private float targetYaw;   // horizontal rotation (around Y)
    private float targetPitch; // vertical rotation (around X)

    void Start()
    {
        // If no object is assigned, default to the GameObject this script is on.
        if (objectToRotate == null)
            objectToRotate = transform;

        // Initialize target angles from the object's current rotation.
        Vector3 angles = objectToRotate.rotation.eulerAngles;
        targetYaw = angles.y;
        targetPitch = angles.x;
    }

    void Update()
    {
        // Update target yaw based on mouse X movement.
        if (enableHorizontalRotation)
        {
            targetYaw += Input.GetAxis("Mouse X") * mouseSensitivityX;
            if (clampHorizontal)
            {
                targetYaw = Mathf.Clamp(targetYaw, minHorizontalAngle, maxHorizontalAngle);
            }
        }

        // Update target pitch based on mouse Y movement (usually inverted).
        if (enableVerticalRotation)
        {
            targetPitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
            if (clampVertical)
            {
                targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
            }
        }

        // Smoothly interpolate from current rotation to target rotation.
        float currentYaw = objectToRotate.rotation.eulerAngles.y;
        float currentPitch = objectToRotate.rotation.eulerAngles.x;
        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * damping);
        float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * damping);

        // Apply the new rotation. Roll (Z) is set to zero.
        objectToRotate.rotation = Quaternion.Euler(newPitch, newYaw, 0f);
    }
}
