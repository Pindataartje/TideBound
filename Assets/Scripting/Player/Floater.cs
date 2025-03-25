using UnityEngine;

public class Floater : MonoBehaviour
{
    [Header("Water & Oscillation Settings")]
    [Tooltip("Nominal water level (Y position).")]
    public float waterLevel = 6f;
    [Tooltip("Base amplitude (in meters) for vertical oscillation.")]
    public float baseAmplitude = 0.3f;
    [Tooltip("Additional amplitude (in meters) for small random variation.")]
    public float randomAmplitude = 0.1f;
    [Tooltip("Oscillation frequency (cycles per second) for the base wave.")]
    public float baseFrequency = 0.3f;
    [Tooltip("Frequency multiplier for the random variation (via Perlin noise).")]
    public float randomFrequency = 0.5f;

    [Header("Vertical Force Settings")]
    [Tooltip("Multiplier for the vertical buoyancy force.")]
    public float verticalForceMultiplier = 10f;
    [Tooltip("Damping factor for vertical motion.")]
    public float verticalDamping = 2f;
    [Tooltip("Extra downward force applied when the boat is above water.")]
    public float extraGravityForce = 9.81f;

    [Header("Rotation Adjustment (Optional)")]
    [Tooltip("If true, the boat will gently pitch and roll to simulate wave motion.")]
    public bool adjustRotation = true;
    [Tooltip("Maximum pitch angle (in degrees).")]
    public float maxPitch = 2f;
    [Tooltip("Maximum roll angle (in degrees).")]
    public float maxRoll = 2f;
    [Tooltip("How quickly the boat rotates toward the target rotation.")]
    public float rotationDamping = 2f;

    [Header("Rigidbody Options")]
    [Tooltip("If true, this script will disable Rigidbody freezeRotation to allow rotation adjustments. " +
             "If false, existing constraints will be preserved.")]
    public bool disableRotationConstraints = false;

    [Header("References")]
    [Tooltip("Rigidbody on the boat.")]
    public Rigidbody rb;

    // Internal state for oscillation.
    private float phaseOffset;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Only disable rotation constraints if disableRotationConstraints is true.
        if (disableRotationConstraints)
        {
            rb.freezeRotation = false;
        }

        // Pick a random phase offset so that multiple boats don’t oscillate in lockstep.
        phaseOffset = Random.Range(0f, 2 * Mathf.PI);
    }

    void FixedUpdate()
    {
        float t = Time.time;

        // Calculate a smooth vertical oscillation offset.
        float baseOffset = baseAmplitude * Mathf.Sin(2 * Mathf.PI * baseFrequency * t + phaseOffset);
        float noise = (Mathf.PerlinNoise(t * randomFrequency, 0f) - 0.5f) * 2f * randomAmplitude;
        float waveOffset = baseOffset + noise;

        // The desired vertical target is waterLevel plus the wave offset.
        float desiredY = waterLevel + waveOffset;

        // Compute vertical error and current vertical velocity.
        float error = desiredY - transform.position.y;
        float verticalVelocity = rb.linearVelocity.y;

        // Compute the corrective buoyancy force.
        float force = verticalForceMultiplier * error - verticalDamping * verticalVelocity;

        // If the boat is above water (or very near), apply extra downward force.
        if (transform.position.y > waterLevel)
        {
            force -= extraGravityForce;
        }

        // Apply the force along Y.
        rb.AddForce(new Vector3(0f, force, 0f), ForceMode.Acceleration);

        // Optional: Apply slight rotation adjustments to simulate the boat tilting with the waves.
        if (adjustRotation)
        {
            // Use similar oscillation to determine target pitch and roll.
            float targetPitch = maxPitch * Mathf.Sin(2 * Mathf.PI * baseFrequency * t + phaseOffset);
            float targetRoll = maxRoll * Mathf.Sin(2 * Mathf.PI * baseFrequency * t + phaseOffset + Mathf.PI / 2);

            // Build a target rotation keeping the current yaw.
            Quaternion currentRot = transform.rotation;
            Vector3 currentEuler = currentRot.eulerAngles;
            Quaternion targetRot = Quaternion.Euler(targetPitch, currentEuler.y, targetRoll);

            // Smoothly interpolate toward the target rotation.
            transform.rotation = Quaternion.Lerp(currentRot, targetRot, rotationDamping * Time.fixedDeltaTime);
        }
    }
}
