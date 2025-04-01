using UnityEngine;
using UnityEngine.UI;

public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    // Movement parameters
    public float maxEnginePower = 2000f;
    public float maxSpeed = 20f;
    public float reverseSpeed = 5f;
    [Tooltip("Base turning speed in degrees per second.")]
    public float turnSpeed = 5f;
    [Tooltip("Base drag multiplier for horizontal velocity.")]
    public float drag = 0.99f;
    [Tooltip("How quickly throttle changes (per second).")]
    public float throttleChangeRate = 0.5f; // How quickly throttle changes

    [Header("Turning Settings")]
    [Tooltip("Multiplier for turning torque. Lower values mean less responsive turning.")]
    public float turnTorqueMultiplier = 0.5f;
    [Tooltip("How quickly the boat’s turning corrects (damping). Higher values mean smoother, slower rotation.")]
    public float turnDamping = 2f;

    [Header("Random Drag Variation")]
    [Tooltip("Minimum drag multiplier value (e.g. 0.95).")]
    public float minDragVariation = 0.95f;
    [Tooltip("Maximum drag multiplier value (e.g. 1.0).")]
    public float maxDragVariation = 1.0f;
    [Tooltip("Frequency of drag variation changes.")]
    public float dragVariationFrequency = 0.5f;

    [Header("UI Elements")]
    public Slider throttleSlider;
    public Slider speedSlider;

    private float throttle = 0f; // Current throttle value (-1 to 1)
    private float speed = 0f;
    private float turnInput = 0f; // Turn input

    [Header("Lights")]
    [Tooltip("Empty GameObject that holds all the lights as children.")]
    public GameObject lightsContainer;

    private bool lightsOn = false;


    // New method to reset gas/throttle when exiting.
    public void ResetGas()
    {
        throttle = 0f;
    }
    private void Start()
    {
        // Initialize sliders
        if (throttleSlider != null)
        {
            throttleSlider.minValue = -1f;
            throttleSlider.maxValue = 1f;
        }
        if (speedSlider != null)
        {
            speedSlider.minValue = 0f;
            speedSlider.maxValue = maxSpeed;
        }
    }

    private void FixedUpdate()
    {
        // Update throttle based on input.
        HandleThrottle();

        // Get turn input.
        turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys

        // Move the boat horizontally.
        HandleMovement();

        // Smoothly handle turning.
        HandleTurning();

        // Apply drag with a slight random variation.
        ApplyDrag();

        // Update UI.
        UpdateSliders();
    }

    private void HandleThrottle()
    {
        if (Input.GetKey(KeyCode.W))
        {
            throttle += throttleChangeRate * Time.fixedDeltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            throttle -= throttleChangeRate * Time.fixedDeltaTime;
        }
        // Clamp throttle to range [-1, 1]
        throttle = Mathf.Clamp(throttle, -1f, 1f);
    }

    private void HandleMovement()
    {
        float enginePower = throttle * maxEnginePower;

        // Get current horizontal velocity (only X and Z) and preserve Y.
        Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (throttle > 0)
        {
            // Forward motion
            if (currentHorizontalVel.magnitude < maxSpeed)
            {
                Vector3 horizontalForce = transform.forward * enginePower * Time.fixedDeltaTime;
                horizontalForce.y = 0f;
                rb.AddForce(horizontalForce, ForceMode.Acceleration);
            }
        }
        else if (throttle < 0)
        {
            // Reverse motion
            if (currentHorizontalVel.magnitude < reverseSpeed)
            {
                Vector3 horizontalForce = transform.forward * enginePower * Time.fixedDeltaTime;
                horizontalForce.y = 0f;
                rb.AddForce(horizontalForce, ForceMode.Acceleration);
            }
        }

        speed = currentHorizontalVel.magnitude;
    }

    private void HandleTurning()
    {
        // Only allow turning if the boat is moving horizontally.
        Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (currentHorizontalVel.magnitude > 0.1f)
        {
            // Scale turning with forward speed.
            float speedFactor = Mathf.Clamp01(currentHorizontalVel.magnitude / maxSpeed);
            // Desired angular velocity around Y axis.
            float desiredAngularY = turnInput * turnSpeed * turnTorqueMultiplier * speedFactor;
            float currentAngularY = rb.angularVelocity.y;
            float torqueY = turnDamping * (desiredAngularY - currentAngularY);
            rb.AddTorque(Vector3.up * torqueY * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    private void ApplyDrag()
    {
        // Compute a random drag multiplier based on Perlin noise.
        float noise = Mathf.PerlinNoise(Time.time * dragVariationFrequency, 0f);
        float randomDrag = Mathf.Lerp(minDragVariation, maxDragVariation, noise);

        // Apply drag only to the horizontal velocity.
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        horizontalVel *= randomDrag;
        rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);

        // Apply drag to angular velocity as well.
        rb.angularVelocity *= randomDrag;
    }

    private void UpdateSliders()
    {
        if (throttleSlider != null)
            throttleSlider.value = throttle;
        if (speedSlider != null)
            speedSlider.value = speed;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLights();
        }
    }

    private void ToggleLights()
    {
        if (lightsContainer == null)
        {
            Debug.LogWarning("Lights container is not assigned.");
            return;
        }

        // Toggle the activation state of the lightsContainer
        lightsOn = !lightsOn;
        lightsContainer.SetActive(lightsOn);

        // Log to confirm the toggling
        Debug.Log("Lights toggled. Lights on: " + lightsOn);
    }




}
