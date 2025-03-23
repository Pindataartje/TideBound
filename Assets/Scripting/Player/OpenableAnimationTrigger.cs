using UnityEngine;

public class OpenableAnimationTrigger : MonoBehaviour
{
    // Maximum distance to detect an interactable object
    public float raycastDistance = 5f;

    void Update()
    {
        // Check if the player presses the "E" key
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Create a ray from the camera's position in its forward direction
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                // Check if the hit object has the tag "Openable"
                if (hit.collider.CompareTag("Openable"))
                {
                    // Try to get the Animator component on the hit object
                    Animator anim = hit.collider.GetComponent<Animator>();
                    if (anim != null)
                    {
                        // Set both triggers; your Animator Controller should handle the toggle logic.
                        anim.SetTrigger("Close");
                        anim.SetTrigger("Open");
                    }
              
                }
            }
        }
    }
}