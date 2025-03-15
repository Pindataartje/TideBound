using UnityEngine;
using UnityEngine.UI;

public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    // Movement parameters
    public float maxEnginePower = 2000f;
    public float maxSpeed = 20f;
    public float reverseSpeed = 5f;
    public float turnSpeed = 5f;
    public float drag = 0.99f;
    public float throttleChangeRate = 0.5f; // How quickly throttle changes

    [Header("UI Elements")]
    public Slider throttleSlider;
    public Slider speedSlider;

    private float throttle = 0f; // Current throttle value (-1 to 1)
    private float speed = 0f;
    private float turnInput = 0f; // Turn input

    private void Start()
    {
        // Initialize sliders
        throttleSlider.minValue = -1f;
        throttleSlider.maxValue = 1f;

        speedSlider.minValue = 0f;
        speedSlider.maxValue = maxSpeed;
    }

    private void FixedUpdate()
    {
        // Update throttle based on input
        HandleThrottle();

        // Get turn input
        turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys

        // Move the boat
        HandleMovement();

        // Turn the boat
        HandleTurning();

        // Apply drag
        ApplyDrag();

        // Update UI
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

        if (throttle > 0)
        {
            // Forward motion
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(transform.forward * enginePower * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
        else if (throttle < 0)
        {
            // Reverse motion
            if (rb.velocity.magnitude < reverseSpeed)
            {
                rb.AddForce(transform.forward * enginePower * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }

        speed = rb.velocity.magnitude;
    }

    private void HandleTurning()
    {
        if (rb.velocity.magnitude > 0.1f) // Allow turning only if the boat is moving
        {
            float turn = turnInput * turnSpeed;
            rb.AddTorque(Vector3.up * turn * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    private void ApplyDrag()
    {
        rb.velocity *= drag;
        rb.angularVelocity *= drag;
    }

    private void UpdateSliders()
    {
        throttleSlider.value = throttle;
        speedSlider.value = speed;
    }
}
