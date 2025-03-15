using UnityEngine;
using UnityEngine.UI;

public class SemiCircleSlider : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider;          // Reference to the Slider component
    public float minAngle = -90f;  // Minimum rotation angle (leftmost position)
    public float maxAngle = 90f;   // Maximum rotation angle (rightmost position)

    [Header("Pointer Settings")]
    public RectTransform pointer;  // Pointer/arrow RectTransform to rotate

    private void Update()
    {
        UpdatePointerRotation();
    }

    private void UpdatePointerRotation()
    {
        // Map the slider's normalized value (0 to 1) to the rotation range (minAngle to maxAngle)
        float normalizedValue = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, normalizedValue);

        // Apply the calculated rotation to the pointer
        pointer.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }
}
