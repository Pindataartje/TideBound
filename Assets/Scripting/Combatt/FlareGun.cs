using UnityEngine;

public class FlareGun : MonoBehaviour
{
    public GameObject flarePrefab;  // The flare object to be shot
    public Transform shootPoint;    // The point from where the flare will be shot
    public float shootForce = 10f;  // The force with which the flare will be shot
    public float flareLifeTime = 21f;  // Time before the flare is destroyed (21 seconds)

    void Update()
    {
        // Check if the player presses the fire button (can be customized to any key or input)
        if (Input.GetButtonDown("Fire1"))
        {
            ShootFlare();
        }
    }

    void ShootFlare()
    {
        // Instantiate the flare at the shoot point
        GameObject flare = Instantiate(flarePrefab, shootPoint.position, shootPoint.rotation);

        // Get the direction the flare should shoot in (straight ahead, regardless of gun's rotation)
        // Since the gun is rotated -90 on the Y-axis, we need to shoot along the local forward direction (Z axis)
        Vector3 shootDirection = shootPoint.forward;

        // Apply force to the flare to shoot it
        Rigidbody flareRb = flare.GetComponent<Rigidbody>();
        if (flareRb != null)
        {
            flareRb.AddForce(shootDirection * shootForce, ForceMode.VelocityChange);
        }

        // Destroy the flare after a set time (21 seconds)
        
        Destroy(flare, flareLifeTime);
    }
}
