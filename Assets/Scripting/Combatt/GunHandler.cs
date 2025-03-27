using UnityEngine;

public class GunHandler : MonoBehaviour
{
    
    public enum GunType { Crossbow }
    public GunType gunType = GunType.Crossbow;

    [Header("Gun Settings")]
    [Tooltip("Amount of damage this gun deals")]
    public float damage = 10f;

    [Header("Sound Settings")]
    public AudioClip crossbowSound;

    [Header("Animation")]
    private Animator animController; // Reference to the Animator component

    [Header("Spawn Settings")]
    [Tooltip("Transform that marks the spawn point for the projectile")]
    public Transform bulletSpawnPoint;  // Transform for the spawn location

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animController = GetComponent<Animator>();

        if (animController == null)
        {
            Debug.LogWarning("No Animator found on " + gameObject.name);
        }

        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Bullet spawn point not assigned!");
        }

        if (gunType == GunType.Crossbow)
        {
            damage = 50f;
        }
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    // Call this method to trigger a shooting action and instantiate the projectile at the specified spawn point.
    public void Shoot()
    {       
        if (gunType == GunType.Crossbow)
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Bullet spawn point is not assigned!");
            return;
        }

        RaycastHit hit;
        Vector3 direction = bulletSpawnPoint.forward;
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            direction = (ray.direction).normalized;
        }

        if (Physics.Raycast(bulletSpawnPoint.position, direction, out hit, 100f))
        {
            if (hit.collider.CompareTag("Enemy")) // Check if the hit object has the "Enemy" tag
            {
                AnimalAI enemy = hit.collider.GetComponent<AnimalAI>(); // Get the AnimalAI component
                if (enemy != null)
                {
                    enemy.TakeDamage(damage); // Call TakeDamage with the specified damage
                    
                }
            }

        }
    }
}
