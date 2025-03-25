using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Length of a full day (in minutes) for baseline speed.")]
    public float dayLengthInMinutes = 10f;
    [Tooltip("Overall time multiplier.")]
    public float timeMultiplier = 1f;
    // timeOfDay is in range [0,1] where 0 represents midnight and 0.5 represents noon.
    private float timeOfDay = 0f;

    [Header("Initial & Manual Time")]
    [Tooltip("Initial time of day when play begins (0 = midnight, 0.5 = noon, 1 = midnight).")]
    [Range(0f, 1f)]
    public float manualTimeOfDay = 0f;
    [Tooltip("Override time progression with manual control.")]
    public bool overrideTime = false;

    [Header("Day/Night Multipliers")]
    [Tooltip("Multiplier for time progression during daytime (when timeOfDay is between 0.25 and 0.75).")]
    public float dayTimeMultiplier = 1f;
    [Tooltip("Multiplier for time progression during nighttime.")]
    public float nightTimeMultiplier = 1f;

    [Header("Digital Clock Display")]
    [Tooltip("Reference to a TextMeshProUGUI component for displaying the current time.")]
    public TextMeshProUGUI timeDisplay;

    [Header("Sun & Moon Settings")]
    public Light sunLight;
    public Light moonLight;

    [Header("Volume Overrides")]
    public Volume globalVolume;
    private PhysicallyBasedSky skySettings;
    private Exposure exposure;
    private Fog fog;

    [Header("Optional Settings")]
    [Tooltip("Toggle on/off updating the global volume settings (exposure, fog, etc.).")]
    public bool updateVolumeSettings = true;

    private float fogRandomOffset = 0f;
    private bool hasAppliedRandomOffset = false;

    private void Start()
    {
        // Initialize timeOfDay from the manualTimeOfDay setting.
        timeOfDay = manualTimeOfDay;

        if (globalVolume.profile.TryGet(out skySettings) &&
            globalVolume.profile.TryGet(out exposure) &&
            globalVolume.profile.TryGet(out fog))
        {
            Debug.Log("Day/Night Cycle: HDRP Volume settings found.");
        }
        else
        {
            Debug.LogError("Day/Night Cycle: Missing volume overrides in the global volume!");
        }
    }

    private void Update()
    {
        UpdateTimeOfDay();
        UpdateLighting();
        if (updateVolumeSettings)
        {
            UpdateAtmosphere();
        }
        UpdateTimeDisplay();
    }

    private void UpdateTimeOfDay()
    {
        // If override is enabled, set timeOfDay from manual slider.
        if (overrideTime)
        {
            timeOfDay = manualTimeOfDay;
        }
        else
        {
            // Determine if it's day or night.
            // Sun is above the horizon when timeOfDay is between 0.25 and 0.75.
            float multiplier = (timeOfDay >= 0.25f && timeOfDay <= 0.75f) ? dayTimeMultiplier : nightTimeMultiplier;
            float increment = (Time.deltaTime / (dayLengthInMinutes * 60f)) * timeMultiplier * multiplier;
            timeOfDay += increment;
            if (timeOfDay > 1f)
            {
                timeOfDay -= 1f; // Wrap-around
                hasAppliedRandomOffset = false;
            }
        }
    }

    private void UpdateLighting()
    {
        // Map timeOfDay to sun angle (from -90° at time 0 to 270° at time 1).
        float sunAngle = Mathf.Lerp(-90f, 270f, timeOfDay);
        float moonAngle = sunAngle + 180f;

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);

        // Toggle shadow casting: if sun is above horizon (0 to 180°), enable its shadows; otherwise, enable moon's.
        if (sunAngle >= 0f && sunAngle <= 180f)
        {
            if (sunLight.shadows != LightShadows.Soft)
                sunLight.shadows = LightShadows.Soft;
            if (moonLight.shadows != LightShadows.None)
                moonLight.shadows = LightShadows.None;
        }
        else
        {
            if (sunLight.shadows != LightShadows.None)
                sunLight.shadows = LightShadows.None;
            if (moonLight.shadows != LightShadows.Soft)
                moonLight.shadows = LightShadows.Soft;
        }
    }

    private void UpdateAtmosphere()
    {
        if (exposure != null)
        {
            exposure.fixedExposure.value = Mathf.Lerp(13f, 15f, 1f - Mathf.Clamp01(timeOfDay * 2f - 1f));
        }
        if (fog != null)
        {
            if (!hasAppliedRandomOffset)
            {
                fogRandomOffset = Random.Range(-10f, 10f);
                hasAppliedRandomOffset = true;
            }
            fog.baseHeight.value = Mathf.Lerp(-530f, -30.5f, 1f - Mathf.Clamp01(timeOfDay * 2f - 1f)) + fogRandomOffset;
        }
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.colorFilter.value = Color.Lerp(new Color32(0x13, 0x54, 0xA1, 255), Color.black, 1f - Mathf.Clamp01(timeOfDay * 2f - 1f));
            colorAdjustments.postExposure.value = Mathf.Lerp(1f, 0.5f, 1f - Mathf.Clamp01(timeOfDay * 2f - 1f));
        }
    }

    // Converts timeOfDay (0 to 1) into a digital time string and updates the TextMeshPro display.
    private void UpdateTimeDisplay()
    {
        if (timeDisplay != null)
        {
            float totalHours = timeOfDay * 24f;
            int hours = Mathf.FloorToInt(totalHours);
            int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);
            timeDisplay.text = string.Format("{0:00}:{1:00}", hours, minutes);
        }
    }
}
