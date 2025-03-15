using UnityEngine;

public class VerticalRotation : MonoBehaviour
{
    public float sensitivity = 3f;
    public float minAngle = -88f;
    public float maxAngle = 88f;

    // Tracks the current vertical rotation angle
    private float verticalRotation = 0f;

    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        verticalRotation -= mouseY; // Subtract to invert mouse Y if needed
        verticalRotation = Mathf.Clamp(verticalRotation, minAngle, maxAngle);

        // Apply the rotation on the X axis
        transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }
}
