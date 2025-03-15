using UnityEngine;

[ScriptTag("Item")]
public class ItemAdderTest : MonoBehaviour
{
    // Reference to the InventoryItemAdder in the scene.
    public InventoryItemAdder inventoryItemAdder;

    void Update()
    {
        // On left mouse click, add one "Stick".
        if (Input.GetMouseButtonDown(0))
        {
            inventoryItemAdder.AddItemByTag("Stick", 1);
        }

        // On right mouse click, add two "Stone".
        if (Input.GetMouseButtonDown(1))
        {
            inventoryItemAdder.AddItemByTag("Stone", 2);
        }
    }
}
