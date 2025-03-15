using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float stamina = 5f;
    public float staminaRecoveryRate = 1f;
    public float raycastDistance = 0.1f; // For obstacle detection
    public LayerMask obstacleLayer; // Obstacles layer mask
    public Transform[] raycastOrigins; // For obstacle detection

    [Header("Interaction Settings")]
    public float interactionRange = 5f;
    public string interactableTag = "Controller";
    public CinemachineVirtualCamera playerCameraVirtual;
    public CinemachineVirtualCamera boatCamera;
    public MonoBehaviour boatController;
    public GameObject playerController;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    private Rigidbody rb;
    private bool isGrounded = false;
    private bool isSprinting = false;
    private bool inBoat = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        // Disable boat controller initially
        boatController.enabled = false;
        playerController.SetActive(true);
    }

    private void Update()
    {
        if (!inBoat)
        {
            HandleMovement();
            HandleInteraction();
        }

        isGrounded = CheckGrounded();
    }

    private void HandleMovement()
    {
        // Input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Sprinting logic
        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float speed = isSprinting ? sprintSpeed : moveSpeed;

        if (isSprinting)
        {
            stamina -= Time.deltaTime;
        }
        else if (stamina < 5f)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
        }

        // Apply movement
        if (moveDirection.magnitude > 0 && CanMove(moveDirection))
        {
            Vector3 velocity = moveDirection * speed;
            velocity.y = rb.velocity.y; // Preserve vertical velocity

            // Debugging for NaN or infinite values
            if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z))
            {
                Debug.LogError("NaN detected in velocity!");
            }
            else if (float.IsInfinity(velocity.x) || float.IsInfinity(velocity.y) || float.IsInfinity(velocity.z))
            {
                Debug.LogError("Infinity detected in velocity!");
            }
            else
            {
                rb.velocity = transform.TransformDirection(velocity);
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }


    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.CompareTag(interactableTag))
                {
                    ToggleControlMode();
                }
            }
        }
    }

    private void ToggleControlMode()
    {
        inBoat = !inBoat;

        if (inBoat)
        {
            playerCameraVirtual.Priority = 0;
            boatCamera.Priority = 10;
            playerController.SetActive(false);
            boatController.enabled = true;
        }
        else
        {
            playerCameraVirtual.Priority = 10;
            boatCamera.Priority = 0;
            playerController.SetActive(true);
            boatController.enabled = false;
        }
    }

    private bool CanMove(Vector3 direction)
    {
        foreach (Transform origin in raycastOrigins)
        {
            Vector3 rayDirection = transform.TransformDirection(direction);
            if (Physics.Raycast(origin.position, rayDirection, raycastDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // Reset vertical velocity when grounded
            return true;
        }
        return false;
    }
    private void FixedUpdate()
    {
        CheckRigidbodyState();
    }

    private void CheckRigidbodyState()
    {
        if (float.IsNaN(rb.velocity.x) || float.IsNaN(rb.velocity.y) || float.IsNaN(rb.velocity.z) ||
            float.IsInfinity(rb.velocity.x) || float.IsInfinity(rb.velocity.y) || float.IsInfinity(rb.velocity.z))
        {
            Debug.LogWarning("Invalid Rigidbody state detected! Resetting Rigidbody.");
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


}
