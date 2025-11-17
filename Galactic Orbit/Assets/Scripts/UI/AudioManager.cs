using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip ButtonSound;
    public AudioClip PurchaseSound;
    public AudioClip CollectSound;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load saved volume settings
        LoadVolumeSettings();
    }
    
   

    public void SetMusicVolume(float volume)
    {
        // Convert linear 0-1 value to logarithmic dB scale
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MusicVolume", dB);
        
        // Save the setting
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        // Convert linear 0-1 value to logarithmic dB scale
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVolume", dB);
        
        // Save the setting
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        // Load music volume (default to 0.7 if not set)
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        SetMusicVolume(musicVolume);
        
        // Load SFX volume (default to 0.7 if not set)
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        SetSFXVolume(sfxVolume);
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}