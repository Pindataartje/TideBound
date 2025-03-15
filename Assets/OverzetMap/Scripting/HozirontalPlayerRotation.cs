using UnityEngine;

public class HorizontalPlayerRotation : MonoBehaviour
{
    public float mouseSensitivity = 2f;

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }
}
