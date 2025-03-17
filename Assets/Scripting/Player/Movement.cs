using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float airControlMultiplier = 0.3f;
    public float slopeAcceleration = 3f;
    public float maxSlopeAngle = 45f;

    [Header("Sprinting Settings")]
    public float sprintMultiplier = 1.5f;
    public float staminaConsumptionRate = 20f;
    public float staminaRecoveryRate = 10f;
    public float sprintCooldownThreshold = 20f;
    public float jumpStaminaCost = 15f;

    [Header("Step Bobbing Settings")]
    public float stepBobbingSpeed = 10f;
    public float stepBobbingAmount = 0.1f;
    public bool IsGrounded { get { return isGrounded; } }

    [Header("Crouch Settings")]
    public Transform crouchCameraTarget;
    public float crouchSpeedMultiplier = 0.5f; // Slow movement while crouching
    public float defaultPlayerScale = 1.5f;      // Default player scale
    public float crouchPlayerScale = 0.8f;       // Scale when crouching
    public float scaleSpeed = 8f;                // How fast scaling transitions

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public Slider healthSlider;
    public Slider staminaSlider;

    [Header("Hunger/Thirst Settings")]
    public float maxHunger = 100f;
    public float currentHunger = 100f;
    public float maxThirst = 100f;
    public float currentThirst = 100f;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public float hungerDecreaseRate = 1f;   // per second
    public float thirstDecreaseRate = 1.5f;   // per second
    public float healthDecreaseRate = 2f;     // health lost per second when starving or dehydrated

    [Header("Fall Damage Settings")]
    public float fallDamageThreshold = 3f;
    public float fallDamageMultiplier = 10f; // Damage per unit of fall distance beyond threshold

    [Header("Cursor & UI Settings")]
    // Drag any GameObjects here that should unlock the cursor (and disable rotation) when active.
    public GameObject[] importantGameObjects;

    [Header("Slope Sliding Settings")]
    // This multiplier scales the sliding force on steeper slopes.
    public float steepSlopeMultiplier = 2f;

    [Header("References")]
    // Instead of a CinemachineVirtualCamera, we use a regular Transform reference for the camera.
    public Transform camReference;
    public CapsuleCollider capsuleCollider;
    public LayerMask groundLayer;

    // Rotation scripts will be located by type.
    public HorizontalRotation[] horizontalRotationScripts;
    public VerticalRotation[] verticalRotationScripts;

    private Rigidbody rb;
    private Transform camTransform;
    private Vector3 camStandLocalPos;
    private bool isCrouching = false;
    private float stepTimer = 0f;
    private bool isGrounded = false;
    private bool isSliding = false;
    private bool canSprint = true;
    private bool isMoving = false;
    private Vector3 slopeNormal;

    // Variables for fall damage
    private bool wasGrounded = true;
    private float fallStartHeight;

    // Variable to store jump direction
    private Vector3 jumpDirection = Vector3.zero;

    // Duration during which the cursor is forced to be locked/hidden.
    private const float forceLockDuration = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Force lock and hide the cursor at start.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set up the camera reference from the provided transform.
        if (camReference != null)
        {
            camTransform = camReference;
            camStandLocalPos = camTransform.localPosition;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
        }
        if (thirstSlider != null)
        {
            thirstSlider.maxValue = maxThirst;
            thirstSlider.value = currentThirst;
        }

        // Find the rotation scripts by type using the updated method.
        horizontalRotationScripts = Object.FindObjectsByType<HorizontalRotation>(FindObjectsSortMode.None);
        verticalRotationScripts = Object.FindObjectsByType<VerticalRotation>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // For the first few seconds, force the cursor to be locked/hidden.
        if (Time.timeSinceLevelLoad < forceLockDuration)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EnableRotationScripts(true);
        }
        else
        {
            // Handle cursor state and rotation script enabling/disabling based on UI.
            HandleCursorLockState();
        }

        // --- Ground & Fall Damage Check ---
        Vector3 rayOrigin = new Vector3(transform.position.x, capsuleCollider.bounds.min.y + 0.05f, transform.position.z);
        RaycastHit hit;
        float rayLength = 0.2f; // A short distance from the base.
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer))
        {
            isGrounded = true;
            slopeNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, slopeNormal);
            isSliding = angle > maxSlopeAngle;
        }
        else
        {
            isGrounded = false;
            isSliding = false;
        }

        // --- Fall Damage ---
        if (wasGrounded && !isGrounded)
        {
            fallStartHeight = transform.position.y;
        }
        if (!wasGrounded && isGrounded)
        {
            float fallDistance = fallStartHeight - transform.position.y;
            if (fallDistance > fallDamageThreshold)
            {
                float damage = (fallDistance - fallDamageThreshold) * fallDamageMultiplier;
                currentHealth -= damage;
                Debug.Log("Fall damage taken: " + damage);
                if (currentHealth < 0) currentHealth = 0;
            }
        }
        wasGrounded = isGrounded;

        // --- Input & Movement ---
        HandleInput();

        if (!isCrouching)
            HandleStepBobbing();
        else if (crouchCameraTarget != null)
            camTransform.localPosition = crouchCameraTarget.localPosition;

        // Update hunger/thirst and then the UI.
        UpdateHungerThirst();
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isSliding)
        {
            HandleSlopeSliding();
        }
    }

    void HandleInput()
    {
        // Use GetAxisRaw for immediate input response.
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        isMoving = (moveX != 0 || moveZ != 0);

        // Calculate desired move direction based on input.
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && moveDir.magnitude > 0.1f && canSprint;

        if (isSprinting)
        {
            if (currentStamina > 0)
            {
                currentSpeed *= sprintMultiplier;
                currentStamina -= staminaConsumptionRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    canSprint = false;
                }
            }
        }
        else
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            if (currentStamina >= sprintCooldownThreshold) canSprint = true;
        }

        if (isGrounded)
        {
            jumpDirection = Vector3.zero;
            Vector3 moveVector = AdjustVelocityForSlope(moveDir * currentSpeed);
            rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);
        }
        else
        {
            if (jumpDirection != Vector3.zero)
            {
                float dot = Vector3.Dot(jumpDirection, moveDir);
                if (dot < 0.1f)
                {
                    moveDir = Vector3.zero;
                }
            }
            Vector3 airMove = moveDir * (currentSpeed * airControlMultiplier);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x + airMove.x * Time.deltaTime, rb.linearVelocity.y, rb.linearVelocity.z + airMove.z * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            jumpDirection = moveDir;
            if (jumpDirection == Vector3.zero)
                jumpDirection = transform.forward;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            currentStamina -= jumpStaminaCost;
        }

        if (Input.GetKey(KeyCode.LeftControl))
            Crouch();
        else
            StandUp();
    }

    Vector3 AdjustVelocityForSlope(Vector3 moveDir)
    {
        if (Vector3.Angle(Vector3.up, slopeNormal) <= maxSlopeAngle)
        {
            return Vector3.ProjectOnPlane(moveDir, slopeNormal);
        }
        return moveDir;
    }

    void HandleSlopeSliding()
    {
        float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;
        float slideMultiplier = Mathf.Lerp(1f, steepSlopeMultiplier, Mathf.InverseLerp(maxSlopeAngle, 90f, slopeAngle));
        rb.linearVelocity += slideDirection * slopeAcceleration * slideMultiplier * Time.deltaTime;
    }

    void HandleStepBobbing()
    {
        if (!isGrounded || !isMoving) return;

        stepTimer += Time.deltaTime * stepBobbingSpeed;
        float bobbingOffset = Mathf.Sin(stepTimer) * stepBobbingAmount;
        camTransform.localPosition = camStandLocalPos + new Vector3(0, bobbingOffset, 0);
    }

    void Crouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            StartCoroutine(ScalePlayer(crouchPlayerScale));
        }
    }

    void StandUp()
    {
        if (isCrouching)
        {
            isCrouching = false;
            StartCoroutine(ScalePlayer(defaultPlayerScale));
        }
    }

    System.Collections.IEnumerator ScalePlayer(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, targetScale, startScale.z);
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(startScale, endScale, time);
            yield return null;
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (staminaSlider != null) staminaSlider.value = currentStamina;
        if (hungerSlider != null) hungerSlider.value = currentHunger;
        if (thirstSlider != null) thirstSlider.value = currentThirst;
    }

    void UpdateHungerThirst()
    {
        // Decrease hunger and thirst over time.
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        // If hunger or thirst are depleted, slowly reduce health.
        if (currentHunger <= 0f || currentThirst <= 0f)
        {
            currentHealth -= healthDecreaseRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }
    }

    // Checks the important gameobjects and updates the cursor and rotation scripts accordingly.
    void HandleCursorLockState()
    {
        bool anyUIActive = false;
        if (importantGameObjects != null)
        {
            foreach (GameObject go in importantGameObjects)
            {
                if (go != null && go.activeInHierarchy)
                {
                    anyUIActive = true;
                    break;
                }
            }
        }

        if (anyUIActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EnableRotationScripts(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EnableRotationScripts(true);
        }
    }

    // Enables or disables all rotation scripts found in the scene.
    void EnableRotationScripts(bool enable)
    {
        if (horizontalRotationScripts != null)
        {
            foreach (HorizontalRotation hr in horizontalRotationScripts)
            {
                if (hr != null)
                    hr.enabled = enable;
            }
        }
        if (verticalRotationScripts != null)
        {
            foreach (VerticalRotation vr in verticalRotationScripts)
            {
                if (vr != null)
                    vr.enabled = enable;
            }
        }
    }
}
