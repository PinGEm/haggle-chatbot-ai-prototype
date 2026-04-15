using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [Header("Click Sounds")]
    [SerializeField] private AudioSource _soundSFxObject;
    [SerializeField] private AudioClip _clickSoundClip;
    [SerializeField] private AudioClip[] _clickSoundClips;

    [Header("Movement Sounds (Experimental)")]
    [SerializeField] private AudioSource _dragAudioSource;
    [SerializeField] private AudioClip _dragSwooshClip;
    [Range(0f, 1f)] public float dragVolume = 0.2f;
    public float movementThreshold = 0.5f;
    public float fadeSpeed = 15f; // NEW: Controls how smooth the fade in/out is!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Pointer.current == null) return;

        // 1. Check for single clicks
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Click();
        }

        // 2. Handle the continuous movement sound
        HandleMovementSound();
    }

    private void HandleMovementSound()
    {
        if (_dragAudioSource == null || _dragSwooshClip == null) return;

        // Just check the raw mouse speed, NO clicking required!
        float currentMouseSpeed = Pointer.current.delta.ReadValue().magnitude;
        float targetVolume = 0f;

        // If moving the mouse fast enough, our target volume is the max volume
        if (currentMouseSpeed > movementThreshold)
        {
            targetVolume = dragVolume;
        }

        // Make sure the audio source is always looping silently in the background
        if (!_dragAudioSource.isPlaying)
        {
            _dragAudioSource.clip = _dragSwooshClip;
            _dragAudioSource.loop = true;
            _dragAudioSource.volume = 0f; // Start silent so it doesn't pop
            _dragAudioSource.Play();
        }

        // THE GLITCH FIX: Smoothly slide the volume up and down instead of instantly cutting it
        _dragAudioSource.volume = Mathf.Lerp(_dragAudioSource.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(_soundSFxObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, audioClip.Length);
        AudioSource audioSource = Instantiate(_soundSFxObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip[rand];
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void Click()
    {
        PlayRandomSoundFXClip(_clickSoundClips, transform, 1f);
    }
}