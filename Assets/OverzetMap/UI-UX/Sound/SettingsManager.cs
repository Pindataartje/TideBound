using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // for scene loading

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer & Sliders")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambienceSlider;

    [Header("Resolution Settings")]
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private Button resolutionLeftButton;
    [SerializeField] private Button resolutionRightButton;

    [Header("Fullscreen Toggle")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("FPS Display")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [Tooltip("Toggle for showing FPS.")]
    [SerializeField] private Toggle fpsToggle;
    [Tooltip("How often (in seconds) to update the FPS text.")]
    [SerializeField] private float fpsUpdateInterval = 0.5f;

    // Internal variables.
    private List<Resolution> validResolutions = new List<Resolution>();
    private int currentResolutionIndex = 0;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string AMBIENCE_VOLUME_KEY = "AmbienceVolume";
    private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string SHOW_FPS_KEY = "ShowFPS";

    private Coroutine fpsCoroutine;

    private void Awake()
    {
        // Filter valid 16:9 resolutions.
        Resolution[] allResolutions = Screen.resolutions;
        foreach (var res in allResolutions)
        {
            float aspect = (float)res.width / res.height;
            if (Mathf.Abs(aspect - (16f / 9f)) < 0.01f)
            {
                validResolutions.Add(res);
            }
        }

        // Hook up resolution arrow buttons.
        if (resolutionLeftButton != null)
            resolutionLeftButton.onClick.AddListener(DecreaseResolutionIndex);
        if (resolutionRightButton != null)
            resolutionRightButton.onClick.AddListener(IncreaseResolutionIndex);

        // Hook up fullscreen toggle.
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);

        // Hook up FPS toggle.
        if (fpsToggle != null)
            fpsToggle.onValueChanged.AddListener(SetShowFPS);

        LoadSettings();
        ApplyAllSettings();

        // Start updating FPS if assigned.
        if (fpsText != null)
        {
            fpsCoroutine = StartCoroutine(UpdateFPS());
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region Volume Methods
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
    }

    public void SetAmbienceVolume(float value)
    {
        audioMixer.SetFloat("AmbienceVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(AMBIENCE_VOLUME_KEY, value);
    }
    #endregion

    #region Resolution Methods
    private void IncreaseResolutionIndex()
    {
        currentResolutionIndex++;
        if (currentResolutionIndex >= validResolutions.Count)
            currentResolutionIndex = validResolutions.Count - 1;

        ApplyResolution();
    }

    private void DecreaseResolutionIndex()
    {
        currentResolutionIndex--;
        if (currentResolutionIndex < 0)
            currentResolutionIndex = 0;

        ApplyResolution();
    }

    private void ApplyResolution()
    {
        if (validResolutions.Count == 0) return;

        Resolution chosen = validResolutions[currentResolutionIndex];
        Screen.SetResolution(chosen.width, chosen.height, Screen.fullScreen);
        if (resolutionText != null)
            resolutionText.text = $"{chosen.width} x {chosen.height}";
        PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, currentResolutionIndex);
    }
    #endregion

    #region Fullscreen Methods
    private void OnFullscreenToggle(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFull ? 1 : 0);
    }
    #endregion

    #region FPS Display Methods
    public void SetShowFPS(bool show)
    {
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(show);
            PlayerPrefs.SetInt(SHOW_FPS_KEY, show ? 1 : 0);
        }
    }

    private IEnumerator UpdateFPS()
    {
        while (true)
        {
            if (fpsText != null && fpsText.gameObject.activeSelf)
            {
                float fps = 1f / Time.unscaledDeltaTime;
                fpsText.text = $"FPS: {fps:F0}";
            }
            yield return new WaitForSeconds(fpsUpdateInterval);
        }
    }

    #endregion

    #region Load/Apply/Save
    private void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        float ambienceVol = PlayerPrefs.GetFloat(AMBIENCE_VOLUME_KEY, 1f);

        currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, 0);
        if (currentResolutionIndex < 0 || currentResolutionIndex >= validResolutions.Count)
            currentResolutionIndex = 0;

        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        bool showFPS = PlayerPrefs.GetInt(SHOW_FPS_KEY, 0) == 1;

        if (musicSlider != null) musicSlider.value = musicVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;
        if (ambienceSlider != null) ambienceSlider.value = ambienceVol;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        if (fpsToggle != null) fpsToggle.isOn = showFPS;
        if (fpsText != null) fpsText.gameObject.SetActive(showFPS);
    }

    private void ApplyAllSettings()
    {
        if (musicSlider != null) SetMusicVolume(musicSlider.value);
        if (sfxSlider != null) SetSFXVolume(sfxSlider.value);
        if (ambienceSlider != null) SetAmbienceVolume(ambienceSlider.value);

        ApplyResolution();
        if (fullscreenToggle != null)
            Screen.fullScreen = fullscreenToggle.isOn;
    }
    #endregion

    #region Scene Loading
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    #endregion
}
