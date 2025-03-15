using UnityEngine;

public class VerticalCameraRotation : MonoBehaviour
{
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 89f;

    private float verticalRotation = 0f;

    private void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
