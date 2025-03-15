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
    private ColorParameter groundTint;
    private ColorParameter horizonTint;

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
        UpdateAtmosphere();
    }

    private void UpdateTimeOfDay()
    {
        timeOfDay += (Time.deltaTime / (dayLengthInMinutes * 60f)) * timeMultiplier;
        if (timeOfDay > 1f)
        {
            timeOfDay -= 1f; // Instead of resetting instantly, continue the cycle smoothly
            hasAppliedRandomOffset = false; // Reset flag for new cycle
        }
    }

    private void UpdateLighting()
    {
        float sunAngle = Mathf.Lerp(-90f, 270f, timeOfDay); // Smooth continuous rotation
        float moonAngle = sunAngle + 180f; // Moon opposite of the Sun

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);
    }

    private void UpdateAtmosphere()
    {
        if (exposure != null)
        {
            exposure.fixedExposure.value = Mathf.Lerp(13f, 15f, 1f - Mathf.Clamp01(timeOfDay * 2f - 1f));
        }

        if (fog != null)
        {
            if (!hasAppliedRandomOffset) // Apply random variation only once per cycle
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
