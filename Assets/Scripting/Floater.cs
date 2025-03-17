using UnityEngine;

public class Floater : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [Tooltip("The target Y level around which the object should float.")]
    public float targetY = 6f;

    [Tooltip("Maximum deviation from the target (oscillation amplitude).")]
    public float amplitude = 0.5f;

    [Tooltip("Oscillation frequency in Hertz (cycles per second).")]
    public float frequency = 1f;

    [Tooltip("Multiplier for the buoyancy force (proportional gain).")]
    public float forceMultiplier = 10f;

    [Tooltip("Damping factor to reduce overshoot.")]
    public float damping = 2f;

    // The Rigidbody to affect.
    public Rigidbody rb;

    // A random offset so that multiple objects don't all oscillate identically.
    private float phaseOffset;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        // Random phase offset for variation.
        phaseOffset = Random.Range(0f, 2 * Mathf.PI);
    }

    void FixedUpdate()
    {
        // Calculate the desired vertical position using a sine wave oscillation.
        float desiredY = targetY + amplitude * Mathf.Sin((Time.time * frequency * 2 * Mathf.PI) + phaseOffset);

        // Calculate the difference between the desired position and the current position.
        float error = desiredY - transform.position.y;

        // Retrieve the current vertical velocity using linearVelocity.
        float verticalVelocity = rb.linearVelocity.y;

        // Calculate a force using a simple PD controller: force = Kp * error - Kd * verticalVelocity.
        float buoyancyForce = forceMultiplier * error - damping * verticalVelocity;

        // Apply the force along the Y axis.
        rb.AddForce(new Vector3(0f, buoyancyForce, 0f), ForceMode.Acceleration);
    }
}