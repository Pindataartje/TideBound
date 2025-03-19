using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public Transform bag; // Reference to your inventory bag (which holds the ItemSlots).

    // Audio source to play when an item is crafted.
    public AudioSource craftAudioSource;

    // Call this method from your Craft button (via the OnClick event) and pass in a CraftingRecipe asset.
    public void CraftItem(CraftingRecipe recipe)
    {
        // First, check if the inventory has enough of each required resource.
        foreach (var req in recipe.requirements)
        {
            ItemSlot slot = GetItemSlot(req.resourceTag);
            if (slot == null || slot.GetItemCount() < req.amount)
            {
                Debug.Log($"Not enough {req.resourceTag} to craft {recipe.recipeName}.");
                return;
            }
        }

        // Remove the required items from the inventory.
        foreach (var req in recipe.requirements)
        {
            ItemSlot slot = GetItemSlot(req.resourceTag);
            // Remove the specified number of items.
            for (int i = 0; i < req.amount; i++)
            {
                GameObject removedItem = slot.RemoveItem();
                if (removedItem != null)
                {
                    // Optionally destroy the consumed item.
                    Destroy(removedItem);
                }
            }
        }

        // Instantiate the output prefab.
        GameObject craftedItem = Instantiate(recipe.outputPrefab);
        // Find the corresponding ItemSlot for the crafted item, based on its tag.
        string outputTag = craftedItem.tag;
        ItemSlot outputSlot = GetItemSlot(outputTag);
        if (outputSlot != null && !outputSlot.IsFull())
        {
            outputSlot.StoreItem(craftedItem);
            Debug.Log($"Crafted {recipe.recipeName} and stored it in inventory.");
        }
        else
        {
            Debug.Log($"Crafted {recipe.recipeName} but inventory is full for {outputTag}. Dropping item.");
            // If no valid slot is available, you can drop the item in the world.
            craftedItem.transform.position = Vector3.zero;
        }

        // Play the craft sound, if an AudioSource is assigned.
        if (craftAudioSource != null)
        {
            craftAudioSource.Play();
        }
    }

    // Helper function to find an ItemSlot by item tag.
    ItemSlot GetItemSlot(string itemTag)
    {
        ItemSlot[] slots = bag.GetComponentsInChildren<ItemSlot>();
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemTag == itemTag)
                return slot;
        }
        return null;
    }
}
