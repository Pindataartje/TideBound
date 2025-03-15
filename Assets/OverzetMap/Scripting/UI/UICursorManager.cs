using UnityEngine;

public class UICursorManager : MonoBehaviour
{
    public UIManager uiManager; // Reference to your existing UIManager

    private void Update()
    {
        if (uiManager == null) return;

        foreach (var action in uiManager.actions)
        {
            if (AnyObjectActive(action.activateObjects))
            {
                UnlockCursor(); // Unlock the cursor if any UI is active
                return; // Exit early, since we only need to unlock once
            }
        }

        LockCursor(); // If no UI is active, lock the cursor
    }

    private bool AnyObjectActive(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf) return true;
        }
        return false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
