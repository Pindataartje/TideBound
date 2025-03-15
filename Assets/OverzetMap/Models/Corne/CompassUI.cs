using UnityEngine;
using UnityEngine.UI;

public class CompassUI : MonoBehaviour
{
    public RectTransform compassImage; // De UI-afbeelding van het kompas
    public Transform player; // De speler of camera

    void Update()
    {
        if (player != null)
        {
            float heading = player.eulerAngles.y; // Haal de rotatie van de speler op
            compassImage.localRotation = Quaternion.Euler(0, 0, -heading); // Draai de UI
        }
    }
}
