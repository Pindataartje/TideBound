using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public LayerMask groundLayer;  // Used for ground detection.
    public HorizontalRotation[] horizontalRotationScripts;
    public VerticalRotation[] verticalRotationScripts;

    [Header("Boat Control Settings")]
    public GameObject boatUI;
    public Transform boatExitPoint;

    [Header("Jump Settings")]
    public float fallMultiplier = 2.5f;  // Adjust for faster falling.

    // --- Private Variables ---
    private Rigidbody rb;
    private Transform camTransform;
    private Vector3 camStandLocalPos;
    private bool isCrouching = false;
    private float stepTimer = 0f;
    private bool isGrounded = false;
    public bool IsGrounded { get { return isGrounded; } }

    private bool isSliding = false;
    private bool canSprint = true;
    private bool isMoving = false;
    private Vector3 slopeNormal = Vector3.up;

    // Fall damage variables
    private bool wasGrounded = false;
    private float fallStartHeight = 0f;

    // Jump direction storage
    private Vector3 jumpDirection = Vector3.zero;

    // Duration during which the cursor is forced to be locked/hidden.
    private const float forceLockDuration = 2f;

    // --- Boat Control Variables ---
    private bool isControllingBoat = false;
    private BoatController currentBoatController = null;
    private Renderer playerRenderer;

    // --- Ship Parenting Variables ---
    private Transform currentShipParent = null;

    // --- Throttling Timers ---
    private float physicsTimer = 0f;
    public float physicsUpdateInterval = 0.1f;  // Physics-related updates occur every 0.1 sec.
    private float uiTimer = 0f;
    public float uiUpdateInterval = 0.2f;         // UI updates every 0.2 sec.

    // For caching rotation script state.
    private bool lastRotationState = true;

    // For ground detection using trigger colliders.
    private int groundContactCount = 0;

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

        // Initialize UI sliders.
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

        // Use the new API with FindObjectsSortMode.None for better performance.
        horizontalRotationScripts = Object.FindObjectsByType<HorizontalRotation>(FindObjectsSortMode.None);
        verticalRotationScripts = Object.FindObjectsByType<VerticalRotation>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // Enforce cursor lock for initial duration.
        if (Time.timeSinceLevelLoad < forceLockDuration)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetRotationScripts(true);
        }
        else
        {
            HandleCursorLockState();
        }

        // Handle movement input if not controlling a boat.
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

        HandleBoatControl();

        // Throttled physics updates (slope normal, slope sliding, fall damage).
        physicsTimer += Time.deltaTime;
        if (physicsTimer >= physicsUpdateInterval)
        {
            UpdateSlopeNormal();
            HandleSlopeSliding();
            HandleFallDamage();
            physicsTimer = 0f;
        }

        // Throttled UI and hunger/thirst updates.
        uiTimer += Time.deltaTime;
        if (uiTimer >= uiUpdateInterval)
        {
            UpdateHungerThirst();
            UpdateUI();
            uiTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        // Apply extra gravity when falling.
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // Ground detection using trigger events with layer check (using groundLayer mask).
    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            groundContactCount++;
            isGrounded = true;
        }
        else if (other.CompareTag("Boat"))
        {
            // Ship parenting system.
            transform.SetParent(other.transform);
            currentShipParent = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            groundContactCount = Mathf.Max(groundContactCount - 1, 0);
            if (groundContactCount == 0)
                isGrounded = false;
        }
        else if (other.CompareTag("Boat") && currentShipParent == other.transform)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
            currentShipParent = null;
        }
    }

    // Throttled update for slope normal: a raycast is performed at a lower frequency.
    void UpdateSlopeNormal()
    {
        Vector3 origin = new Vector3(transform.position.x, capsuleCollider.bounds.min.y + 0.1f, transform.position.z);
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f, groundLayer))
        {
            slopeNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, slopeNormal);
            isSliding = angle > maxSlopeAngle;
        }
        else
        {
            slopeNormal = Vector3.up;
            isSliding = false;
        }
    }

    // Handles fall damage by checking transitions between grounded and airborne states.
    void HandleFallDamage()
    {
        if (wasGrounded && !isGrounded)
        {
            fallStartHeight = transform.position.y;
        }
        else if (!wasGrounded && isGrounded)
        {
            float fallDistance = fallStartHeight - transform.position.y;
            if (fallDistance > fallDamageThreshold)
            {
                float damage = (fallDistance - fallDamageThreshold) * fallDamageMultiplier;
                currentHealth -= damage;
                currentHealth = Mathf.Max(currentHealth, 0f);
            }
        }
        wasGrounded = isGrounded;
    }

    // Handles player input for movement, sprinting, jumping, and crouching.
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
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            if (currentStamina >= sprintCooldownThreshold)
                canSprint = true;
        }

        if (isGrounded)
        {
            jumpDirection = Vector3.zero;
            Vector3 adjustedMove = AdjustVelocityForSlope(moveDir * currentSpeed);
            rb.linearVelocity = new Vector3(adjustedMove.x, rb.linearVelocity.y, adjustedMove.z);
        }
        else
        {
            if (jumpDirection != Vector3.zero)
            {
                float dot = Vector3.Dot(jumpDirection, moveDir);
                if (dot < 0.1f)
                    moveDir = Vector3.zero;
            }
            Vector3 airMove = moveDir * (currentSpeed * airControlMultiplier);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x + airMove.x * Time.deltaTime,
                                      rb.linearVelocity.y,
                                      rb.linearVelocity.z + airMove.z * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            jumpDirection = (moveDir != Vector3.zero) ? moveDir : transform.forward;
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
            return Vector3.ProjectOnPlane(moveDir, slopeNormal);
        return moveDir;
    }

    // Applies sliding when the slope is too steep.
    void HandleSlopeSliding()
    {
        if (!isGrounded) return;
        float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
        if (slopeAngle <= maxSlopeAngle) return;
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;
        float slideMultiplier = Mathf.Lerp(1f, steepSlopeMultiplier, Mathf.InverseLerp(maxSlopeAngle, 90f, slopeAngle));
        rb.linearVelocity += slideDirection * slopeAcceleration * slideMultiplier * Time.deltaTime;
    }

    // Simple step bobbing effect.
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

    IEnumerator ScalePlayer(float targetScale)
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
                            if (boatUI != null) boatUI.SetActive(true);
                            rb.isKinematic = true;
                            if (playerRenderer != null) playerRenderer.enabled = false;
                            if (camReference != null) camReference.gameObject.SetActive(false);
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
                    if (boatUI != null) boatUI.SetActive(false);
                    if (boatExitPoint != null) transform.position = boatExitPoint.position;
                    rb.isKinematic = false;
                    if (playerRenderer != null) playerRenderer.enabled = true;
                    if (camReference != null) camReference.gameObject.SetActive(true);
                }
                isControllingBoat = false;
                currentBoatController = null;
            }
        }
    }

    // --- Cursor & Rotation Scripts ---
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
            SetRotationScripts(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetRotationScripts(true);
        }
    }

    void SetRotationScripts(bool enable)
    {
        if (lastRotationState == enable) return;
        if (horizontalRotationScripts != null)
        {
            foreach (HorizontalRotation hr in horizontalRotationScripts)
                if (hr != null) hr.enabled = enable;
        }
        if (verticalRotationScripts != null)
        {
            foreach (VerticalRotation vr in verticalRotationScripts)
                if (vr != null) vr.enabled = enable;
        }
        lastRotationState = enable;
    }
}
