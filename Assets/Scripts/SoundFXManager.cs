using UnityEngine;
using UnityEngine.Rendering;

public class SoundFXManager : MonoBehaviour
{

    public static SoundFXManager Instance;


    [SerializeField] private AudioSource soundSFxObject;
    [SerializeField] private AudioClip clickSoundClip;
    [SerializeField] private AudioClip[] clickSoundClips;




    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {

        AudioSource audioSource = Instantiate(soundSFxObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);



    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {

        int rand = Random.Range(0, audioClip.Length);

        AudioSource audioSource = Instantiate(soundSFxObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip[rand];

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);



    }

    public void Click()
    {
        PlayRandomSoundFXClip(clickSoundClips, transform, 1f);
        Debug.Log("D");
    }


}
