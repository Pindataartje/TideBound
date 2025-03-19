using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 8f;  // Increased jump force for a snappier jump.
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
    public float crouchSpeedMultiplier = 0.5f;
    public float defaultPlayerScale = 1.5f;
    public float crouchPlayerScale = 0.8f;
    public float scaleSpeed = 8f;

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
    public float hungerDecreaseRate = 1f;
    public float thirstDecreaseRate = 1.5f;
    public float healthDecreaseRate = 2f;

    [Header("Fall Damage Settings")]
    public float fallDamageThreshold = 3f;
    public float fallDamageMultiplier = 10f;

    [Header("Cursor & UI Settings")]
    public GameObject[] importantGameObjects;

    [Header("Slope Sliding Settings")]
    public float steepSlopeMultiplier = 2f;

    [Header("References")]
    public Transform camReference;
    public CapsuleCollider capsuleCollider;
    public LayerMask groundLayer;
    public HorizontalRotation[] horizontalRotationScripts;
    public VerticalRotation[] verticalRotationScripts;

    [Header("Boat Control Settings")]
    public GameObject boatUI;
    public Transform boatExitPoint;

    [Header("Jump Settings")]
    public float fallMultiplier = 2.5f;  // Adjust for faster falling.

    // --- Boat Control Variables ---
    private bool isControllingBoat = false;
    private BoatController currentBoatController = null;
    private Renderer playerRenderer;

    // --- Ship Parenting Variables ---
    private Transform currentShipParent = null;

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

    // Fall damage variables
    private bool wasGrounded = true;
    private float fallStartHeight;

    // Jump direction storage
    private Vector3 jumpDirection = Vector3.zero;

    // Duration during which the cursor is forced to be locked/hidden.
    private const float forceLockDuration = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camReference != null)
        {
            camTransform = camReference;
            camStandLocalPos = camTransform.localPosition;
        }

        playerRenderer = GetComponentInChildren<Renderer>();

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

        horizontalRotationScripts = Object.FindObjectsByType<HorizontalRotation>(FindObjectsSortMode.None);
        verticalRotationScripts = Object.FindObjectsByType<VerticalRotation>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Time.timeSinceLevelLoad < forceLockDuration)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EnableRotationScripts(true);
        }
        else
        {
            HandleCursorLockState();
        }

        // Call step climbing if grounded.
        if (isGrounded)
        {
            HandleStepClimbingAdvanced();
        }

        // Ground and fall damage checks.
        Vector3 rayOrigin = new Vector3(transform.position.x, capsuleCollider.bounds.min.y + 0.05f, transform.position.z);
        RaycastHit hit;
        float rayLength = 0.2f;
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
                if (currentHealth < 0) currentHealth = 0;
            }
        }
        wasGrounded = isGrounded;

        HandleBoatControl();

        if (!isControllingBoat)
        {
            HandleInput();

            if (isCrouching && crouchCameraTarget != null)
                camTransform.localPosition = crouchCameraTarget.localPosition;
            else
            {
                if (!isMoving)
                    camTransform.localPosition = camStandLocalPos;
                else
                    HandleStepBobbing();
            }
        }

        UpdateHungerThirst();
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isSliding)
        {
            HandleSlopeSliding();
        }

        // Apply extra gravity when falling.
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    // Advanced step climbing: detects small steps and nudges the player upward.
    void HandleStepClimbingAdvanced()
    {
        // Only attempt if grounded.
        if (!isGrounded) return;

        // Parameters – adjust these to suit your game:
        float maxStepHeight = 0.5f;      // Maximum height the player can step up.
        float stepCheckDistance = 0.3f;  // How far ahead to check for an obstacle.
        float minStepHeight = 0.1f;      // Ignore very small obstacles.

        // Get capsule collider bounds for a rough step check.
        Vector3 colliderCenter = capsuleCollider.bounds.center;
        float capsuleRadius = capsuleCollider.radius;

        // Use the collider's bounds: bottom and top points.
        Vector3 bottom = new Vector3(colliderCenter.x, capsuleCollider.bounds.min.y, colliderCenter.z);
        Vector3 top = new Vector3(colliderCenter.x, capsuleCollider.bounds.max.y, colliderCenter.z);

        RaycastHit hit;
        if (Physics.CapsuleCast(bottom, top, capsuleRadius, transform.forward, out hit, stepCheckDistance))
        {
            float stepHeight = hit.point.y - capsuleCollider.bounds.min.y;
            if (stepHeight > minStepHeight && stepHeight <= maxStepHeight)
            {
                // Check for clearance above the step.
                Vector3 elevatedBottom = bottom + Vector3.up * maxStepHeight;
                Vector3 elevatedTop = top + Vector3.up * maxStepHeight;
                if (!Physics.CapsuleCast(elevatedBottom, elevatedTop, capsuleRadius, transform.forward, out hit, stepCheckDistance))
                {
                    // Move the player upward by the detected step height.
                    transform.position += Vector3.up * stepHeight;
                }
            }
        }
    }

    void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        isMoving = (moveX != 0 || moveZ != 0);

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
            rb.linearVelocity = new Vector3(rb.linearVelocity.x + airMove.x * Time.deltaTime,
                                            rb.linearVelocity.y,
                                            rb.linearVelocity.z + airMove.z * Time.deltaTime);
        }

        // Improved jumping using impulse force.
        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            jumpDirection = moveDir;
            if (jumpDirection == Vector3.zero)
                jumpDirection = transform.forward;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            if (camTransform != null)
                camTransform.localPosition = camStandLocalPos;
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
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        if (currentHunger <= 0f || currentThirst <= 0f)
        {
            currentHealth -= healthDecreaseRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }
    }

    // --- Boat Control System ---
    // Press E to board or exit a boat.
    void HandleBoatControl()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isControllingBoat)
            {
                Ray ray = new Ray(camReference.position, camReference.forward);
                RaycastHit hit;
                float rayDistance = 5f;
                if (Physics.Raycast(ray, out hit, rayDistance))
                {
                    if (hit.collider.CompareTag("Boat"))
                    {
                        BoatController boatController = hit.collider.GetComponent<BoatController>();
                        if (boatController != null)
                        {
                            boatController.enabled = true;

                            if (boatUI != null)
                            {
                                boatUI.SetActive(true);
                            }

                            rb.isKinematic = true;
                            if (playerRenderer != null)
                                playerRenderer.enabled = false;
                            if (camReference != null)
                                camReference.gameObject.SetActive(false);

                            currentBoatController = boatController;
                            isControllingBoat = true;
                        }
                    }
                }
            }
            else
            {
                if (currentBoatController != null)
                {
                    currentBoatController.enabled = false;

                    if (boatUI != null)
                    {
                        boatUI.SetActive(false);
                    }

                    if (boatExitPoint != null)
                    {
                        transform.position = boatExitPoint.position;
                    }

                    rb.isKinematic = false;
                    if (playerRenderer != null)
                        playerRenderer.enabled = true;
                    if (camReference != null)
                        camReference.gameObject.SetActive(true);
                }
                isControllingBoat = false;
                currentBoatController = null;
            }
        }
    }

    // --- Ship Parenting System Using Trigger Colliders ---
    // When the player enters a trigger collider on an object tagged "Boat", the player is parented to that ship.
    // When exiting, the parent is removed and rotation is reset.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat"))
        {
            transform.SetParent(other.transform);
            currentShipParent = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boat") && currentShipParent == other.transform)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
            currentShipParent = null;
        }
    }

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
