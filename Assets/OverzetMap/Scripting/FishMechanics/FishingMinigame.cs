using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    // Internal references
    private Slider currentSlider;
    private GameObject currentSliderGO;

    // Difficulty parameters (set when a difficulty is chosen)
    private float currentBaseDriftSpeed;    // Base drift speed for the chosen difficulty
    private float requiredHoldTime;         // How long the slider must remain in range
    private float minThreshold;             // Lower bound of safe slider value
    private float maxThreshold;             // Upper bound of safe slider value
    private float currentDriftChangeInterval; // How often the drift value is re-randomized

    // This is the drift value that changes periodically.
    private float currentDriftSpeed = 0f;

    // Minigame state flags
    private bool minigameActive = false;
    private float holdTimer = 0f;
    private bool fishingAttemptStarted = false;

    void Update()
    {
        // If the minigame is not active or starting, listen for a fishing attempt.
        if (!minigameActive && !fishingAttemptStarted)
        {
            // When the fishing rod is equipped and the player left-clicks...
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast from the camera.
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    // Only start if we hit an object tagged as water.
                    if (hit.collider != null && hit.collider.CompareTag(waterTag))
                    {
                        Debug.Log("Water detected! Starting fishing attempt.");
                        fishingAttemptStarted = true;
                        StartCoroutine(StartFishingAfterDelay());
                    }
                    else
                    {
                        Debug.Log("Hit object is not water.");
                    }
                }
            }
        }

        // When the minigame is active, update the slider.
        if (minigameActive)
        {
            // Process player input: Q/LeftArrow decreases slider value; E/RightArrow increases it.
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentSlider.value -= 5f;
            }
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentSlider.value += 5f;
            }

            // Apply the current drift to the slider.
            currentSlider.value += currentDriftSpeed * Time.deltaTime;

            // Check if the slider value is outside the safe bounds.
            if (currentSlider.value <= minThreshold || currentSlider.value >= maxThreshold)
            {
                Debug.Log("Fishing minigame failed: slider out of bounds.");
                EndMinigame(false);
            }
            else
            {
                // Increase the hold timer if within safe bounds.
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
        // Wait for a random delay between 10 and 30 seconds.
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
            case 0: // Easy
                currentSliderGO = easySliderGO;
                currentBaseDriftSpeed = easyDriftSpeed;
                requiredHoldTime = easyRequiredHoldTime;
                minThreshold = easyMinThreshold;
                maxThreshold = easyMaxThreshold;
                currentDriftChangeInterval = easyDriftChangeInterval;
                break;
            case 1: // Normal
                currentSliderGO = normalSliderGO;
                currentBaseDriftSpeed = normalDriftSpeed;
                requiredHoldTime = normalRequiredHoldTime;
                minThreshold = normalMinThreshold;
                maxThreshold = normalMaxThreshold;
                currentDriftChangeInterval = normalDriftChangeInterval;
                break;
            case 2: // Hard
                currentSliderGO = hardSliderGO;
                currentBaseDriftSpeed = hardDriftSpeed;
                requiredHoldTime = hardRequiredHoldTime;
                minThreshold = hardMinThreshold;
                maxThreshold = hardMaxThreshold;
                currentDriftChangeInterval = hardDriftChangeInterval;
                break;
        }

        // Activate the chosen slider UI.
        currentSliderGO.SetActive(true);
        currentSlider = currentSliderGO.GetComponent<Slider>();
        if (currentSlider == null)
        {
            Debug.LogError("Missing Slider component on the chosen slider GameObject.");
            return;
        }
        currentSlider.value = 50f; // Center the slider.
        holdTimer = 0f;
        minigameActive = true;

        // Start the coroutine that updates the drift value periodically.
        StartCoroutine(UpdateDrift());
    }

    IEnumerator UpdateDrift()
    {
        while (minigameActive)
        {
            // Randomly choose a new drift value between -currentBaseDriftSpeed and +currentBaseDriftSpeed.
            currentDriftSpeed = Random.Range(-currentBaseDriftSpeed, currentBaseDriftSpeed);
            yield return new WaitForSeconds(currentDriftChangeInterval);
        }
    }

    void EndMinigame(bool success)
    {
        // Hide all slider UI objects.
        easySliderGO.SetActive(false);
        normalSliderGO.SetActive(false);
        hardSliderGO.SetActive(false);

        // Reset the minigame state.
        minigameActive = false;
        fishingAttemptStarted = false;
        holdTimer = 0f;
    }
}
