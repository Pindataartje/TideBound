using UnityEngine;

[ScriptTag("Item")]
public class FoodItem : MonoBehaviour
{
    // Effects of consuming this item.
    // Positive values add to the stat, negative values remove from it.
    public float hungerChange = 20f;
    public float thirstChange = 0f;
    public float healthChange = 0f;
    public float staminaChange = 0f;

    void Start()
    {
        // Optionally disable the script by default (if you want to activate it manually).
        enabled = false;
    }

    void Update()
    {
        // Check for left mouse button input to consume the item.
        if (Input.GetMouseButtonDown(0))
        {
            // Find the player's Movement script in the scene using the new API.
            Movement movement = Object.FindAnyObjectByType<Movement>();
            if (movement != null)
            {
                // Apply the changes.
                movement.currentHunger += hungerChange;
                movement.currentThirst += thirstChange;
                movement.currentHealth += healthChange;
                movement.currentStamina += staminaChange;

                // Optionally, if you want clamping logic here you can add it, but
                // your Movement script might already clamp these values.

                Debug.Log("Item consumed! " +
                    "Hunger change: " + hungerChange + ", " +
                    "Thirst change: " + thirstChange + ", " +
                    "Health change: " + healthChange + ", " +
                    "Stamina change: " + staminaChange);
            }
            else
            {
                Debug.LogWarning("Movement script not found!");
            }

            // Destroy the item after consumption.
            Destroy(gameObject);
        }
    }
}
