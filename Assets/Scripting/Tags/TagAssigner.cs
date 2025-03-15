using UnityEngine;

public class TagAssigner : MonoBehaviour
{
    public string tagToAssign = "Untagged"; // Default tag, change in Inspector
    [Tooltip("Custom tag for gameplay logic. This does NOT change the GameObject's built-in tag.")]
    public string customTag; // This will store the assigned tag without modifying gameObject.tag

    void Start()
    {
        // Check if the tag exists before assigning it
        if (IsTagValid(tagToAssign))
        {
            customTag = tagToAssign;
            Debug.Log($"Custom tag '{customTag}' assigned to {gameObject.name}");
        }
        else
        {
            Debug.LogError($"Tag '{tagToAssign}' does not exist. Please add it in the Unity Tag Manager.");
        }
    }

    // Helper method to check if the tag exists in the Tag Manager
    private bool IsTagValid(string tag)
    {
#if UNITY_EDITOR
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (existingTag == tag) return true;
        }
#endif
        return false;
    }
}
