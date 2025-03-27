using System.Collections.Generic;
using UnityEngine;

public class ColliderActivator : MonoBehaviour
{
    [Header("Static Collider Settings (No Rigidbody)")]
    [Tooltip("Distance from the main camera at which static colliders (without Rigidbody) will be enabled.")]
    public float colliderActivationDistance = 50f;

    [Header("Rigidbody Object Settings")]
    [Tooltip("Distance from the main camera at which objects with Rigidbody will be activated.")]
    public float rbActivationDistance = 100f;

    [Header("Exclusion Settings")]
    [Tooltip("List of GameObjects that should always remain active (their colliders and rigidbodies won't be deactivated).")]
    public List<GameObject> exclusionList = new List<GameObject>();

    [Tooltip("Time interval (in seconds) between checks.")]
    public float updateInterval = 0.5f;

    // Timer for controlling update frequency
    private float timer = 0f;

    // Cache for static colliders (those without a Rigidbody)
    private Collider[] staticColliders;

    // Cache for rigidbody objects (which also have colliders)
    private Rigidbody[] rigidbodies;

    // Store the original isKinematic state for each Rigidbody
    private Dictionary<Rigidbody, bool> originalKinematic = new Dictionary<Rigidbody, bool>();

    void Start()
    {
        // Find all colliders in the scene using the new API.
        Collider[] allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);

        // Filter those that do NOT have a Rigidbody attached.
        List<Collider> staticColList = new List<Collider>();
        foreach (Collider col in allColliders)
        {
            if (col == null) continue;
            // Skip the player/manager itself.
            if (col.gameObject == this.gameObject)
                continue;
            // Skip objects in the exclusion list.
            if (exclusionList.Contains(col.gameObject))
                continue;
            if (col.attachedRigidbody == null)
                staticColList.Add(col);
        }
        staticColliders = staticColList.ToArray();

        // Cache all Rigidbody objects (with at least one collider) using the new API.
        rigidbodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);

        // For each rigidbody, store its original isKinematic state, if not excluded.
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null) continue;
            if (rb.gameObject == this.gameObject)
                continue;
            if (exclusionList.Contains(rb.gameObject))
                continue;
            if (!originalKinematic.ContainsKey(rb))
                originalKinematic.Add(rb, rb.isKinematic);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateStaticColliders();
            UpdateRigidbodies();
            timer = 0f;
        }
    }

    void UpdateStaticColliders()
    {
        Vector3 cameraPos = Camera.main ? Camera.main.transform.position : transform.position;

        foreach (Collider col in staticColliders)
        {
            if (col == null) continue;
            // Skip if this object is in the exclusion list.
            if (exclusionList.Contains(col.gameObject))
                continue;

            float distance = Vector3.Distance(col.transform.position, cameraPos);
            if (distance <= colliderActivationDistance)
            {
                // Enable collider if within range.
                if (!col.enabled)
                    col.enabled = true;
            }
            else
            {
                // Disable collider if out of range.
                if (col.enabled)
                    col.enabled = false;
            }
        }
    }

    void UpdateRigidbodies()
    {
        Vector3 cameraPos = Camera.main ? Camera.main.transform.position : transform.position;

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null) continue;
            // Skip the player/manager and any excluded objects.
            if (rb.gameObject == this.gameObject)
                continue;
            if (exclusionList.Contains(rb.gameObject))
                continue;

            float distance = Vector3.Distance(rb.transform.position, cameraPos);
            // Get all colliders attached to this Rigidbody.
            Collider[] cols = rb.GetComponents<Collider>();

            if (distance <= rbActivationDistance)
            {
                // Reactivation: enable colliders first.
                foreach (Collider col in cols)
                {
                    if (col != null && !col.enabled)
                        col.enabled = true;
                }
                // Then restore Rigidbody's original state.
                if (originalKinematic.TryGetValue(rb, out bool wasKinematic))
                {
                    // If the object was originally dynamic, ensure it is now non-kinematic.
                    if (!wasKinematic && rb.isKinematic)
                        rb.isKinematic = false;
                }
            }
            else
            {
                // Deactivation: first set Rigidbody to kinematic to prevent physics effects.
                if (!rb.isKinematic)
                    rb.isKinematic = true;
                // Then disable colliders.
                foreach (Collider col in cols)
                {
                    if (col != null && col.enabled)
                        col.enabled = false;
                }
            }
        }
    }
}
