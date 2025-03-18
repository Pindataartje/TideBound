using UnityEngine;

[ScriptTag("Item")]
public class FoodItem : MonoBehaviour
{
    public float foodAmount = 20f;

    void Start()
    {
        // Ensure the script is inactive by default.
        // This can also be set manually in the Inspector.
        enabled = false;
    }

    void Update()
    {
        // Check for left mouse button input.
        if (Input.GetMouseButtonDown(0))
        {
            // Look for the Movement script (assumed to be on the player)
            Movement movement = FindObjectOfType<Movement>();
            if (movement != null)
            {
                movement.currentHunger += foodAmount;
                Debug.Log("Food consumed! Health increased by " + foodAmount);
            }
            else
            {
                Debug.LogWarning("Movement script not found!");
            }

            // Destroy this food item after consumption.
            Destroy(gameObject);
        }
    }
}
