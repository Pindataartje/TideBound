using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[ScriptTag("Item")]
public class FishingMinigame : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's camera (for raycasting water).")]
    public Transform playerCamera;
    [Tooltip("Reference to the InventoryItemAdder script.")]
    public InventoryItemAdder inventoryItemAdder;
    [Tooltip("Reference to the fishing rod GameObject (used to check if it’s equipped).")]
    public GameObject fishingRod;

    [Header("Water Detection")]
    [Tooltip("Tag used to identify water objects.")]
    public string waterTag = "Water";
    [Tooltip("Distance for the raycast to check for water.")]
    public float raycastDistance = 100f;

    [Header("Easy Settings")]
    public GameObject easySliderGO;
    public float easyDriftSpeed = 1f;
    public float easyRequiredHoldTime = 10f;
    public float easyMinThreshold = 5f;
    public float easyMaxThreshold = 95f;
    public float easyDriftChangeInterval = 1f;

    [Header("Normal Settings")]
    public GameObject normalSliderGO;
    public float normalDriftSpeed = 2f;
    public float normalRequiredHoldTime = 15f;
    public float normalMinThreshold = 15f;
    public float normalMaxThreshold = 85f;
    public float normalDriftChangeInterval = 1f;

    [Header("Hard Settings")]
    public GameObject hardSliderGO;
    public float hardDriftSpeed = 3f;
    public float hardRequiredHoldTime = 20f;
    public float hardMinThreshold = 25f;
    public float hardMaxThreshold = 75f;
    public float hardDriftChangeInterval = 1f;

    [Header("UI Feedback")]
    [Tooltip("Text displayed when fishing is active.")]
    public TextMeshProUGUI fishingStatusText;

    // Internal references for slider and difficulty settings.
    private Slider currentSlider;
    private GameObject currentSliderGO;
    private float currentBaseDriftSpeed;
    private float requiredHoldTime;
    private float minThreshold;
    private float maxThreshold;
    private float currentDriftChangeInterval;
    private float currentDriftSpeed = 0f;

    // Minigame state flags.
    private bool minigameActive = false;
    private float holdTimer = 0f;
    private bool fishingAttemptStarted = false;

    // Player movement reference.
    private Movement playerMovement;
    private float defaultWalkSpeed = 5f;

    void Start()
    {
        // Automatically assign playerCamera if not set.
        if (playerCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                playerCamera = mainCam.transform;
            else
                Debug.LogWarning("Main Camera not found. Please assign playerCamera manually.");
        }

        // Automatically assign inventoryItemAdder if not set.
        if (inventoryItemAdder == null)
        {
            inventoryItemAdder = FindObjectOfType<InventoryItemAdder>();
            if (inventoryItemAdder == null)
                Debug.LogWarning("InventoryItemAdder not found in the scene. Please assign it manually.");
        }

        // Automatically assign playerMovement by finding the GameObject tagged "Player".
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<Movement>();
            if (playerMovement != null)
                defaultWalkSpeed = playerMovement.walkSpeed;
            else
                Debug.LogWarning("Movement component not found on the player.");
        }
        else
        {
            Debug.LogWarning("Player GameObject not found. Please tag your player as 'Player'.");
        }
    }

    void Update()
    {
        // Cancel fishing if minigame is active and RMB is pressed.
        if (minigameActive && Input.GetMouseButtonDown(1))
        {
            Debug.Log("Fishing cancelled by player.");
            EndMinigame(false);
            return;
        }

        // If not active and not already attempting, listen for fishing attempt.
        if (!minigameActive && !fishingAttemptStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    if (hit.collider != null && hit.collider.CompareTag(waterTag))
                    {
                        Debug.Log("Water detected! Starting fishing attempt.");
                        fishingAttemptStarted = true;
                        if (fishingStatusText != null)
                        {
                            fishingStatusText.gameObject.SetActive(true);
                            fishingStatusText.text = "Waiting for fish...";
                        }
                        if (playerMovement != null)
                            playerMovement.walkSpeed = 0f; // Disable movement.
                        StartCoroutine(StartFishingAfterDelay());
                    }
                    else
                    {
                        Debug.Log("Hit object is not water.");
                    }
                }
            }
        }

        // If the minigame is active, update the slider and UI feedback.
        if (minigameActive)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
                currentSlider.value -= 5f;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow))
                currentSlider.value += 5f;

            currentSlider.value += currentDriftSpeed * Time.deltaTime;

            if (fishingStatusText != null)
            {
                fishingStatusText.text = $"Fishing... ({holdTimer:F1}/{requiredHoldTime:F1} sec)";
            }

            if (currentSlider.value <= minThreshold || currentSlider.value >= maxThreshold)
            {
                Debug.Log("Fishing minigame failed: slider out of bounds.");
                EndMinigame(false);
            }
            else
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= requiredHoldTime)
                {
                    Debug.Log("Fishing minigame succeeded!");
                    inventoryItemAdder.AddItemByTag("Fish1", 1);
                    EndMinigame(true);
                }
            }
        }
    }

    IEnumerator StartFishingAfterDelay()
    {
        float delay = Random.Range(10f, 30f);
        yield return new WaitForSeconds(delay);
        StartMinigame();
    }

    void StartMinigame()
    {
        // Randomly select a difficulty: 0 = Easy, 1 = Normal, 2 = Hard.
        int diff = Random.Range(0, 3);
        switch (diff)
        {
            case 0:
                currentSliderGO = easySliderGO;
                currentBaseDriftSpeed = easyDriftSpeed;
                requiredHoldTime = easyRequiredHoldTime;
                minThreshold = easyMinThreshold;
                maxThreshold = easyMaxThreshold;
                currentDriftChangeInterval = easyDriftChangeInterval;
                break;
            case 1:
                currentSliderGO = normalSliderGO;
                currentBaseDriftSpeed = normalDriftSpeed;
                requiredHoldTime = normalRequiredHoldTime;
                minThreshold = normalMinThreshold;
                maxThreshold = normalMaxThreshold;
                currentDriftChangeInterval = normalDriftChangeInterval;
                break;
            case 2:
                currentSliderGO = hardSliderGO;
                currentBaseDriftSpeed = hardDriftSpeed;
                requiredHoldTime = hardRequiredHoldTime;
                minThreshold = hardMinThreshold;
                maxThreshold = hardMaxThreshold;
                currentDriftChangeInterval = hardDriftChangeInterval;
                break;
        }

        // Activate slider UI and configure its range.
        currentSliderGO.SetActive(true);
        currentSlider = currentSliderGO.GetComponent<Slider>();
        if (currentSlider == null)
        {
            Debug.LogError("Missing Slider component on the chosen slider GameObject.");
            return;
        }
        currentSlider.minValue = minThreshold;
        currentSlider.maxValue = maxThreshold;
        currentSlider.value = (minThreshold + maxThreshold) / 2f; // Center the slider.
        holdTimer = 0f;
        minigameActive = true;

        // Update fishing status text to "Fishing..."
        if (fishingStatusText != null)
        {
            fishingStatusText.gameObject.SetActive(true);
            fishingStatusText.text = "Fishing...";
        }

        // Start the coroutine to update drift.
        StartCoroutine(UpdateDrift());
    }

    IEnumerator UpdateDrift()
    {
        while (minigameActive)
        {
            currentDriftSpeed = Random.Range(-currentBaseDriftSpeed, currentBaseDriftSpeed);
            yield return new WaitForSeconds(currentDriftChangeInterval);
        }
    }

    void EndMinigame(bool success)
    {
        easySliderGO.SetActive(false);
        normalSliderGO.SetActive(false);
        hardSliderGO.SetActive(false);
        if (fishingStatusText != null)
            fishingStatusText.gameObject.SetActive(false);

        minigameActive = false;
        fishingAttemptStarted = false;
        holdTimer = 0f;

        // Restore player's walk speed.
        if (playerMovement != null)
            playerMovement.walkSpeed = defaultWalkSpeed;
    }
}
