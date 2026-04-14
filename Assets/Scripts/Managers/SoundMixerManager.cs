using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // <-- We need this to talk to your Sliders

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [Header("Drag your UI Sliders in here!")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Start()
    {
        // 1. Load the saved volume from the hard drive. 
        // (The '1f' is the default if it's their first time playing)
        float savedMaster = PlayerPrefs.GetFloat("masterVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("soundFXVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);

        // 2. Update the visual UI sliders on the screen to match the saved values
        if (masterSlider != null) masterSlider.value = savedMaster;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        if (musicSlider != null) musicSlider.value = savedMusic;

        // 3. Tell the Audio Mixer to actually use those loaded volumes
        // (Since your sliders are hooked up, updating the slider value in step 2 
        // usually triggers this automatically, but this guarantees it works!)
        SetMasterVolume(savedMaster);
        SetSoundFXVolume(savedSFX);
        SetMusicVolume(savedMusic);
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);

        // --- THIS IS WHAT SAVES THE VOLUME ---
        PlayerPrefs.SetFloat("masterVolume", level);
        PlayerPrefs.Save();
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);

        // Save SFX
        PlayerPrefs.SetFloat("soundFXVolume", level);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);

        // Save Music
        PlayerPrefs.SetFloat("musicVolume", level);
        PlayerPrefs.Save();
    }
}