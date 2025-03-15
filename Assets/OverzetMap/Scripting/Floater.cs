using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Floater : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmersion = 1f;   // Depth at which the floater is fully submerged
    public float displacementAmount = 3f;      // Buoyancy force multiplier
    public int floaters = 4;                   // Number of floaters

    public float springForce = 10f;            // Spring strength for buoyancy
    public float dampingForce = 5f;            // Damping to reduce oscillations
    public float waterDrag = 0.99f;            // Linear drag
    public float waterAngularDrag = 0.5f;      // Angular drag

    public WaterSurface water;               // HDRP Water Surface

    // Frequency settings: update water height every 'updateWaterFrequency' FixedUpdates
    private int fixedUpdateCounter = 0;
    public int updateWaterFrequency = 2;       // Update water height every 2 FixedUpdates

    // Tolerance: only apply buoyancy if the object is submerged more than this value.
    // If the difference between water height and floater's y position is less than or equal to this, skip calculations.
    public float submersionTolerance = 0.2f;

    private WaterSearchParameters searchParams;
    private WaterSearchResult searchResult;

    private void Start()
    {
        // If the water surface is not assigned, try to find it automatically.
        if (water == null)
        {
            water = FindObjectOfType<WaterSurface>();

            if (water == null)
            {
                Debug.LogError("❌ No WaterSurface object found in the scene! Please add an HDRP Water Surface.");
                enabled = false; // Disable the script to prevent further errors
                return;
            }
            else
            {
                Debug.Log("🌊 WaterSurface automatically assigned: " + water.gameObject.name);
            }
        }
    }

    private void FixedUpdate()
    {
        if (water == null) return;

        // Always apply gravity divided by the number of floaters.
        rb.AddForceAtPosition(Physics.gravity / floaters, transform.position, ForceMode.Acceleration);

        fixedUpdateCounter++;
        if (fixedUpdateCounter % updateWaterFrequency == 0)
        {
            searchParams.startPositionWS = transform.position;
            water.ProjectPointOnWaterSurface(searchParams, out searchResult);
        }

        float waterHeight = searchResult.projectedPositionWS.y;
        float floaterHeight = transform.position.y;
        float submersionDepth = waterHeight - floaterHeight;

        // Only apply buoyancy and drag if the floater is sufficiently submerged.
        if (submersionDepth > submersionTolerance)
        {
            float displacementMultiplier = Mathf.Clamp01(submersionDepth / depthBeforeSubmersion);

            // Compute buoyancy force, spring force, and damping.
            float buoyancyForce = displacementMultiplier * displacementAmount * Mathf.Abs(Physics.gravity.y);
            float spring = springForce * submersionDepth;
            float damping = dampingForce * rb.velocity.y;

            rb.AddForceAtPosition(new Vector3(0f, buoyancyForce + spring - damping, 0f), transform.position, ForceMode.Acceleration);

            // Apply linear drag and angular drag.
            rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        // If submersionDepth is less than or equal to submersionTolerance, the object is considered not submerged enough,
        // and no buoyancy or drag corrections are applied.
    }
}
