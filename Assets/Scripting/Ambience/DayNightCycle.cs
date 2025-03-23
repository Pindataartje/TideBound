using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthInMinutes = 10f;
    public float timeMultiplier = 1f;
    private float timeOfDay = 0f;

    [Header("Sun & Moon Settings")]
    public Light sunLight;
    public Light moonLight;

    [Header("Volume Overrides")]
    public Volume globalVolume;
    private PhysicallyBasedSky skySettings;
    private Exposure exposure;
    private Fog fog;
    // Other volume parameters like groundTint, horizonTint, etc., can be added as needed.

    [Header("Optional Settings")]
    [Tooltip("Toggle on/off updating the global volume settings (e.g., exposure, fog, color adjustments).")]
    public bool updateVolumeSettings = true;

    private float fogRandomOffset = 0f;
    private bool hasAppliedRandomOffset = false;

    private void Start()
    {
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
    }

    private void UpdateTimeOfDay()
    {
        timeOfDay += (Time.deltaTime / (dayLengthInMinutes * 60f)) * timeMultiplier;
        if (timeOfDay > 1f)
        {
            timeOfDay -= 1f; // Continue cycle smoothly.
            hasAppliedRandomOffset = false; // Reset for new cycle.
        }
    }

    private void UpdateLighting()
    {
        // Compute sun and moon angles.
        float sunAngle = Mathf.Lerp(-90f, 270f, timeOfDay);
        float moonAngle = sunAngle + 180f;

        // Set rotations.
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);

        // Toggle shadow casting based on the sun's position.
        if (sunAngle >= 0f && sunAngle <= 180f)
        {
            if (sunLight.shadows != LightShadows.Soft)
                sunLight.shadows = LightShadows.Soft; // Enable sun shadows.
            if (moonLight.shadows != LightShadows.None)
                moonLight.shadows = LightShadows.None; // Disable moon shadows.
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
}
