using UnityEngine;

[ScriptTag("Item")]
public class FlareGun : MonoBehaviour
{
    public GameObject flarePrefab;  // The flare object to be shot
    public Transform shootPoint;    // The point from where the flare will be shot
    public float shootForce = 10f;  // The force with which the flare will be shot
    public float flareLifeTime = 21f;  // Time before the flare is destroyed (21 seconds)
    public AudioClip shootSound;
    public float shootCooldown = 30f; // Cooldown time before shooting again

    private float lastShootTime = -30f; // Track last shoot time, initialized to allow first shot instantly

    void Update()
    {
        // Check if LMB is pressed and cooldown has passed
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootCooldown)
        {
            ShootFlare();
            lastShootTime = Time.time; // Update last shoot time
        }
    }

    void ShootFlare()
    {
        AudioSource.PlayClipAtPoint(shootSound, shootPoint.position, 0.25f);

        // Instantiate the flare at the shoot point
        GameObject flare = Instantiate(flarePrefab, shootPoint.position, shootPoint.rotation);

        // Get the direction the flare should shoot in (straight ahead)
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
