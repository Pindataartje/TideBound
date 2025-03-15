using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class UIAction
    {
        [Tooltip("Keybind to trigger this action")]
        public KeyCode primaryKey = KeyCode.None;

        [Tooltip("Secondary Keybind to trigger this action (optional)")]
        public KeyCode secondaryKey = KeyCode.None;

        [Tooltip("GameObjects to Activate when this key is pressed")]
        public GameObject[] activateObjects;

        [Tooltip("GameObjects to Deactivate when this key is pressed")]
        public GameObject[] deactivateObjects;

        [Tooltip("Animator References for UI Animations")]
        public Animator[] animators;

        [Tooltip("Trigger Animator Parameters (e.g., SlideIn, SlideOut)")]
        public string[] animationTriggers;
    }

    [Header("UI Actions List")]
    public List<UIAction> actions = new List<UIAction>();

    private void Update()
    {
        foreach (UIAction action in actions)
        {
            if ((action.primaryKey != KeyCode.None && Input.GetKeyDown(action.primaryKey)) ||
                (action.secondaryKey != KeyCode.None && Input.GetKeyDown(action.secondaryKey)))
            {
                HandleUIAction(action);
            }
        }
    }

    private void HandleUIAction(UIAction action)
    {
        // Handle Toggle Objects
        if (action.activateObjects != null && action.deactivateObjects != null)
        {
            foreach (GameObject obj in action.activateObjects)
            {
                if (obj != null)
                {
                    // Toggle if object is in both activate and deactivate lists
                    if (System.Array.Exists(action.deactivateObjects, element => element == obj))
                    {
                        obj.SetActive(!obj.activeSelf);
                    }
                    else
                    {
                        obj.SetActive(true);
                    }
                }
            }

            foreach (GameObject obj in action.deactivateObjects)
            {
                if (obj != null && !System.Array.Exists(action.activateObjects, element => element == obj))
                {
                    obj.SetActive(false);
                }
            }
        }

        // Handle Animations
        if (action.animators != null && action.animationTriggers != null)
        {
            foreach (Animator animator in action.animators)
            {
                if (animator != null)
                {
                    foreach (string trigger in action.animationTriggers)
                    {
                        if (!string.IsNullOrEmpty(trigger))
                        {
                            animator.ResetTrigger("SlideIn");
                            animator.ResetTrigger("SlideOut");
                            animator.SetTrigger(trigger);
                        }
                    }
                }
            }
        }
    }
}
