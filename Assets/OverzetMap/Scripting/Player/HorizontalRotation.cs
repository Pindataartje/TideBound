using UnityEngine;

public class HorizontalRotation : MonoBehaviour
{
    public float sensitivity = 3f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        // Rotate around Y axis
        transform.Rotate(0f, mouseX, 0f);
    }
}
