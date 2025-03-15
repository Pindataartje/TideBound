using UnityEngine;
using Cinemachine;

public class MenuCameraRotate : MonoBehaviour
{
    public Transform target; // The object the camera rotates around
    public float rotationSpeed = 10f; // Speed of rotation

    private void Update()
    {
        if (target == null) return;

        // Rotate around the target on the Y-axis smoothly
        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
