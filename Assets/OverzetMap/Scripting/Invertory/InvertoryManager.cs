using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Assign your inventory panel
    public Transform dropPosition; // Empty object in front of camera where dropped items spawn
    public TextMeshProUGUI itemInfoText; // UI text to show item count
    private ItemSlot hoveredSlot; // Track hovered slot

    // Reference to the PickupSystem script so we can call EquipItem
    public PickupSystem pickupSystem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (inventoryUI.activeSelf)
        {
            DetectHoveredItem();

            if (Input.GetMouseButtonDown(1)) // Right Click to drop
            {
                TryDropHoveredItem();
            }

            if (Input.GetMouseButtonDown(0)) // Left Click to equip
            {
                TryEquipHoveredItem();
            }
        }
    }

    void ToggleInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        Cursor.lockState = inventoryUI.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryUI.activeSelf;
    }

    void DetectHoveredItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ItemSlot newHoveredSlot = hit.collider.GetComponent<ItemSlot>();

            if (newHoveredSlot == null && hit.collider.transform.parent != null)
                newHoveredSlot = hit.collider.transform.parent.GetComponent<ItemSlot>();

            hoveredSlot = newHoveredSlot;
            UpdateItemText();
        }
        else
        {
            hoveredSlot = null;
            itemInfoText.text = "";
        }
    }

    void TryDropHoveredItem()
    {
        if (hoveredSlot != null)
        {
            DropItem(hoveredSlot);
        }
        else
        {
            Debug.Log("❌ No inventory item detected when trying to drop.");
        }
    }

    void DropItem(ItemSlot slot)
    {
        if (slot == null || slot.GetItemCount() <= 0)
        {
            Debug.Log("❌ Tried to drop an item, but inventory slot is empty.");
            return;
        }

        GameObject droppedItem = slot.RemoveItem();
        if (droppedItem)
        {
            droppedItem.SetActive(true);
            droppedItem.transform.position = dropPosition.position;
            droppedItem.transform.SetParent(null);

            if (droppedItem.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = false;

            if (droppedItem.TryGetComponent(out Collider col))
                col.enabled = true;

            Debug.Log($"✅ Dropped: {slot.itemTag}");
            UpdateItemText();
        }
        else
        {
            Debug.Log("❌ Tried to drop an item, but no object was returned.");
        }
    }

    void TryEquipHoveredItem()
    {
        if (hoveredSlot != null && hoveredSlot.GetItemCount() > 0)
        {
            // Optional: Check if handPoint is free before removing the item.
            if (pickupSystem.handPoint.childCount > 0)
            {
                Debug.Log("❌ Cannot equip - Already holding an item!");
                return;
            }
            // Remove the item from the inventory slot...
            GameObject itemToEquip = hoveredSlot.RemoveItem();
            // ...and pass it to PickupSystem to equip.
            pickupSystem.EquipItem(itemToEquip);
            UpdateItemText();
        }
    }

    void UpdateItemText()
    {
        if (hoveredSlot != null)
        {
            int count = hoveredSlot.GetItemCount();
            itemInfoText.text = count > 0 ? $"{hoveredSlot.itemTag}: {count}" : "";
        }
        else
        {
            itemInfoText.text = "";
        }
    }
}
