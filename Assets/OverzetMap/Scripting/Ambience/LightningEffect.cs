using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Collections;

public class LightningEffect : MonoBehaviour
{
    public Light moonLight; // Assign the moonlight (Directional Light)
    public Volume volume; // Assign the HDRP Volume
    public AudioSource audioSource; // Assign an AudioSource for thunder sounds
    public AudioClip[] thunderSounds; // Assign different thunder sounds

    public float minTimeBetweenFlashes = 3f; // Minimum wait time between lightning flashes
    public float maxTimeBetweenFlashes = 10f; // Maximum wait time between lightning flashes
    public float flashIntensity = 999999f; // Intensity during lightning
    public float normalIntensity = 0f; // Normal moonlight intensity
    public Color lightningColor = new Color(0.164f, 0.290f, 0.518f); // #2A4A84
    public Color normalColor = Color.black; // Normal horizon tint color
    public float flashDuration = 0.2f; // Duration of a single lightning flash
    public int flickerCount = 2; // Number of flickers per lightning event
    public float flickerInterval = 0.05f; // Time between flickers
    public float minThunderDelay = 1f; // Minimum delay before thunder
    public float maxThunderDelay = 3f; // Maximum delay before thunder

    private bool isFlashing = false;
    private PhysicallyBasedSky skySettings;

    private void Start()
    {
        // Try to get the Physically Based Sky component from the volume
        if (volume.profile.TryGet(out skySettings))
        {
            skySettings.horizonTint.value = normalColor; // Set initial color
        }

        StartCoroutine(LightningRoutine());
    }

    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            // Wait for a random time before the next lightning strike
            yield return new WaitForSeconds(Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes));

            // Start the lightning flash effect and play thunder after
            StartCoroutine(FlashLightning());
        }
    }

    private IEnumerator FlashLightning()
    {
        if (isFlashing) yield break; // Prevent multiple lightning flashes at once
        isFlashing = true;

        for (int i = 0; i < flickerCount; i++)
        {
            ApplyLightningEffect(true);
            yield return new WaitForSeconds(flickerInterval);
            ApplyLightningEffect(false);
            yield return new WaitForSeconds(flickerInterval);
        }

        // Ensure it returns to normal after flickering
        ApplyLightningEffect(false);

        // Randomly delay and then play a thunder sound
        StartCoroutine(PlayThunderWithDelay());

        isFlashing = false;
    }

    private void ApplyLightningEffect(bool isLightning)
    {
        if (moonLight != null)
        {
            moonLight.intensity = isLightning ? flashIntensity : normalIntensity;
        }

        if (skySettings != null)
        {
            skySettings.horizonTint.value = isLightning ? lightningColor : normalColor;
        }
    }

    private IEnumerator PlayThunderWithDelay()
    {
        if (thunderSounds.Length == 0 || audioSource == null) yield break;

        // Random delay between 1 to 3 seconds before playing thunder
        float delay = Random.Range(minThunderDelay, maxThunderDelay);
        yield return new WaitForSeconds(delay);

        // Choose a random thunder sound from the array
        AudioClip selectedThunder = thunderSounds[Random.Range(0, thunderSounds.Length)];
        audioSource.PlayOneShot(selectedThunder);
    }
}
