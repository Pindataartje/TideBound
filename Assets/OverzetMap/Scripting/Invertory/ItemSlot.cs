using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public string itemTag; // Set this in the Inspector, e.g., "Stick"
    public int maxAmount = 10; // Max number of this item type in this slot

    private List<GameObject> storedItems = new List<GameObject>();

    public bool IsFull()
    {
        return storedItems.Count >= maxAmount;
    }

    public bool StoreItem(GameObject item)
    {
        if (!IsFull())
        {
            storedItems.Add(item);
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.SetActive(true);

            // ✅ Disable physics but KEEP colliders enabled for raycasting
            DisableItemPhysics(item);

            return true;
        }
        return false;
    }

    public GameObject RemoveItem()
    {
        if (storedItems.Count > 0)
        {
            GameObject item = storedItems[storedItems.Count - 1];
            storedItems.RemoveAt(storedItems.Count - 1);

            // ✅ Re-enable physics for dropped item
            EnableItemPhysics(item);

            return item;
        }
        return null;
    }

    public int GetItemCount()
    {
        return storedItems.Count;
    }

    private void DisableItemPhysics(GameObject item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true; // Prevent movement
            // 🚨 Remove this line (detectCollisions = false) so raycasts still work!
        }

        if (item.TryGetComponent(out Floater floater))
        {
            floater.enabled = false;
        }

        if (item.TryGetComponent(out Collider col))
        {
            col.enabled = true; // Ensure colliders stay active for raycasting
        }
    }

    private void EnableItemPhysics(GameObject item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false; // Enable physics
            rb.detectCollisions = true; // ✅ Make sure it interacts with physics again
        }

        if (item.TryGetComponent(out Floater floater))
        {
            floater.enabled = true;
        }

        if (item.TryGetComponent(out Collider col))
        {
            col.enabled = true; // Ensure colliders remain enabled
        }
    }
}
