using UnityEngine;

[System.Serializable]
public class ItemPrefabMapping
{
    public string itemTag;      // e.g. "Stick" or "Stone"
    public GameObject prefab;   // The prefab to instantiate for this item
}

public class InventoryItemAdder : MonoBehaviour
{
    [Tooltip("Assign the Bag transform that holds your ItemSlot children.")]
    public Transform bag;

    [Tooltip("Mapping between item tags and their prefabs.")]
    public ItemPrefabMapping[] prefabMappings;

    /// <summary>
    /// Adds a specified amount of the given item prefab to the inventory.
    /// Returns true if all items were added successfully.
    /// </summary>
    /// <param name="itemPrefab">The prefab to add (its tag must match an ItemSlot's itemTag).</param>
    /// <param name="amount">How many of this item to add.</param>
    public bool AddItem(GameObject itemPrefab, int amount)
    {
        if (bag == null)
        {
            Debug.LogError("Bag is not assigned in InventoryItemAdder.");
            return false;
        }

        // Get the tag from the itemPrefab.
        string tagToMatch = itemPrefab.tag;

        // Find the corresponding ItemSlot by checking each child of bag.
        ItemSlot targetSlot = null;
        ItemSlot[] slots = bag.GetComponentsInChildren<ItemSlot>();
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemTag == tagToMatch)
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot == null)
        {
            Debug.LogError("No ItemSlot found for tag: " + tagToMatch);
            return false;
        }

        // Loop to add the requested amount.
        for (int i = 0; i < amount; i++)
        {
            if (targetSlot.IsFull())
            {
                Debug.LogWarning("ItemSlot for " + tagToMatch + " is full. Could not add all items.");
                return false;
            }

            // Instantiate a new instance of the prefab.
            GameObject newItem = Instantiate(itemPrefab);
            newItem.SetActive(true);

            // Store the new item in the target slot.
            bool stored = targetSlot.StoreItem(newItem);
            if (!stored)
            {
                Debug.LogWarning("Failed to store item: " + newItem.name);
                Destroy(newItem); // Clean up if not stored.
                return false;
            }
        }

        Debug.Log("Successfully added " + amount + " " + tagToMatch + " to the inventory.");
        return true;
    }

    /// <summary>
    /// Adds an item by looking up the prefab based on the provided tag.
    /// </summary>
    /// <param name="itemTag">The tag of the item to add (must match a mapping and an ItemSlot's itemTag).</param>
    /// <param name="amount">How many to add.</param>
    public bool AddItemByTag(string itemTag, int amount)
    {
        foreach (var mapping in prefabMappings)
        {
            if (mapping.itemTag == itemTag)
            {
                return AddItem(mapping.prefab, amount);
            }
        }
        Debug.LogError("No prefab mapping found for tag: " + itemTag);
        return false;
    }
}
