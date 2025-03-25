using UnityEngine;

public class WaterFloaterActivator : MonoBehaviour
{
    // Reference to the Floater script on this object.
    private Floater floater;

    private void Awake()
    {
        // Get the Floater component (make sure it's on the same GameObject).
        floater = GetComponent<Floater>();
        if (floater == null)
        {
            Debug.LogWarning("WaterFloaterActivator: No Floater component found on " + gameObject.name);
        }
        // Optionally, disable the floater by default.
        if (floater != null)
        {
            floater.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If we enter a collider tagged "Water", enable the floater.
        if (other.CompareTag("Water"))
        {
            if (floater != null)
            {
                floater.enabled = true;
                // Optionally, log the event.
                Debug.Log("Entered Water: Floater enabled on " + gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When we exit a "Water" trigger, disable the floater.
        if (other.CompareTag("Water"))
        {
            if (floater != null)
            {
                floater.enabled = false;
                Debug.Log("Exited Water: Floater disabled on " + gameObject.name);
            }
        }
    }
}
