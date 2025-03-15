using UnityEngine;

public class PlayerOnBoatHandler : MonoBehaviour
{
    public Rigidbody boatRigidbody; // Assign the boat's Rigidbody in the inspector
    private Rigidbody playerRigidbody;

    private bool isOnBoat = false;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isOnBoat && boatRigidbody != null)
        {
            // Match boat's velocity
            Vector3 boatVelocity = boatRigidbody.velocity;
            Vector3 adjustedVelocity = new Vector3(boatVelocity.x, 0, boatVelocity.z); // Ignore vertical velocity
            playerRigidbody.velocity += adjustedVelocity * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boat")) // Ensure the boat has the tag "Boat"
        {
            isOnBoat = true;
            boatRigidbody = collision.rigidbody;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boat"))
        {
            isOnBoat = false;
            boatRigidbody = null;
        }
    }
}
