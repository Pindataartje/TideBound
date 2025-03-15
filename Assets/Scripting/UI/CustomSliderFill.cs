using UnityEngine;
using UnityEngine.UI;

public class CustomSliderFill : MonoBehaviour
{
    public Image fillImage; // The semi-circle fill image
    public Slider slider;   // The slider controlling the fill

    private void Update()
    {
        // Set the fill amount based on the slider value
        fillImage.fillAmount = slider.value; // Ensure slider value is normalized (0 to 1)
    }
}
