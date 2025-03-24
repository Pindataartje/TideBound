using UnityEngine;

public class Bird : MonoBehaviour
{
    [Tooltip("Movement speed of the bird.")]
    public float speed = 2f;
    [Tooltip("How quickly the bird rotates to face its movement direction.")]
    public float rotationSpeed = 2f;
    [Tooltip("Time (in seconds) between random direction changes.")]
    public float randomDirectionChangeInterval = 3f;

    // Reference to the spawn area to keep the bird inside.
    [HideInInspector]
    public BoxCollider spawnArea;

    private Vector3 velocity;
    private float directionChangeTimer = 0f;

    void Start()
    {
        // Start with an upward velocity plus some random horizontal offset.
        SetRandomVelocity();
    }

    void Update()
    {
        // Move the bird.
        transform.position += velocity * Time.deltaTime;

        // Smoothly rotate to face the movement direction.
        if (velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // If the bird leaves the spawn area, steer it back.
        if (spawnArea != null && !spawnArea.bounds.Contains(transform.position))
        {
            Vector3 toCenter = (spawnArea.bounds.center - transform.position).normalized;
            velocity = toCenter * speed;
        }

        // Change direction randomly after a set interval.
        directionChangeTimer += Time.deltaTime;
        if (directionChangeTimer >= randomDirectionChangeInterval)
        {
            SetRandomVelocity();
            directionChangeTimer = 0f;
        }
    }

    void SetRandomVelocity()
    {
        // Base direction is upward, plus a random horizontal component.
        Vector3 randomHorizontal = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        // Combine upward and horizontal. Normalizing ensures consistent speed.
        velocity = (Vector3.up + randomHorizontal).normalized * speed;
    }
}
