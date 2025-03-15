using UnityEngine;
using TMPro;

public class SimpleResourceDisplay : MonoBehaviour
{
    [Tooltip("The display name, e.g., 'Stick'")]
    public string resourceName;

    [Tooltip("Assign the ItemSlot transform (the empty that holds the resource items)")]
    public Transform resourceSlot;

    [Tooltip("Assign the TextMeshProUGUI component that displays the resource count")]
    public TextMeshProUGUI displayText;

    void Update()
    {
        int count = resourceSlot != null ? resourceSlot.childCount : 0;
        displayText.text = $"{resourceName}: {count}";
    }
}
