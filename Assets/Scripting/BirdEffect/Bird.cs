using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("Minimum speed for birds.")]
    public float minSpeed = 1f;
    [Tooltip("Maximum speed for birds.")]
    public float maxSpeed = 3f;
    private float speed;

    [Header("Movement Settings")]
    [Tooltip("Time interval (min, max) for target changes (in seconds).")]
    public Vector2 targetChangeIntervalRange = new Vector2(2f, 6f);
    private float targetChangeTimer;
    private float currentTargetChangeInterval;

    [Tooltip("The BoxCollider that defines the movement area (set this from the spawner).")]
    public BoxCollider movementArea;
    private Vector3 targetPosition;

    [Header("Smoothing & Rotation")]
    [Tooltip("How quickly the bird adjusts its velocity.")]
    public float smoothTime = 0.5f;
    private Vector3 currentVelocity; // used by SmoothDamp
    [Tooltip("How quickly the bird rotates to face its travel direction.")]
    public float rotationSpeed = 2f;
    private Vector3 velocity;

    [Header("Oscillation Settings")]
    [Tooltip("Amplitude of vertical oscillation (up/down movement).")]
    public float oscillationAmplitude = 0.5f;
    [Tooltip("Frequency of vertical oscillation.")]
    public float oscillationFrequency = 1f;
    private float oscillationOffset;

    void Start()
    {
        SetRandomSpeed();
        PickNewTarget();
        oscillationOffset = Random.Range(0f, 2 * Mathf.PI);
        // Start with an initial upward velocity.
        velocity = Vector3.up;
    }

    void Update()
    {
        // Update target timer; pick a new target if time has elapsed.
        targetChangeTimer += Time.deltaTime;
        if (targetChangeTimer >= currentTargetChangeInterval)
        {
            PickNewTarget();
        }

        // Compute horizontal direction toward target (ignore vertical for smoother turning).
        Vector3 horizontalDir = (targetPosition - transform.position);
        horizontalDir.y = 0f;
        Vector3 desiredHorizontalVelocity = horizontalDir.normalized * speed;

        // Compute vertical oscillation.
        float verticalOscillation = Mathf.Sin(Time.time * oscillationFrequency + oscillationOffset) * oscillationAmplitude;

        // Combine desired horizontal velocity and vertical oscillation.
        Vector3 desiredVelocity = desiredHorizontalVelocity;
        desiredVelocity.y = verticalOscillation;

        // Smoothly adjust current velocity toward desired velocity.
        velocity = Vector3.SmoothDamp(velocity, desiredVelocity, ref currentVelocity, smoothTime);

        // Move the bird.
        transform.position += velocity * Time.deltaTime;

        // Rotate smoothly to face the direction of travel.
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // Randomizes the speed.
    public void SetRandomSpeed()
    {
        speed = Random.Range(minSpeed, maxSpeed);
    }

    // Picks a new target position within the movement area.
    void PickNewTarget()
    {
        targetChangeTimer = 0f;
        currentTargetChangeInterval = Random.Range(targetChangeIntervalRange.x, targetChangeIntervalRange.y);
        if (movementArea != null)
        {
            Bounds bounds = movementArea.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            targetPosition = new Vector3(x, y, z);
        }
        else
        {
            targetPosition = transform.position + new Vector3(Random.Range(-10f, 10f), Random.Range(-2f, 2f), Random.Range(-10f, 10f));
        }
    }
}
