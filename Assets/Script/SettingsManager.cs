using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("🎵 AUDIO SETTINGS")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("🖥️ GRAPHICS SETTINGS")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("🎮 UI ELEMENTS")]
    public GameObject settingsPanel;
    public Button backButton;
    public AudioClip clickSound;

    private AudioSource audioSource;
    private Resolution[] resolutions;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup resolutions
        SetupResolutions();

        // Load saved settings
        LoadSettings();

        // Add listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQuality);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);

        // Hide panel initially
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("[SettingsManager] Initialized!");
    }

    void SetupResolutions()
    {
        resolutions = Screen.resolutions;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            PlayClickSound();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            PlayClickSound();
            SaveSettings();
        }
    }

    // 🔊 VOLUME CONTROLS
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }

        if (masterVolumeText != null)
        {
            masterVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }

        if (musicVolumeText != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    // 🖥️ GRAPHICS CONTROLS
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log($"[Settings] Quality set to: {QualitySettings.names[qualityIndex]}");
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"[Settings] Resolution set to: {resolution.width} x {resolution.height}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"[Settings] Fullscreen: {isFullscreen}");
    }

    // 💾 SAVE/LOAD SETTINGS
    void SaveSettings()
    {
        // Audio
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);

        // Graphics
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown != null ? resolutionDropdown.value : 0);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("[Settings] Settings saved!");
    }

    void LoadSettings()
    {
        // Audio
        if (masterVolumeSlider != null)
        {
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.value = masterVol;
            SetMasterVolume(masterVol);
        }

        if (musicVolumeSlider != null)
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.value = musicVol;
            SetMusicVolume(musicVol);
        }

        if (sfxVolumeSlider != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.value = sfxVol;
            SetSFXVolume(sfxVol);
        }

        // Graphics
        int quality = PlayerPrefs.GetInt("QualityLevel", 2);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = quality;
        }
        QualitySettings.SetQualityLevel(quality);

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = resIndex;
        }

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
        }
        Screen.fullScreen = fullscreen;

        Debug.Log("[Settings] Settings loaded!");
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Reset to default settings
    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 1f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
        if (qualityDropdown != null) qualityDropdown.value = 2;
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;

        SaveSettings();
        PlayClickSound();

        Debug.Log("[Settings] Reset to defaults!");
    }
}